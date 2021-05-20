using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ExceptionSoftware.Logs.ExLogSettings;

namespace ExceptionSoftware.Logs
{
    [InitializeOnLoad]
    public class ExLogUtilityEditor
    {
        static ExLogSettings _settings = null;
        public static ExLogSettings Settings => _settings;

        public const string LOGS_PATH = ExConstants.GAME_PATH + "Logs/";

        public const string LOGS_PATH_RESOURCES = LOGS_PATH + "Resources/";

        public const string LOGS_SETTINGS_FILENAME = "ExLogSettings";
        static ExLogUtilityEditor() => LoadAsset();


        static void LoadAsset()
        {
            if (!System.IO.Directory.Exists(LOGS_PATH))
                System.IO.Directory.CreateDirectory(LOGS_PATH);

            if (!System.IO.Directory.Exists(LOGS_PATH_RESOURCES))
                System.IO.Directory.CreateDirectory(LOGS_PATH_RESOURCES);

            if (_settings == null)
            {
                _settings = ExAssets.FindAssetsByType<ExLogSettings>().First();
            }


            if (_settings == null)
            {
                _settings = Resources.FindObjectsOfTypeAll<ExLogSettings>().FirstOrDefault();
            }

            if (_settings == null)
            {
                _settings = ExAssets.CreateAsset<ExLogSettings>(LOGS_PATH_RESOURCES, LOGS_SETTINGS_FILENAME);
                CreateBasicLogs();
            }
        }

        [MenuItem("Tools/Logs/Settings", priority = 3000)]
        static void SelectAsset()
        {
            LoadAsset();
            Selection.activeObject = _settings;
        }

        public static void CreateBasicLogs()
        {
            CreateTypeLog("Core", new Color(1, 0, 0));
            CreateTypeLog("Audio", new Color(.8f, 0, 0));
            CreateTypeLog("Input", new Color(.6f, 0, 0));
            CreateTypeLog("Gameplay", new Color(0, 1, 0));
            CreateTypeLog("Loading", new Color(0, 0, 1));
            CreateTypeLog("Saving", new Color(0, 0, 1));
            AssetDatabase.SaveAssets();
            CreateLogEnum();
        }
        public static void CreateTypeLog(string name, Color color)
        {
            LogsType logtype = new LogsType()
            {
                name = name,
                color = color
            };
            Settings.logstypes.Add(logtype);
            EditorUtility.SetDirty(Settings);
        }

        public static void CreateLogEnum()
        {

            Settings.Sort();

            CodeFactory.CodeFactory.CreateScripts(new CodeFactory.EnumFlagsTemplate(ExLogUtilityEditor.LOGS_PATH)
            {
                className = "LogxEnum",
                enums = GenerateEnumContent(Settings.logstypes.Select(s => s.name).ToList())
            });


        }


        static string[] GenerateEnumContent(List<string> sceneList)
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
