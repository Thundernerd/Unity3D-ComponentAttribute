using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
sealed class ComponentAttribute : Attribute {

    public readonly bool AddComponentIfMissing;
    public readonly bool DisableComponentOnError;

    public ComponentAttribute() {
        AddComponentIfMissing = false;
        DisableComponentOnError = false;
    }

    public ComponentAttribute( bool addComponentIfMissing, bool disableComponentOnError ) {
        AddComponentIfMissing = addComponentIfMissing;
        DisableComponentOnError = disableComponentOnError;
    }

    #region Custom Types
    private struct Container {
        public Type Type;
        public List<FieldInfo> Fields;
        public List<PropertyInfo> Properties;
    }

    private class ComponentLoader : MonoBehaviour {

        private static GameObject loader;

        public static void Inject( EventHandler handler ) {
            loader = new GameObject( "ComponentLoader" );
            loader.AddComponent<ComponentLoader>().OnSceneLoaded = handler;
            loader.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad( loader );
        }

        private event EventHandler OnSceneLoaded;

#if UNITY_5_3
        private int previousCount = 0;
        private int previousLevel = -1;
        private string previousLevelName = "";

        public void Update() {
            var activeLevel = SceneManager.GetActiveScene();
            var currentLevel = activeLevel.buildIndex;
            var currentLevelName = activeLevel.name;
            var currentCount = SceneManager.sceneCount;

            if ( previousCount != currentCount || currentLevel != previousLevel || currentLevelName != previousLevelName ) {
                if ( OnSceneLoaded != null ) {
                    OnSceneLoaded( null, null );
                }
            }

            previousCount = currentCount;
            previousLevel = currentLevel;
            previousLevelName = currentLevelName;
        }
#else
        private int previousLevel = -1;
        private string previousLevelName = "";

        public void Update() {
            var currentLevel = Application.loadedLevel;
            var currentLevelName = Application.loadedLevelName;

            if ( currentLevel != previousLevel || currentLevelName != previousLevelName ) {
                if ( OnSceneLoaded != null ) {
                    OnSceneLoaded( null, null );
                }
            }
        
            previousLevel = currentLevel;
            previousLevelName = currentLevelName;
        }
#endif

    }
#endregion

    private const string MISSING = "Component Loader: Unable to load {0} on {1}";
    private const string MISSING_ADD = "Component Loader: Unable to load {0}, adding it on {1}";
    private const string MISSING_ERROR = "Component Loader: Unable to load {0}, disabling {1} on {2}";
    private const string NO_WRITE = "Component Loader: Unable to write {0} on {1}";
    private const string NO_WRITE_ERROR = "Component Loader: Unable to write {0} on {1}, disabling it on {2}";

    private static List<Container> containers;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        ComponentLoader.Inject( OnLevelLoaded );

        var type = typeof( ComponentAttribute );
        var assembly = type.Assembly;
        var types = assembly.GetTypes();

        containers = new List<Container>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        foreach ( var item in types ) {
            var fields = item.GetFields( flags ).Where( f => f.GetCustomAttributes( type, false ).Length == 1 ).ToList();
            var properties = item.GetProperties( flags ).Where( p => p.GetCustomAttributes( type, false ).Length == 1 ).ToList();

            fields.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );
            properties.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

            if ( fields.Count > 0 || properties.Count > 0 ) {
                containers.Add( new Container() {
                    Type = item,
                    Fields = fields,
                    Properties = properties
                } );
            }
        }
    }

    private static void OnLevelLoaded( object sender, EventArgs e ) {
        var type = typeof( ComponentAttribute );

        foreach ( var container in containers ) {
            var objects = GameObject.FindObjectsOfType( container.Type );

            foreach ( var obj in objects ) {
                var cmp = obj as MonoBehaviour;

                foreach ( var field in container.Fields ) {
                    var value = cmp.GetComponent( field.FieldType );
                    var attribute = field.GetCustomAttributes( type, false )[0] as ComponentAttribute;

                    if ( value == null ) {
                        if ( attribute.AddComponentIfMissing ) {
                            Debug.LogWarningFormat( cmp, MISSING_ADD, field.FieldType.Name, cmp.gameObject.name );
                            value = cmp.gameObject.AddComponent( field.FieldType );
                        } else if ( attribute.DisableComponentOnError ) {
                            Debug.LogErrorFormat( cmp, MISSING_ERROR, field.FieldType.Name, container.Type.Name, cmp.gameObject.name );
                            cmp.enabled = false;
                            goto NextObject;
                        } else {
                            Debug.LogWarningFormat( cmp, MISSING, field.FieldType.Name, cmp.gameObject.name );
                        }
                    }

                    if ( value != null ) {
                        field.SetValue( cmp, value );
                    }
                }

                foreach ( var property in container.Properties ) {
                    var attribute = property.GetCustomAttributes( type, false )[0] as ComponentAttribute;

                    if ( property.CanWrite ) {
                        var value = cmp.GetComponent( property.PropertyType );

                        if ( value == null ) {
                            if ( attribute.AddComponentIfMissing ) {
                                Debug.LogWarningFormat( cmp, MISSING_ADD, property.PropertyType.Name, cmp.gameObject.name );
                                value = cmp.gameObject.AddComponent( property.PropertyType );
                            } else if ( attribute.DisableComponentOnError ) {
                                Debug.LogErrorFormat( cmp, MISSING_ERROR, property.PropertyType.Name, container.Type.Name, cmp.gameObject.name );
                                cmp.enabled = false;
                                goto NextObject;
                            } else {
                                Debug.LogWarningFormat( cmp, MISSING, property.PropertyType.Name, cmp.gameObject.name );
                            }
                        }

                        if ( value != null ) {
                            property.SetValue( cmp, value, null );
                        }
                    } else {
                        if ( attribute.DisableComponentOnError ) {
                            Debug.LogErrorFormat( cmp, NO_WRITE_ERROR, property.Name, container.Type.Name, cmp.gameObject.name );
                            cmp.enabled = false;
                            goto NextObject;
                        } else {
                            Debug.LogWarningFormat( cmp, NO_WRITE, property.Name, container.Type.Name );
                        }
                    }
                }

                NextObject:
                continue;
            }
        }
    }
}