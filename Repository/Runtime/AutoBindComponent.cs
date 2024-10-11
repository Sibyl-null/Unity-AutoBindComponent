using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AutoBindComponent.Runtime
{
    public sealed partial class AutoBindComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        [System.Serializable]
        public struct Info
        {
            public Object Component;
            public string FieldName;
        }
        
        [SerializeField, LabelText("目标生成脚本")]
        [ValueDropdown(nameof(GetListOfMonoBehaviours), AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
        private MonoBehaviour _targetPartialScript;
        
        [SerializeField]
        private bool _hideRefComponents = true;
        
        [SerializeField, HideIf(nameof(_hideRefComponents)), DisableInEditorMode]
        private List<Info> _infos = new();
        
        public MonoBehaviour TargetPartialScript => _targetPartialScript;
        public List<Info> Infos => _infos;

        public bool Contains(Object component)
        {
            foreach (Info info in _infos)
            {
                if (info.Component == component)
                    return true;
            }

            return false;
        }

        public bool ContainsAnyRefInGo(GameObject go)
        {
            foreach (Info info in _infos)
            {
                if (info.Component == go)
                    return true;

                Component comp = info.Component as Component;
                if (comp != null && comp.gameObject == go)
                    return true;
            }
            
            return false;
        }

        public void Add(Object component, string fieldName)
        {
            for (int i = 0; i < _infos.Count; i++)
            {
                if (_infos[i].Component == component)
                {
                    Info info = _infos[i];
                    info.FieldName = fieldName;
                    _infos[i] = info;
                    return;
                }
            }
            
            _infos.Add(new Info()
            {
                Component = component,
                FieldName = fieldName
            });
        }

        public void Remove(Object component)
        {
            if (!Contains(component))
                return;

            int index = _infos.FindIndex(x => x.Component == component);
            _infos.RemoveAt(index);
        }

        public string GetFieldName(Object component)
        {
            if (!Contains(component))
                return null;
            
            int index = _infos.FindIndex(x => x.Component == component);
            return _infos[index].FieldName;
        }
        
        private List<MonoBehaviour> GetListOfMonoBehaviours()
        {
            List<MonoBehaviour> list = new List<MonoBehaviour>();
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour.GetType() == typeof(AutoBindComponent))
                    continue;
                
                string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(
                    UnityEditor.MonoScript.FromMonoBehaviour(behaviour));
                
                if (string.IsNullOrEmpty(scriptPath) || !scriptPath.StartsWith("Assets/"))
                    continue;
                
                list.Add(behaviour);
            }
            
            return list;
        }
#endif
    }
}