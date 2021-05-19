using System.Linq;
using UnityEngine;

namespace ExceptionSoftware.Logs
{
    public static class ExLogUtility
    {
        static ExLogSettings _settings = null;
        public static ExLogSettings Settings => LoadAsset();
        internal static ExLogSettings LoadAsset()
        {
            if (_settings == null)
            {
                _settings = ExAssets.FindAssetsByType<ExLogSettings>().FirstOrDefault();
            }

            if (_settings == null)
            {
                _settings = Resources.FindObjectsOfTypeAll<ExLogSettings>().FirstOrDefault();
            }

            return _settings;
        }
    }
}
