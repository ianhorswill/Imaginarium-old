using UnityEngine;

public static class ExtensionMethods
{
    public static void DestroyAllChildren(this Transform root) {
        int childCount = root.childCount;
        for (int i=childCount-1; i>=0; i--) {
            Object.Destroy(root.GetChild(i).gameObject);
        }
    }
}

