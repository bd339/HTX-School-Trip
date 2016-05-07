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

        HierarchyManager.Find ("Chapter 1 Button").GetComponent<Button> ().interactable = false;
        HierarchyManager.Find ("Chapter 2 Button").GetComponent<Button> ().interactable = false;
        HierarchyManager.Find ("Chapter 3 Button").GetComponent<Button> ().interactable = false;

        if (PlayerPrefs.HasKey ("PlayerName")) {
            HierarchyManager.Find ("Chapter 1 Button").GetComponent<Button> ().interactable = true;
        }

        if (PlayerPrefs.HasKey ("chapter1")) {
            HierarchyManager.Find ("Chapter 2 Button").GetComponent<Button> ().interactable = true;
        }

        if (PlayerPrefs.HasKey ("chapter2")) {
            HierarchyManager.Find ("Chapter 3 Button").GetComponent<Button> ().interactable = true;
        }
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
            UnityEngine.SceneManagement.SceneManager.LoadScene ("chapter1");
        }
    }

    public void PlayChapter1 () {
        // delete chapter 1 flags
        PlayerPrefs.DeleteKey ("GowsiGlass");
        PlayerPrefs.DeleteKey ("Player ID Card");
        PlayerPrefs.DeleteKey ("Ferry Ticket");
        UnityEngine.SceneManagement.SceneManager.LoadScene ("chapter1");
    }

    public void PlayChapter2 () {
        //delete chapter 2 flags
        //...
        //...
        UnityEngine.SceneManagement.SceneManager.LoadScene ("chapter2");
    }
}
