using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

public enum ELoadComponentsMode {
    Awake,
    Start,
    OnEnable
}

public class ExtendedBehaviour : MonoBehaviour {

    /// <summary>
    /// Determines when the members with the Component attribute should be loaded
    /// </summary>
    public ELoadComponentsMode LoadComponentsMode = ELoadComponentsMode.Awake;

    public virtual void Awake() {
        if ( ( LoadComponentsMode & ELoadComponentsMode.Awake ) == ELoadComponentsMode.Awake ) {
            this.LoadComponents();
        }
    }

    public virtual void Start() {
        if ( ( LoadComponentsMode & ELoadComponentsMode.Start ) == ELoadComponentsMode.Start ) {
            this.LoadComponents();
        }
    }

    public virtual void OnEnable() {
        if ( ( LoadComponentsMode & ELoadComponentsMode.OnEnable ) == ELoadComponentsMode.OnEnable ) {
            this.LoadComponents();
        }
    }
}

public static class MonoBehaviourExtensions {

    public class Members {
        public List<FieldInfo> Fields;
        public List<PropertyInfo> Properties;
    }

    public static Dictionary<Type, Members> TypeMembers = new Dictionary<Type, Members>();

    private const string MISSING_ADD = "Unable to load {0}, adding it on \"{1}\"";
    private const string MISSING_ERROR = "Unable to load {0}, disabling {1} on \"{2}\"";
    private const string NO_WRITE = "Unable to set \"{0}\" on \"{1}\"";
    private const string NO_WRITE_ERROR = NO_WRITE + "; Disabling component";

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
                    Debug.LogWarningFormat( MISSING_ADD, item.FieldType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( item.FieldType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( MISSING_ERROR, item.FieldType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
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
                    Debug.LogWarningFormat( MISSING_ADD, item.PropertyType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( item.PropertyType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( MISSING_ERROR, item.PropertyType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
                }
            }

            if ( component != null ) {
                if ( !item.CanWrite ) {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( NO_WRITE_ERROR, item.Name, behaviour.name );
                        behaviour.enabled = false;
                    } else {
                        Debug.LogErrorFormat( NO_WRITE, item.Name, behaviour.name );
                    }
                } else {
                    item.SetValue( behaviour, component, null );
                }
            }
        }
    }
}
