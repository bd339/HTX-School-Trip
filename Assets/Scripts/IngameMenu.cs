using UnityEngine;
using UnityEngine.UI;

class IngameMenu : MonoBehaviour {

    private bool startMusicOver;

    // Use this for initialization
    void Start () {
        enabled = false;
        StartCoroutine (Hide ());
    }

    private void UpdateText () {
        var saveText = HierarchyManager.Find ("Remaining Save Slots Text").GetComponent<Text> ();
        int remainingSaves = LevelSerializer.MaxGames - LevelSerializer.SavedGames [Player.Data.playerName].Count;
        saveText.text = remainingSaves + " saves left before oldest is overwritten on next save";
    }

    private System.Collections.IEnumerator Hide () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        if (!Player.Data.wasDeserialized) {
            gameObject.GetComponent<Image> ().enabled = false;
            HierarchyManager.Find ("Resume Play Button").       SetActive (false);
            HierarchyManager.Find ("Main Menu Button"  ).       SetActive (false);
            HierarchyManager.Find ("Save Button"       ).       SetActive (false);
            HierarchyManager.Find ("Load Menu Button"  ).       SetActive (false);
            HierarchyManager.Find ("Remaining Save Slots Text").SetActive (false);
            HierarchyManager.Find ("Inventory Button"  ).       SetActive (false);
            HierarchyManager.Find ("Map Button"        ).       SetActive (false);
        } else {
            startMusicOver = true;
            UpdateText ();
        }

        enabled = true;
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown (KeyCode.Escape)) {
            if (HierarchyManager.Find ("Load Menu").activeInHierarchy) {
                ToggleLoadMenu ();
            } else if (HierarchyManager.Find ("Inventory Menu").activeInHierarchy) {
                ToggleInventoryMenu ();
            } else if (HierarchyManager.Find ("Map Menu").activeInHierarchy) {
                ToggleMapMenu ();
            } else {
                ToggleMenu ();
            }
        }
    }

    public void ToggleMenu () {
        Player.Data.gamePaused = !Player.Data.gamePaused;
        gameObject.GetComponent<Image> ().enabled = !gameObject.GetComponent<Image> ().enabled;
        HierarchyManager.Find ("Resume Play Button").       SetActive (!HierarchyManager.Find ("Resume Play Button").       activeInHierarchy);
        HierarchyManager.Find ("Main Menu Button"  ).       SetActive (!HierarchyManager.Find ("Main Menu Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Save Button"       ).       SetActive (!HierarchyManager.Find ("Save Button"       ).       activeInHierarchy);
        HierarchyManager.Find ("Load Menu Button"  ).       SetActive (!HierarchyManager.Find ("Load Menu Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Remaining Save Slots Text").SetActive (!HierarchyManager.Find ("Remaining Save Slots Text").activeInHierarchy);
        HierarchyManager.Find ("Inventory Button"  ).       SetActive (!HierarchyManager.Find ("Inventory Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Map Button"        ).       SetActive (!HierarchyManager.Find ("Map Button"        ).       activeInHierarchy);

        Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);

        UpdateText ();

        foreach (Hotspot h in HierarchyManager.FindObjectsOfType<Hotspot> ()) {
            h.gameObject.SetActive (!Player.Data.gamePaused);
        }

        if (Player.Data.gamePaused) {
            Debug.Log (HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().isPlaying);
            HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().Pause ();
            Debug.Log (HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().isPlaying);
            HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Pause ();
        } else {
            if (startMusicOver) {
                HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().Play ();
                startMusicOver = false;
            } else {
                Debug.Log (HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().isPlaying);
                HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().UnPause ();
                Debug.Log (HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().isPlaying);
                HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().UnPause ();
            }
        }
    }

    public void GoToMainMenu () {
        UnityEngine.SceneManagement.SceneManager.LoadScene ("main menu");
    }

    public void Save () {
        LevelSerializer.PlayerName = Player.Data.playerName;
        LevelSerializer.SaveGame (Player.Data.location);

        UpdateText ();
    }

    public void ToggleLoadMenu () {
        HierarchyManager.Find ("Load Menu").SetActive (!HierarchyManager.Find ("Load Menu").activeInHierarchy);
        gameObject.GetComponent<Image> ().enabled = !gameObject.GetComponent<Image> ().enabled;
        HierarchyManager.Find ("Resume Play Button"       ).SetActive (!HierarchyManager.Find ("Resume Play Button").       activeInHierarchy);
        HierarchyManager.Find ("Main Menu Button"         ).SetActive (!HierarchyManager.Find ("Main Menu Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Save Button"              ).SetActive (!HierarchyManager.Find ("Save Button"       ).       activeInHierarchy);
        HierarchyManager.Find ("Load Menu Button"         ).SetActive (!HierarchyManager.Find ("Load Menu Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Remaining Save Slots Text").SetActive (!HierarchyManager.Find ("Remaining Save Slots Text").activeInHierarchy);
        HierarchyManager.Find ("Inventory Button"         ).SetActive (!HierarchyManager.Find ("Inventory Button"  ).       activeInHierarchy);
        HierarchyManager.Find ("Map Button"               ).SetActive (!HierarchyManager.Find ("Map Button"        ).       activeInHierarchy);

        var dropdown = HierarchyManager.Find ("Load Dropdown").GetComponent<Dropdown> ();
        dropdown.ClearOptions ();
        HierarchyManager.Find ("Load Button").GetComponent<Button> ().interactable = false;

        foreach (var game in LevelSerializer.SavedGames [Player.Data.playerName]) {
            dropdown.options.Add (new Dropdown.OptionData () { text = game.Name + " - " + game.When.ToShortDateString () + ' ' + game.When.ToLongTimeString () });
            HierarchyManager.Find ("Load Button").GetComponent<Button> ().interactable = true;
        }
        dropdown.RefreshShownValue ();
    }

    public void Load () {
        var dropdown = HierarchyManager.Find ("Load Dropdown").GetComponent<Dropdown> ();
        LevelSerializer.SavedGames [Player.Data.playerName] [dropdown.value].Load ();
    }

    public void ToggleInventoryMenu () {
        HierarchyManager.Find ("Inventory Menu").SetActive (!HierarchyManager.Find ("Inventory Menu").activeInHierarchy);
        gameObject.GetComponent<Image> ().enabled = !gameObject.GetComponent<Image> ().enabled;
        HierarchyManager.Find ("Resume Play Button").SetActive (!HierarchyManager.Find ("Resume Play Button").activeInHierarchy);
        HierarchyManager.Find ("Main Menu Button").SetActive (!HierarchyManager.Find ("Main Menu Button").activeInHierarchy);
        HierarchyManager.Find ("Save Button").SetActive (!HierarchyManager.Find ("Save Button").activeInHierarchy);
        HierarchyManager.Find ("Load Menu Button").SetActive (!HierarchyManager.Find ("Load Menu Button").activeInHierarchy);
        HierarchyManager.Find ("Remaining Save Slots Text").SetActive (!HierarchyManager.Find ("Remaining Save Slots Text").activeInHierarchy);
        HierarchyManager.Find ("Inventory Button").SetActive (!HierarchyManager.Find ("Inventory Button").activeInHierarchy);
        HierarchyManager.Find ("Map Button").SetActive (!HierarchyManager.Find ("Map Button").activeInHierarchy);

        if (!HierarchyManager.Find ("Inventory Menu").activeInHierarchy) {
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().sprite = null;
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().color = Color.clear;
            HierarchyManager.Find ("Preview Text").GetComponent<Text> ().text = "";
        }
    }

    public void ToggleMapMenu () {
        HierarchyManager.Find ("Map Menu").SetActive (!HierarchyManager.Find ("Map Menu").activeInHierarchy);
    }
}
