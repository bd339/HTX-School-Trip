using UnityEngine;

// wrapper for Unity 5.4 beta methods
class HierarchyManager {

    public static GameObject Find (string name) {
        var go = GameObject.Find ("Root").transform.FindChildIncludingDeactivated (name);
        return go != null ? go.gameObject : null;
    }

    public static GameObject Find (string name, Transform t) {
        var go = t.FindChildIncludingDeactivated(name);
        return go != null ? go.gameObject : null;
    }

    public static T [] FindObjectsOfType<T> () where T : Component {
        return GameObject.Find ("Root").transform.GetAllComponentsInChildren<T> ();
    }

    public static T FindObjectOfType<T> () where T : Component {
        var a = FindObjectsOfType<T> ();
        return a.Length > 0 ? a [0] : null;
    }
}
