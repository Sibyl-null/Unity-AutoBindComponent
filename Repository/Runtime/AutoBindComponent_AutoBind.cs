#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AutoBindComponent.Runtime
{
    public sealed partial class AutoBindComponent
    {
        public void AutoBindFields()
        {
            Type targetType = _targetPartialScript.GetType();
            
            foreach (Info info in _infos)
                SetFieldValue(info, targetType);
            
            Debug.Log("Auto bind fields completed.");
            EditorUtility.SetDirty(gameObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SetFieldValue(Info info, Type targetType)
        {
            var fieldInfo = targetType.GetField(info.FieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                Debug.LogError("Field not found: " + info.FieldName);
                return;
            }
            
            fieldInfo.SetValue(_targetPartialScript, info.Component);
        }
    }
}
#endif