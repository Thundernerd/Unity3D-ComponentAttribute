using UnityEngine;

public class ExtensionTest : MonoBehaviour {

    [Component]
    public Transform Transform;

    [Component]
    protected Light Light { get { return specialLight; } }
    private Light specialLight;

    [Component( true )]
    public BoxCollider2D BoxCollider { get; set; }

    void Start() {
        // You have to call LoadComponents with this. for it to work
        this.LoadComponents();
    }

    void Update() {

    }
}
