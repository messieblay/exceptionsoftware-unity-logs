using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Logs
{
    [InitializeOnLoad]
    public class ExLogUtilityEditor
    {
        static ExLogSettings _settings = null;
        public static ExLogSettings Asset => _settings;

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
            }
        }

        [MenuItem("Tools/Logs/Settings", priority = 3000)]
        static void SelectAsset()
        {
            LoadAsset();
            Selection.activeObject = _settings;
        }

    }
}
