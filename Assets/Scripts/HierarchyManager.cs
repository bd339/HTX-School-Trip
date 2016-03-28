using UnityEngine;

// wrapper for Unity 5.4 beta methods
class HierarchyManager {

    public static GameObject Find (string name) {
        return GameObject.Find ("Root").transform.FindChildIncludingDeactivated (name).gameObject;
    }

    public static GameObject Find (string name, Transform t) {
        return t.FindChildIncludingDeactivated (name).gameObject;
    }

    public static T [] FindObjectsOfType<T> () where T : Component {
        return GameObject.Find ("Root").transform.GetAllComponentsInChildren<T> ();
    }

    public static T FindObjectOfType<T> () where T : Component {
        var a = FindObjectsOfType<T> ();
        return a.Length > 0 ? a [0] : null;
    }
}
