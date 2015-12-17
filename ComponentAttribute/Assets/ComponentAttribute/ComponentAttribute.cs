using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class MonoBehaviourExtensions {

    public class Members {
        public List<FieldInfo> Fields;
        public List<PropertyInfo> Properties;
    }

    public static Dictionary<Type, Members> TypeMembers = new Dictionary<Type, Members>();

    private const string MISSING = "Component Loader: Unable to load {0} on {1}";
    private const string MISSING_ADD = "Component Loader: Unable to load {0}, adding it on {1}";
    private const string MISSING_ERROR = "Component Loader: Unable to load {0}, disabling {1} on {2}";
    private const string NO_WRITE = "Component Loader: Unable to write {0} on {1}";
    private const string NO_WRITE_ERROR = "Component Loader: Unable to write {0} on {1}, disabling it on {2}";

    public static void LoadComponents( this MonoBehaviour behaviour ) {
        var bType = behaviour.GetType();
        var cType = typeof( ComponentAttribute );
        List<FieldInfo> fields;
        List<PropertyInfo> properties;

        if ( TypeMembers.ContainsKey( bType ) ) {
            var members = TypeMembers[bType];
            fields = members.Fields;
            properties = members.Properties;
        } else {
            fields = behaviour.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( f => f.GetCustomAttributes( cType, true ).Length == 1 ).ToList();
            properties = behaviour.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( p => p.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

            fields.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );
            properties.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

            TypeMembers.Add( bType, new Members() {
                Fields = fields,
                Properties = properties
            } );
        }

        foreach ( var item in fields ) {
            var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;

            var component = behaviour.GetComponent( item.FieldType );
            if ( component == null ) {
                if ( attribute.AddComponentIfMissing ) {
                    Debug.LogWarningFormat( component, MISSING_ADD, item.FieldType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( item.FieldType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( component, MISSING_ERROR, item.FieldType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
                } else {
                    Debug.LogWarningFormat( component, MISSING, item.FieldType.Name, behaviour.name );
                }
            }

            if ( component != null ) {
                item.SetValue( behaviour, component );
            }
        }

        foreach ( var item in properties ) {
            var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;

            var component = behaviour.GetComponent( item.PropertyType );
            if ( component == null ) {
                if ( attribute.AddComponentIfMissing ) {
                    Debug.LogWarningFormat( component, MISSING_ADD, item.PropertyType.Name, bType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( item.PropertyType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( component, MISSING_ERROR, item.PropertyType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
                }
            }

            if ( component != null ) {
                if ( !item.CanWrite ) {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( component, NO_WRITE_ERROR, item.Name, behaviour.name );
                        behaviour.enabled = false;
                    } else {
                        Debug.LogErrorFormat( component, NO_WRITE, item.Name, behaviour.name );
                    }
                } else {
                    item.SetValue( behaviour, component, null );
                }
            }
        }
    }
}

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
}