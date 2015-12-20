using CA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

    private static Dictionary<Type, List<MemberInfo>> TypeMembers = new Dictionary<Type, List<MemberInfo>>();

    private const string MISSING = "Component Loader: Unable to load {0} on {1}";
    private const string MISSING_ADD = "Component Loader: Adding {0} on {1}";
    private const string MISSING_ERROR = "Component Loader: Unable to load {0}, disabling {1} on {2}";

    private const string MISSING_OBJECT = "Component Loader: Unable to find a GameObject named {0}";
    private const string MISSING_OBJECT_ADD = "Component Loader: Adding {0} on {1} for {2}";
    private const string MISSING_OBJECT_ERROR = "Component Loader: Unable to find a GameObject named {0}, disabling {1} on {2}";
    private const string MISSING_OBJECT_CERROR = "Component Loader: Unable to load {0} on {1}, disabling {2} on {3}";

    private const string NO_WRITE = "Component Loader: Unable to write {0} on {1}";
    private const string NO_WRITE_ERROR = "Component Loader: Unable to write {0} on {1}, disabling it on {2}";

    public static void LoadComponents( this MonoBehaviour behaviour ) {
        var bGameObject = behaviour.gameObject;
        var bType = behaviour.GetType();
        var cType = typeof( ComponentAttribute );
        var mType = typeof( Component );
        List<MemberInfo> members;

        if ( TypeMembers.ContainsKey( bType ) ) {
            members = TypeMembers[bType];
        } else {
            members = bType.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( m =>
                    ( m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property )
                    && m.GetMemberType().IsSubclassOf( mType )
                    && m.GetCustomAttributes( cType, true ).Length == 1 ).ToList();

            members.OrderBy( m => m.MemberType ).ThenBy( m => m.Name );
            TypeMembers.Add( bType, members );
        }

        foreach ( var item in members ) {
            var attribute = item.GetCustomAttributes( cType, true )[0] as ComponentAttribute;
            var memberType = item.GetMemberType();

            Component component = null;

            if ( string.IsNullOrEmpty( attribute.GameObject ) ) {
                component = behaviour.GetComponent( memberType );
            } else {
                var gObj = GameObject.Find( attribute.GameObject );
                if ( gObj != null ) {
                    component = gObj.GetComponent( memberType );
                } else {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( bGameObject, MISSING_OBJECT_ERROR, attribute.GameObject, bType.Name, behaviour.name );
                        return;
                    } else {
                        Debug.LogWarningFormat( bGameObject, MISSING_OBJECT, attribute.GameObject );
                        continue;
                    }
                }

                if ( component == null ) {
                    if ( attribute.AddComponentIfMissing ) {
                        Debug.LogWarningFormat( bGameObject, MISSING_OBJECT_ADD, memberType.Name, gObj.name, behaviour.name );
                        component = gObj.AddComponent( memberType );
                    } else if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( bGameObject, MISSING_OBJECT_CERROR, memberType.Name, gObj.name, bType.Name, behaviour.name );
                        behaviour.enabled = false;
                        return;
                    } else {
                        Debug.LogWarningFormat( bGameObject, MISSING, memberType.Name, gObj.name );
                        continue;
                    }
                }
            }

            if ( component == null ) {
                if ( attribute.AddComponentIfMissing ) {
                    Debug.LogWarningFormat( bGameObject, MISSING_ADD, memberType.Name, behaviour.name );
                    component = behaviour.gameObject.AddComponent( memberType );
                } else if ( attribute.DisableComponentOnError ) {
                    Debug.LogErrorFormat( bGameObject, MISSING_ERROR, memberType.Name, bType.Name, behaviour.name );
                    behaviour.enabled = false;
                    return;
                } else {
                    Debug.LogWarningFormat( bGameObject, MISSING, memberType.Name, behaviour.name );
                }

                if ( component != null ) {
                    if ( item.CanWrite() ) {
                        item.SetValue( behaviour, component );
                    } else {
                        if ( attribute.DisableComponentOnError ) {
                            Debug.LogErrorFormat( bGameObject, NO_WRITE_ERROR, item.Name, behaviour.name );
                            behaviour.enabled = false;
                        } else {
                            Debug.LogErrorFormat( bGameObject, NO_WRITE, item.Name, behaviour.name );
                        }
                    }
                }
            } else {
                if ( item.CanWrite() ) {
                    item.SetValue( behaviour, component );
                } else {
                    if ( attribute.DisableComponentOnError ) {
                        Debug.LogErrorFormat( bGameObject, NO_WRITE_ERROR, item.Name, behaviour.name );
                        behaviour.enabled = false;
                    } else {
                        Debug.LogErrorFormat( bGameObject, NO_WRITE, item.Name, behaviour.name );
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
    public readonly string GameObject;

    public ComponentAttribute() {
        AddComponentIfMissing = false;
        DisableComponentOnError = false;
        GameObject = "";
    }

    public ComponentAttribute( bool addComponentIfMissing ) {
        AddComponentIfMissing = addComponentIfMissing;
        DisableComponentOnError = false;
        GameObject = "";
    }

    public ComponentAttribute( bool addComponentIfMissing, bool disableComponentOnError ) {
        AddComponentIfMissing = addComponentIfMissing;
        DisableComponentOnError = disableComponentOnError;
        GameObject = "";
    }

    public ComponentAttribute( string gameObject ) {
        AddComponentIfMissing = false;
        DisableComponentOnError = false;
        GameObject = gameObject;
    }

    public ComponentAttribute( string gameObject, bool addComponentIfMissing, bool disableComponentOnError ) {
        AddComponentIfMissing = addComponentIfMissing;
        DisableComponentOnError = disableComponentOnError;
        GameObject = gameObject;
    }
}