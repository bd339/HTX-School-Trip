using UnityEngine;

[SerializeAll]
class Evidence : MonoBehaviour {

    [TextArea (0, 100)]
    [DoNotSerialize]
    public string onCollectScript;

    public string title;
    public string description;

    public Sprite thumbnail;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }
}
