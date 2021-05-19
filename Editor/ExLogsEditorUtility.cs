using UnityEditor;

namespace ExceptionSoftware.Logs
{
    [InitializeOnLoad]
    public class ExLogsEditorUtility
    {
        static ExLogSettings _assets = null;
        public static ExLogSettings Asset => _assets;

        public const string LOGS_PATH = ExConstants.GAME_PATH + "Logs/";

        public const string LOGS_PATH_RESOURCES = LOGS_PATH + "Resources/";

        public const string LOGS_SETTINGS_FILENAME = "ExLogSettings";
        static ExLogsEditorUtility() => LoadAsset();


        static void LoadAsset()
        {
            if (!System.IO.Directory.Exists(LOGS_PATH))
                System.IO.Directory.CreateDirectory(LOGS_PATH);

            if (!System.IO.Directory.Exists(LOGS_PATH_RESOURCES))
                System.IO.Directory.CreateDirectory(LOGS_PATH_RESOURCES);

            if (_assets == null)
            {
                _assets = ExAssets.FindAssetsByType<ExLogSettings>().First();
            }

            if (_assets == null)
            {
                _assets = ExAssets.CreateAsset<ExLogSettings>(LOGS_PATH_RESOURCES, LOGS_SETTINGS_FILENAME);
            }
        }

        [MenuItem("Tools/Logs/Settings", priority = 3000)]
        static void SelectAsset()
        {
            LoadAsset();
            Selection.activeObject = _assets;
        }

    }
}
