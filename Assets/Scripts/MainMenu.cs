using UnityEngine;
using UnityEngine.UI;

class MainMenu : MonoBehaviour {

    public void ShowNewGameMenu () {
        HierarchyManager.Find ("New Game Menu").SetActive (true);
        HierarchyManager.Find ("Main Menu").SetActive (false);
    }

    public void ShowLoadMenu () {
        HierarchyManager.Find ("Load Menu").SetActive (true);
        HierarchyManager.Find ("Main Menu").SetActive (false);

        var sDropdown = HierarchyManager.Find ("Save Dropdown").GetComponent<Dropdown> ();
        var cDropdown = HierarchyManager.Find ("Character Dropdown").GetComponent<Dropdown> ();

        cDropdown.ClearOptions ();
        sDropdown.ClearOptions ();

        sDropdown.interactable = false;
        HierarchyManager.Find ("Resume Game Button").GetComponent<Button> ().interactable = false;

        foreach (var character in LevelSerializer.SavedGames.Keys) {
            cDropdown.options.Add (new Dropdown.OptionData () { text = character });
            SelectCharacter ();
        }
        cDropdown.RefreshShownValue ();
    }

    public void SelectCharacter () {
        HierarchyManager.Find ("Resume Game Button").GetComponent<Button> ().interactable = false;

        var cDropdown = HierarchyManager.Find ("Character Dropdown").GetComponent<Dropdown> ();
        var sDropdown = HierarchyManager.Find ("Save Dropdown").GetComponent<Dropdown> ();
        sDropdown.ClearOptions ();
        sDropdown.interactable = true;

        var character = cDropdown.options [cDropdown.value].text;
        foreach (var game in LevelSerializer.SavedGames [character]) {
            sDropdown.options.Add (new Dropdown.OptionData () { text = game.Name + " - " + game.When.ToShortDateString () + ' ' + game.When.ToLongTimeString () });
            HierarchyManager.Find ("Resume Game Button").GetComponent<Button> ().interactable = true;
        }
        sDropdown.RefreshShownValue ();
    }

    public void ResumeGame () {
        var cDropdown = HierarchyManager.Find ("Character Dropdown").GetComponent<Dropdown> ();
        var sDropdown = HierarchyManager.Find ("Save Dropdown").GetComponent<Dropdown> ();
        var character = cDropdown.options [cDropdown.value].text;
        LevelSerializer.SavedGames [character] [sDropdown.value].Load ();
    }

    public void ExitGame () {
        Application.Quit ();
    }

    public void ShowMainMenu () {
        HierarchyManager.Find ("Main Menu"    ).SetActive (true);
        HierarchyManager.Find ("New Game Menu").SetActive (false);
        HierarchyManager.Find ("Load Menu"    ).SetActive (false);
    }

    public void PlayGame () {
        string pName = HierarchyManager.Find ("Player Name Field").GetComponent<InputField> ().text.Trim ();

        if (pName.Length > 0) {
            LevelSerializer.PlayerName = pName;
            UnityEngine.SceneManagement.SceneManager.LoadScene ("test");
        }
    }
}
