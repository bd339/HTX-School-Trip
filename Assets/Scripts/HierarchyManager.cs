﻿using UnityEngine;

class HierarchyManager {

    public static T [] FindObjectsOfType<T> () where T : Component {
        return GameObject.Find ("Root").GetComponentsInChildren<T> (true);
    }

    public static T FindObjectOfType<T> () where T : Component {
        var a = FindObjectsOfType<T> ();
        return a.Length > 0 ? a [0] : null;
    }

    public static GameObject Find (string name) {
        return Find (name, GameObject.Find ("Root").transform);
    }

    public static GameObject Find (string name, Transform t) {
        for (int i = 0; i < t.childCount; i++) {
            if (t.GetChild (i).name.Equals (name)) {
                return t.GetChild (i).gameObject;
            } else {
                GameObject child = Find (name, t.GetChild (i));

                if (child != null) {
                    return child;
                }
            }
        }

        return null;
    }
}
