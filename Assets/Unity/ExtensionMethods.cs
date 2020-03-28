using UnityEngine;

public static class ExtensionMethods
{
    public static void DestroyAllChildren(this Transform root) {
        int childCount = root.childCount;
        for (int i=0; i<childCount; i++) {
            GameObject.Destroy(root.GetChild(0).gameObject);
        }
    }
}

