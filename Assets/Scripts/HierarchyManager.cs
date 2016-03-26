using UnityEngine;

class HierarchyManager {

	public static GameObject FindChild (string name) {
		return FindChild (name, GameObject.Find ("Root").transform);
	}

	public static GameObject FindChild (string name, Transform t) {
		for (int i = 0; i < t.childCount; i++) {
			if (t.GetChild (i).name.Equals (name)) {
				return t.GetChild (i).gameObject;
			} else {
				GameObject child = FindChild (name, t.GetChild (i));

				if (child != null) {
					return child;
				}
			}
		}

		return null;
	}
}
