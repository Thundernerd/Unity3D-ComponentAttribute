using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using CA;

namespace CA {
    public static class MemberInfoExtensions {

        public static bool CanWrite( this MemberInfo info ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    return true;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    return p.CanWrite;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return false;
            }
        }

        public static Type GetMemberType( this MemberInfo info ) {
            switch ( info.MemberType ) {
                case MemberTypes.Event:
                    var e = info as EventInfo;
                    return e.EventHandlerType;
                case MemberTypes.Field:
                    var f = info as FieldInfo;
                    return f.FieldType;
                case MemberTypes.Method:
                    var m = info as MethodInfo;
                    return m.ReturnType;
                case MemberTypes.Property:
                    var p = info as PropertyInfo;
                    return p.PropertyType;
                case MemberTypes.Constructor:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return null;
            }
        }

        public static void SetValue( this MemberInfo info, object obj, object value ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    var f = ( info as FieldInfo );
                    f.SetValue( obj, value );
                    break;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    p.SetValue( obj, value, null );
                    break;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    break;
            }
        }
    }
}

public static class CAExtensions {

    public static Dictionary<Type, List<MemberInfo>> TypeMembers = new Dictionary<Type, List<MemberInfo>>();

    private const string MISSING = "Component Loader: Unable to load {0} on {1}";
    private const string MISSING_ADD = "Component Loader: Unable to load {0}, adding it on {1}";
    private const string MISSING_ERROR = "Component Loader: Unable to load {0}, disabling {1} on {2}";
    private const string NO_WRITE = "Component Loader: Unable to write {0} on {1}";
    private const string NO_WRITE_ERROR = "Component Loader: Unable to write {0} on {1}, disabling it on {2}";

    public static void LoadComponents( this MonoBehaviour behaviour ) {
        var bType = behaviour.GetType();
        var cType = typeof( ComponentAttribute );
        List<MemberInfo> members;

        if ( TypeMembers.ContainsKey( bType ) ) {
            members = TypeMembers[bType];
        } else {
            members = bType.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( m => m.GetCustomAttributes( cType, true ).Length == 1 ).ToList();
            members.OrderBy( m => m.MemberType ).ThenBy( m => m.Name );
            TypeMembers.Add( bType, members );
        }

        foreach ( var item in members ) {
            var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;
            var memberType = item.GetMemberType();

            var component = behaviour.GetComponent( memberType );
            if ( component == null ) {
                if ( attribute.AddComponentIfMissing ) {
                    Debug.LogWarningFormat( component, MISSING_ADD, memberType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( memberType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( component, MISSING_ERROR, memberType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
                } else {
                    Debug.LogWarningFormat( component, MISSING, memberType.Name, behaviour.name );
                }

                if ( component != null ) {
                    if ( item.CanWrite() ) {
                        item.SetValue( behaviour, component );
                    } else {
                        if ( attribute.DisableComponentOnError ) {
                            Debug.LogErrorFormat( component, NO_WRITE_ERROR, item.Name, behaviour.name );
                            behaviour.enabled = false;
                        } else {
                            Debug.LogErrorFormat( component, NO_WRITE, item.Name, behaviour.name );
                        }
                    }
                }
            } else {
                if ( item.CanWrite() ) {
                    item.SetValue( behaviour, component );
                } else {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( component, NO_WRITE_ERROR, item.Name, behaviour.name );
                        behaviour.enabled = false;
                    } else {
                        Debug.LogErrorFormat( component, NO_WRITE, item.Name, behaviour.name );
                    }
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