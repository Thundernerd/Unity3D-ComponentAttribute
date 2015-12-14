using UnityEngine;
using System.Collections;

public class FieldsExample : MonoBehaviour {

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
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
