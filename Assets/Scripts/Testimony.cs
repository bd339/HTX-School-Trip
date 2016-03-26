using UnityEngine;

[SerializeAll]
class Testimony : MonoBehaviour {

    [TextArea (0, 100)]
    [DoNotSerialize]
    public string onObtainScript;

    public string title;
    public string statement;
    public string description;

    public Sprite thumbnail;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }
}
