using UnityEngine;
using UnityEngine.UI;

class MainMenu : MonoBehaviour {

    public void ShowNewGameMenu () {
        HierarchyManager.Find ("New Game Menu").SetActive (true);
        HierarchyManager.Find ("Main Menu").SetActive (false);
    }

    public void ShowChaptersMenu () {
        HierarchyManager.Find ("Chapters Menu").SetActive (true);
        HierarchyManager.Find ("Main Menu").SetActive (false);
    }

    public void ExitGame () {
        Application.Quit ();
    }

    public void ShowMainMenu () {
        HierarchyManager.Find ("Main Menu"    ).SetActive (true);
        HierarchyManager.Find ("New Game Menu").SetActive (false);
        HierarchyManager.Find ("Chapters Menu").SetActive (false);
    }

    public void PlayGame () {
        string pName = HierarchyManager.Find ("Player Name Field").GetComponent<InputField> ().text.Trim ();

        if (pName.Length > 0) {
            PlayerPrefs.DeleteAll ();
            PlayerPrefs.SetString ("PlayerName", pName);
            UnityEngine.SceneManagement.SceneManager.LoadScene ("test");
        }
    }
}
