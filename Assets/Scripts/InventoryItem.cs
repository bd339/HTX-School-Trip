using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class InventoryItem : MonoBehaviour, IPointerClickHandler {

    public string description;
    public string flag;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void OnPointerClick (PointerEventData d) {
        if (GetComponent<Image>().enabled) {
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().color = Color.white;
            HierarchyManager.Find ("Inventory Preview").GetComponent<Image> ().sprite = GetComponent<Image> ().sprite;
            HierarchyManager.Find ("Preview Text").GetComponent<Text> ().text = description;
        }
    }
}
