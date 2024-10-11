using AutoBindComponent.Editor.CodeGen;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace AutoBindComponent.Editor
{
    [CustomEditor(typeof(Runtime.AutoBindComponent))]
    public class AutoBindComponentDrawer : OdinEditor
    {
        private Runtime.AutoBindComponent Target => (Runtime.AutoBindComponent)target;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("生成代码"))
            {
                if (!CheckValid(out string scriptPath))
                    return;

                ClearNullPointers();
                AutoBindCodeGenerator.GenerateScript(scriptPath, Target);
            }

            if (GUILayout.Button("自动绑定"))
            {
                if (!CheckValid(out _))
                    return;
            
                ClearNullPointers();
                Target.AutoBindFields();
            }
        }
        
        private bool CheckValid(out string scriptPath)
        {
            if (Target.TargetPartialScript == null)
            {
                Debug.LogError("请选择正确的目标生成脚本");
                scriptPath = null;
                return false;
            }

            scriptPath = AssetDatabase.GetAssetPath(
                MonoScript.FromMonoBehaviour(Target.TargetPartialScript));
            if (string.IsNullOrEmpty(scriptPath) || !scriptPath.StartsWith("Assets/"))
            {
                Debug.LogError("请选择正确的目标生成脚本");
                scriptPath = null;
                return false;
            }

            return true;
        }
        
        private void ClearNullPointers()
        {
            Target.Infos.RemoveAll(x => x.Component == null);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}