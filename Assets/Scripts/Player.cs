using UnityEngine;

[SerializeAll]
class Player : MonoBehaviour {

    [DoNotSerialize]
    private static Player pData;
    [DoNotSerialize]
    public static Player Data {
        get {
            if (pData == null) {
                pData = FindObjectOfType<Player> ();
            }

            return pData;
        }
    }

    // Use this for initialization
    void Start () {
        pData = FindObjectOfType<Player> ();

        LevelLoader.LoadData += MarkAsDeserialized;
    }

    private void MarkAsDeserialized (GameObject gameObject, ref bool cancel) {
        wasDeserialized = true;
    }

    [DoNotSerialize]
    public bool wasDeserialized;

    public bool gamePaused;
}
