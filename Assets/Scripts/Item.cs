using UnityEngine;

[SerializeAll]
class Item : MonoBehaviour {

    [TextArea (0, 100)]
    [DoNotSerialize]
    public string onCollectScript;

    new public string name;
    public string description;

    public Sprite thumbnail;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }
}
