using UnityEngine;

using System.Collections.Generic;

[SerializeAll]
class Player : MonoBehaviour {

    [DoNotSerialize]
    private static Player pData;
    [DoNotSerialize]
    public static Player Data {
        get {
            if (pData == null) {
                pData = HierarchyManager.FindObjectOfType<Player> ();
            }

            return pData;
        }
    }

    // Use this for initialization
    void Start () {
        LevelLoader.LoadData += MarkAsDeserialized;

        StartCoroutine (StoreName ());
    }

    private void MarkAsDeserialized (GameObject gameObject, ref bool cancel) {
        wasDeserialized = true;
    }

    private System.Collections.IEnumerator StoreName () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        if (!wasDeserialized) {
            playerName = LevelSerializer.PlayerName;
        }
    }

    [DoNotSerialize]
    public bool wasDeserialized;

    public bool gamePaused;

    public string playerName;

    public string location = "Unknown";

    public string backgroundTexture = "";

    public Dictionary<string, string> spriteMap = new Dictionary<string, string> ();

    public List<string> flags = new List<string> ();
}
