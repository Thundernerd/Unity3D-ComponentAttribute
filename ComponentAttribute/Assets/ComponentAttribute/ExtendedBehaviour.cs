using System.Linq;
using System.Reflection;
using UnityEngine;

public class ExtendedBehaviour : MonoBehaviour {

    protected virtual void Awake() {
        var cType = typeof( ComponentAttribute );
        var fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( f => f.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in fields ) {
            var component = this.GetComponent( item.FieldType );
            if ( component != null ) {
                item.SetValue( this, component );
            } else {
                Debug.LogErrorFormat( "Unable to load component of type \"{0}\" for field \"{1}\" on \"{2}\"", item.FieldType.Name, item.Name, name );
            }
        }

        var properties = this.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .Where( p => p.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

        foreach ( var item in properties ) {
            var component = this.GetComponent( item.PropertyType );
            if ( component != null ) {
                if ( !item.CanWrite ) {
                    Debug.LogErrorFormat( "Unable to set the property \"{0}\" on \"{1}\"; Make sure it is writable", item.Name, name );
                } else {
                    item.SetValue( this, component, null );
                }
            } else {
                Debug.LogErrorFormat( "Unable to load component of type \"{0}\" for property \"{1}\" on \"{2}\"", item.PropertyType.Name, item.Name, name );
            }
        }
    }
}
