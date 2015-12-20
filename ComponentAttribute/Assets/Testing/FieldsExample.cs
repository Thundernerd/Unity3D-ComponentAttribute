using UnityEngine;
using System.Collections;

public class FieldsExample : MonoBehaviour {

    [Component( "Main Camera" )]
    public Camera Camera1;
    [Component( "Light" )]
    public Camera Camera2;
    [Component( "NotExisting GameObject" )]
    public Camera Camera3;

    [Component]
    public BoxCollider2D Collider1;
    [Component( true, false )]
    public BoxCollider2D Collider2;

    [Component]
    private Transform transform1;
    [Component( true, false )]
    private Transform transform2;

    [Component]
    public TerrainCollider Terrain1;
    [Component( false, true )]
    public TerrainCollider Terrain2;

    // Use this for initialization
    void Start () {
        this.LoadComponents();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
