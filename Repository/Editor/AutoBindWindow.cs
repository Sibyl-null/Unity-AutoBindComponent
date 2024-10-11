using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoBindComponent.Editor
{
    public class AutoBindWindow : OdinEditorWindow
    {
        [MenuItem("GameObject/自动绑定窗口 #a")]
        public static void OpenWindow()
        {
            GetWindow<AutoBindWindow>();
        }
    
        [Serializable]
        private struct Info
        {
            public Object Component;
            public string FieldName;
            public bool IsSelected;

            [Button("重置字段名")]
            private void ResetFieldName()
            {
                AutoBindEditorSettings settings = AutoBindEditorSettings.MustLoad();
                FieldName = settings.GetFieldName(Component);
            }
        }

        [InfoBox("AutoBindEditorSettings 中可定义不同类型字段的默认前缀")]
        [InfoBox("如果存在 AutoBindComponent 组件嵌套的情况，点击 ... 选择期望的组件，默认选择第一个找到的")]
    
        [SerializeField]
        [ValueDropdown(nameof(GetArrOfAutoBinds), AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
        private Runtime.AutoBindComponent _targetAutoBind;
    
        [SerializeField, Space]
        [TableList(AlwaysExpanded = true, HideToolbar = true)]
        private List<Info> _infos = new();

        private GameObject _selectedGo;
    
        protected override void OnEnable()
        {
            base.OnEnable();
            _targetAutoBind = null;
            _infos.Clear();

            _selectedGo = Selection.activeGameObject;
            if (_selectedGo == null)
            {
                Debug.LogError("请先选中 GameObject");
                return;
            }

            Runtime.AutoBindComponent[] autoBinds = GetArrOfAutoBinds();
            _targetAutoBind = autoBinds.Length > 0 ? autoBinds[0] : null;
            if (_targetAutoBind == null)
                return;

            AddGameObject();
            AddComponents();
        }

        private Runtime.AutoBindComponent[] GetArrOfAutoBinds()
        {
            if (Selection.activeGameObject == null)
                return Array.Empty<Runtime.AutoBindComponent>();
        
            return Selection.activeGameObject.GetComponentsInParent<Runtime.AutoBindComponent>(true);
        }

        private void AddGameObject()
        {
            AutoBindEditorSettings settings = AutoBindEditorSettings.MustLoad();
            
            bool isSelected = _targetAutoBind.Contains(_selectedGo);
            string fieldName = isSelected
                ? _targetAutoBind.GetFieldName(_selectedGo)
                : settings.GetFieldName(_selectedGo);
        
            _infos.Add(new Info
            {
                Component = _selectedGo,
                FieldName = fieldName,
                IsSelected = isSelected
            });
        }

        private void AddComponents()
        {
            AutoBindEditorSettings settings = AutoBindEditorSettings.MustLoad();
            
            Component[] components = _selectedGo.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                    continue;
            
                bool isSelected = _targetAutoBind.Contains(component);
                string fieldName = isSelected
                    ? _targetAutoBind.GetFieldName(component)
                    : settings.GetFieldName(component);
            
                _infos.Add(new Info
                {
                    Component = component,
                    FieldName = fieldName,
                    IsSelected = isSelected
                });
            }
        }

        private void Apply()
        {
            foreach (Info info in _infos)
            {
                if (info.Component == null)
                    continue;
            
                if (info.IsSelected)
                    _targetAutoBind.Add(info.Component, info.FieldName);
                else
                    _targetAutoBind.Remove(info.Component);
            }
        
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Button("保存", ButtonSizes.Large)]
        private void Save()
        {
            if (_targetAutoBind == null)
            {
                Debug.LogError("请先选择 AutoBindComponent");
                return;
            }
        
            Apply();
            Close();
        }
    }
}