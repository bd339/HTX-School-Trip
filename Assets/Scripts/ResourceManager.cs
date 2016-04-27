using UnityEngine;

using System.Collections.Generic;

class ResourceManager {

    private static Dictionary<string, Object> resources = new Dictionary<string, Object> ();

    public static T Load<T>(string resourceName) where T : Object {
        Object resource;
        if (resources.TryGetValue (resourceName, out resource)) {
            return (T)resource;
        } else {
            var res = Resources.Load<T> (resourceName);
            resources.Add (resourceName, res);
            return res;
        }
    }
}
