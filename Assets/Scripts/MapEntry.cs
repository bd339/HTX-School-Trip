using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class MapEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public int state;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void OnPointerClick (PointerEventData eventData) {
        IngameMenu.Menu.ToggleMapMenu ();

        CooperScript.Engine.CommandIndex = 0;
        CooperScript.Engine.StateId = state;
    }

    public void OnPointerEnter (PointerEventData eventData) {
        GetComponent<Image> ().color = new Color (1f, 1f, 1f, 0.5f);
    }

    public void OnPointerExit (PointerEventData eventData) {
        GetComponent<Image> ().color = Color.clear;
    }
}
