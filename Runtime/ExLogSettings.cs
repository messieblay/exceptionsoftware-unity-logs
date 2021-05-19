using System.Collections.Generic;
using UnityEngine;

namespace ExceptionSoftware.Logs
{
    [System.Serializable]
    public class ExLogSettings : ScriptableObject
    {
        [SerializeField] public List<LogsType> logstypes = new List<LogsType>();

        [SerializeField] public List<Entry> entrys = new List<Entry>();

        [System.Serializable]
        public class LogsType
        {
            public string name;
            public Color color = Color.clear;
            public bool showing = true;
        }
    }
}
