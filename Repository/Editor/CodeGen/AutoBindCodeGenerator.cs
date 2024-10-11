using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;

namespace AutoBindComponent.Editor.CodeGen
{
    internal static class AutoBindCodeGenerator
    {
        struct FieldInfo
        {
            public string TypeName;
            public string FieldName;
        }
    
        struct GenInfo
        {
            public List<string> Namespaces;
            public string SelfNamespace;
            public string ClassName;
            public List<FieldInfo> Fields;
        }

        public static void GenerateScript(string targetScriptPath, Runtime.AutoBindComponent autoBindComponent)
        {
            GenInfo genInfo = GetGenInfo(autoBindComponent);
            string outputPath = targetScriptPath.Replace(".cs", "_Gen.cs");

            AutoBindEditorSettings settings = AutoBindEditorSettings.MustLoad();
            string templateText = settings.template.text;
            string outputText = ScribanGenerateText(templateText, genInfo);
            
            File.WriteAllText(outputPath, outputText);
            AssetDatabase.Refresh();
        }

        private static GenInfo GetGenInfo(Runtime.AutoBindComponent autoBindComponent)
        {
            HashSet<string> namespaceSet = new HashSet<string>();
            List<FieldInfo> fieldInfoList = new List<FieldInfo>();
            
            namespaceSet.Add("UnityEngine");

            foreach (Runtime.AutoBindComponent.Info info in autoBindComponent.Infos)
            {
                namespaceSet.Add(info.Component.GetType().Namespace);
                fieldInfoList.Add(new FieldInfo
                {
                    TypeName = info.Component.GetType().Name,
                    FieldName = info.FieldName
                });
            }

            return new GenInfo
            {
                Namespaces = namespaceSet.ToList(),
                SelfNamespace = autoBindComponent.TargetPartialScript.GetType().Namespace,
                ClassName = autoBindComponent.TargetPartialScript.GetType().Name,
                Fields = fieldInfoList
            };
        }
        
        private static string ScribanGenerateText(string templateText, object data)
        {
            ScriptObject scriptObject = new ScriptObject();
            scriptObject.Import(data);

            TemplateContext context = new TemplateContext();
            context.PushGlobal(scriptObject);

            Template template = Template.Parse(templateText);
            if (template.HasErrors)
            {
                foreach (var error in template.Messages)
                    Debug.LogError(error.ToString());

                throw new Exception("文本生成失败，Scriban 模板解析出错");
            }
            
            return template.Render(context);
        }
    }
}
