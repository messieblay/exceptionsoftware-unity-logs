using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Logs
{

    [System.Flags]
    public enum LogxEnumT
    {
        None = 0,
        InjectorCore = 1 << 0,
        GamePlay = 1 << 1,
        Audio = 1 << 2,
        All = ~0
    }

    [CustomEditor(typeof(ExLogSettings))]
    public class ExLogSettingsEditor : Editor
    {
        LogxEnumT enumflag;
        public override void OnInspectorGUI()
        {
            //EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Ayuda\nmas ayuda", EditorStyles.helpBox);
            //EditorGUILayout.EndVertical();
            base.DrawDefaultInspector();
            if (GUILayout.Button("Update"))
            {
                (target as ExLogSettings).logstypes.Sort((x, y) => x.name.CompareTo(y.name));

                //CodeFactory.CreateScripts(new EnumFlagsTemplate() { });
                CodeFactory.CodeFactory.CreateScripts(new CodeFactory.EnumFlagsTemplate(ExLogsEditorUtility.LOGS_PATH)
                {
                    className = "LogxEnum",
                    enums = GenerateEnumContent((target as ExLogSettings).logstypes.Select(s => s.name).ToList())
                });


            }

            enumflag = (LogxEnumT)EditorGUILayout.EnumFlagsField(enumflag);
        }

        string[] GenerateEnumContent(List<string> sceneList)
        {
            sceneList = sceneList.OrderBy(s => s).ToList();
            List<string> content = new List<string>();
            if (sceneList.Count > 0)
            {
                for (int i = 0; i < sceneList.Count; i++)
                {
                    content.Add(ValidName(sceneList[i]));
                }
            }
            return content.ToArray();

            string ValidName(string sceneName)
            {
                return sceneName.Replace(" ", "");
            }
        }
    }
}
