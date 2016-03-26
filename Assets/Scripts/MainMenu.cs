using UnityEngine;

class MainMenu : MonoBehaviour {

    public void ShowNewGameMenu () {
        HierarchyManager.FindChild ("New Game Menu").SetActive (true);
        HierarchyManager.FindChild ("Main Menu").SetActive (false);
    }

    public void ShowLoadMenu () {
        HierarchyManager.FindChild ("Load Menu").SetActive (true);
        HierarchyManager.FindChild ("Main Menu").SetActive (false);
    }

    public void ExitGame () {
        Application.Quit ();
    }

    public void ShowMainMenu () {
        HierarchyManager.FindChild ("Main Menu").SetActive (true);

        if (HierarchyManager.FindChild("New Game Menu").activeInHierarchy) {
            HierarchyManager.FindChild ("New Game Menu").SetActive (false);
        } else if (HierarchyManager.FindChild ("Load Menu").activeInHierarchy) {
            HierarchyManager.FindChild ("Load Menu").SetActive (false);
        }
    }

    public void PlayGame () {
        string pName = HierarchyManager.FindChild ("Player Name Field").GetComponent<UnityEngine.UI.InputField> ().text.Trim ();

        if (pName.Length > 0) {
            LevelSerializer.PlayerName = pName;
            UnityEngine.SceneManagement.SceneManager.LoadScene ("test");
        }
    }
}
