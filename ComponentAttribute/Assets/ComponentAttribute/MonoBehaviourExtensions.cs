using System.Linq;
using System.Reflection;
using UnityEngine;

public static class MonoBehaviourExtensions {

    public static void LoadComponents( this MonoBehaviour behaviour ) {
        var cType = typeof( ComponentAttribute );
        var fields = behaviour.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( f => f.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in fields ) {
            var component = behaviour.GetComponent( item.FieldType );
            if ( component != null ) {
                item.SetValue( behaviour, component );
            } else {
                Debug.LogErrorFormat( "Unable to load component of type \"{0}\" for field \"{1}\" on \"{2}\"", item.FieldType.Name, item.Name, behaviour.name );
            }
        }

        var properties = behaviour.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( p => p.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in properties ) {
            var component = behaviour.GetComponent( item.PropertyType );
            if ( component != null ) {
                if ( !item.CanWrite ) {
                    Debug.LogErrorFormat( "Unable to set the property \"{0}\" on \"{1}\"; Make sure it is writable", item.Name, behaviour.name );
                } else {
                    item.SetValue( behaviour, component, null );
                }
            } else {
                Debug.LogErrorFormat( "Unable to load component of type \"{0}\" for property \"{1}\" on \"{2}\"", item.PropertyType.Name, item.Name, behaviour.name );
            }
        }
    }
}
