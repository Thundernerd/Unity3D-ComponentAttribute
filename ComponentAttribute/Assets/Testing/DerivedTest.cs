using UnityEngine;
using System.Collections;

public class DerivedTest : ExtendedBehaviour {

    [Component]
    public Transform Transform;
    [Component]
    public Light Light;
    [Component]
    public BoxCollider2D Collider;

    [Component]
    public Transform TransformP { get; set; }
    [Component]
    public Light LightP { get; set; }
    [Component]
    public BoxCollider2D ColliderP { get; set; }

    private BoxCollider2D bcollider;
    [Component]
    protected BoxCollider2D BCollider { get { return bcollider; } }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
