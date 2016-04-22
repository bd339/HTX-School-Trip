using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public string description;
    public string flag;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void OnPointerClick (PointerEventData eventData) {
        if (GetComponent<Image> ().enabled) {
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().color = Color.white;
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().sprite = GetComponent<Image> ().sprite;
            HierarchyManager.Find ("Preview Text").GetComponent<Text> ().text = description;
        }
    }

    public void OnPointerEnter (PointerEventData eventData) {
        if (GetComponent<Image> ().enabled) {
            GetComponent<Image> ().color = new Color (1f, 1f, 1f, 0.5f);
        }
    }

    public void OnPointerExit (PointerEventData eventData) {
        if (GetComponent<Image> ().enabled) {
            GetComponent<Image> ().color = Color.white;
        }
    }
}
