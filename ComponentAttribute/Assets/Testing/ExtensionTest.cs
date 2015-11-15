using UnityEngine;
using System.Collections;

public class ExtensionTest : MonoBehaviour {

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

    void Start () {
        // You have to call LoadComponents with this. for it to work
        this.LoadComponents();
	}
	
	void Update () {
	
	}
}
