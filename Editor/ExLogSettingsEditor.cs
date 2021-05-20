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
                ExLogUtilityEditor.CreateLogEnum();

            }

            enumflag = (LogxEnumT)EditorGUILayout.EnumFlagsField(enumflag);
        }

    }
}
