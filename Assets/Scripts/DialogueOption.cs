using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class DialogueOption : Text, IPointerEnterHandler, IPointerExitHandler {

    public bool selected;

    public string optionText;

    public int optionStateId;

    public void Select () {
        foreach (DialogueOption o in HierarchyManager.FindObjectsOfType<DialogueOption> ()) {
            if (o.selected) {
                o.Deselect ();
            }
        }

        fontStyle = FontStyle.Bold;
        base.text = '>' + optionText;
        color = new Color (226f / 255f, 18f / 255f, 219f / 255f, 255f);
        GetComponent<Outline> ().effectColor = Color.white;

        selected = true;
    }

    public void Deselect () {
        color = Color.white;
        base.text = optionText;
        fontStyle = FontStyle.Normal;
        GetComponent<Outline> ().effectColor = Color.black;

        selected = false;
    }

    public void OnPointerEnter (PointerEventData eventData) {
        Select ();
    }

    public void OnPointerExit (PointerEventData eventData) {
        Deselect ();
    }
}
