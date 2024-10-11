using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace AutoBindComponent.Editor
{
    public class AutoBindEditorSettings : SerializedScriptableObject
    {
        [InfoBox("如果 Odin 显示 EditorOnly 的错误，可以忽视，不影响使用")]
        [SerializeField, LabelText("代码模板文件"), Required]
        public TextAsset template;
        
        [SerializeField, Space, LabelText("类型默认前缀表")]
        private Dictionary<string, string> _map = new()
        {
            { "GameObject", "_go" },
            { "RectTransform", "_rt" },
            { "Image", "_img" },
            { "Button", "_btn" },
            { "Text", "_txt" },
            { "Canvas", "_can" },
            { "Slider", "_sld" },
            { "CanvasGroup", "_cg" },
            { "InputField", "_input" },
            { "TextMeshProUGUI", "_tmp" },
        };
        
        public string GetFieldName(UnityEngine.Object component)
        {
            string goName = component.name.Replace(" ", "");
            
            Type type = component.GetType();
            if (_map.TryGetValue(type.Name, out string prefix))
            {
                return $"{prefix}{goName}";
            }

            return $"_{type.Name.Substring(0, 3)}{goName}";
        }
        
        
        // ----------------------------------------------------------------------------------
        
        private const string DefaultSavePath = "Assets/Editor/AutoBindEditorSettings.asset";
        private const string DefaultTemplatePath = "Packages/com.sibyl.autobindcomponent/Editor/CodeGen/AutoBindTemplate.txt";

        private void Reset()
        {
            template = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultTemplatePath);
        }

        [MenuItem("Project/AutoBindComponent/Create EditorSettings")]
        public static void CreateAsset()
        {
            if (File.Exists(DefaultSavePath))
            {
                Debug.LogWarning("[UI] 目标路径已存在文件 " + DefaultSavePath);
                return;
            }

            string directoryName = Path.GetDirectoryName(DefaultSavePath);
            if (Directory.Exists(directoryName) == false)
                Directory.CreateDirectory(directoryName);

            AutoBindEditorSettings so = CreateInstance<AutoBindEditorSettings>();
            so.template = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultTemplatePath);
            AssetDatabase.CreateAsset(so, DefaultSavePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(so);
            Debug.Log($"[AutoBind] {DefaultSavePath} 创建成功");
        }

        public static AutoBindEditorSettings MustLoad()
        {
            string guid = AssetDatabase.FindAssets($"t:{nameof(AutoBindEditorSettings)}")[0];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            AutoBindEditorSettings result = AssetDatabase.LoadAssetAtPath<AutoBindEditorSettings>(path);
            if (result == null)
                throw new Exception("Can't find AutoBindEditorSettings");
            
            return result;
        }
    }
}