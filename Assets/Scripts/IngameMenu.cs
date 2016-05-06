using UnityEngine;
using UnityEngine.UI;

class IngameMenu : MonoBehaviour {

    private static IngameMenu menu;
    public static IngameMenu Menu {
        get {
            if (menu == null) {
                menu = HierarchyManager.FindObjectOfType<IngameMenu> ();
            }

            return menu;
        }
    }

    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown (KeyCode.Escape)) {
            Back ();
        }
    }

    public void GoToMainMenu () {
        UnityEngine.SceneManagement.SceneManager.LoadScene ("main menu");
    }

    public void Back () {
        if (HierarchyManager.Find ("Inventory Menu").activeInHierarchy) {
            ToggleInventoryMenu ();
        } else if (HierarchyManager.Find ("Map Menu").activeInHierarchy) {
            ToggleMapMenu ();
        } else {
            ToggleMenu ();
        }
    }

    public void ToggleMenu () {
        CooperScript.Engine.gamePaused = !CooperScript.Engine.gamePaused;

        HierarchyManager.Find ("Back Button").SetActive (!HierarchyManager.Find ("Back Button").activeInHierarchy);
        HierarchyManager.Find ("Map Button").GetComponent<Button> ().interactable = InvestigationControls.Controls.enabled;
        ToggleDefaultUI ();

        Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);

        foreach (Hotspot h in HierarchyManager.FindObjectsOfType<Hotspot> ()) {
            h.gameObject.SetActive (!CooperScript.Engine.gamePaused);
        }

        if (CooperScript.Engine.gamePaused) {
            HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().Pause ();
            HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Pause ();
        } else {
            HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().UnPause ();
            HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().UnPause ();
        }
    }

    public void ToggleInventoryMenu () {
        HierarchyManager.Find ("Inventory Menu").SetActive (!HierarchyManager.Find ("Inventory Menu").activeInHierarchy);
        ToggleDefaultUI ();

        if (!HierarchyManager.Find ("Inventory Menu").activeInHierarchy) {
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().sprite = null;
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().color = Color.clear;
            HierarchyManager.Find ("Preview Text").GetComponent<Text> ().text = "";
        } else {
            foreach (var item in HierarchyManager.FindObjectsOfType<InventoryItem> ()) {
                if (PlayerPrefs.HasKey (item.flag)) {
                    item.GetComponent<Image> ().enabled = true;
                }
            }
        }
    }

    public void ToggleMapMenu () {
        if (InvestigationControls.Controls.enabled) {
            InvestigationControls.Controls.LeaveInvestigationMode ();
        } else {
            InvestigationControls.Controls.EnterInvestigationMode ();
        }

        HierarchyManager.Find ("Map Menu").SetActive (!HierarchyManager.Find ("Map Menu").activeInHierarchy);
        ToggleDefaultUI ();
    }

    private void ToggleDefaultUI () {
        GetComponent<Image> ().enabled = !GetComponent<Image> ().enabled;

        HierarchyManager.Find ("Main Menu Button").SetActive (!HierarchyManager.Find ("Main Menu Button").activeInHierarchy);
        HierarchyManager.Find ("Inventory Button").SetActive (!HierarchyManager.Find ("Inventory Button").activeInHierarchy);
        HierarchyManager.Find ("Map Button").SetActive (!HierarchyManager.Find ("Map Button").activeInHierarchy);
    }
}
