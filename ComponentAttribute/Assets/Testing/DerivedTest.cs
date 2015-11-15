using UnityEngine;

public class DerivedTest : ExtendedBehaviour {

    [Component]
    public Transform Transform;

    [Component]
    protected Light Light { get { return specialLight; } }
    private Light specialLight;

    [Component( true )]
    public BoxCollider2D BoxCollider { get; set; }

    void Start() {

    }

    void Update() {

    }
}
