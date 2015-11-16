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
            var component = behaviour.GetComponent( item.FieldType );
            if ( component != null ) {
                item.SetValue( behaviour, component );
            } else {
                if ( CheckAttribute( behaviour, item, cType,
                    string.Format( "Unable to load {0} on \"{1}\"", item.FieldType.Name, behaviour.name ) ) )
                    return;
            }
        }

        foreach ( var item in properties ) {
            var component = behaviour.GetComponent( item.PropertyType );
            if ( component != null ) {
                if ( !item.CanWrite ) {
                    if ( CheckAttribute( behaviour, item, cType,
                        string.Format( "Unable to set \"{0}\" on \"{1}\"", item.Name, behaviour.name ) ) )
                        return;
                } else {
                    item.SetValue( behaviour, component, null );
                }
            } else {
                if ( CheckAttribute( behaviour, item, cType,
                    string.Format( "Unable to load {0} on \"{1}\"", item.PropertyType.Name, behaviour.name ) ) )
                    return;
            }
        }
    }

    private static bool CheckAttribute( Behaviour behaviour, MemberInfo item, Type cType, string defaultMessage ) {
        var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;
        if ( attribute.DisableComponentOnError ) {
            Debug.LogErrorFormat( "{0}; Disabling \"{1}\" on \"{2}\"", defaultMessage, behaviour.GetType().Name, behaviour.name );
            behaviour.enabled = false;
            return true;
        } else {
            Debug.LogError( defaultMessage );
        }

        return false;
    }
}
