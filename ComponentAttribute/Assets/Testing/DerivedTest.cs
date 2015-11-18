using UnityEngine;

public class DerivedTest : ExtendedBehaviour {

    [Component]
    public Transform Transform;

    [Component]
    protected Light Light { get { return specialLight; } }
    private Light specialLight;

    [Component( true, false )]
    public BoxCollider2D BoxCollider { get; set; }

    [Component( false, true )]
    private Camera camera;

    public override void Start() {
        base.Start();
    }

    void Update() {

    }
}
