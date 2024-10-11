using UnityEditor;
using UnityEngine;

namespace AutoBindComponent.Editor
{
    public static class AutoBindHierarchy
    {
        private static readonly Color[] Colors =
        {
            Color.yellow,
            Color.blue,
            Color.cyan,
            Color.green,
        };
    
        [InitializeOnLoadMethod]
        private static void Load()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
                return;

            int depth = GetRefDepth(go);
            if (depth < 0)
                return;
        
            Rect r = new Rect(selectionRect)
            {
                x = 34,
                width = 80
            };
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = Colors[Mathf.Min(depth, Colors.Length - 1)]
                }
            };
        
            GUI.Label(r, "★", style);
        }

        private static int GetRefDepth(GameObject go)
        {
            int depth = -1;
            Transform trans = go.transform;

            while (trans != null)
            {
                Runtime.AutoBindComponent autoBind = trans.GetComponent<Runtime.AutoBindComponent>();
                if (autoBind == null)
                {
                    trans = trans.parent;
                }
                else
                {
                    ++depth;
                    trans = trans.parent;

                    if (autoBind.ContainsAnyRefInGo(go))
                        return depth;
                }
            }

            return -1;
        }
    }
}