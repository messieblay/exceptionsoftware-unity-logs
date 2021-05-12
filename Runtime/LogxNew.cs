using System.Collections.Generic;
using UnityEngine;

public enum UnityLogType
{
    None,
    Log,
    Warning,
    Error
}

public class Entry
{
    public int id;
    public UnityLogType logType;
    public string label;
    public string text;

    public string st;
    public string currentFile;
    public int currentLine;

    public Entry(UnityLogType logType, string label, string text)
    {
        this.logType = logType;
        this.label = label;
        this.text = text;
    }

}
public class LogxNew
{

    [SerializeField] static List<Entry> _lEntrys = new List<Entry>();
    [SerializeField] static int id = int.MinValue;

    public static void Log(string msg, System.Enum label, UnityLogType logtype = UnityLogType.None, bool showInUnityConsole = true)
    {
        Log(msg, label.ToString(), logtype, showInUnityConsole);
    }

    public static void Log(string msg, string label = "Default", UnityLogType logtype = UnityLogType.None, bool showInUnityConsole = true)
    {
        //if (UnityEngine.Debug.isDebugBuild)

        string currentFile = "";
        int currentLine = 0;
        string fullstack = "";

        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        for (int i = 1; i < st.FrameCount; i++)
        {
            if (st.GetFrame(i).GetFileName().Contains("Logx")) continue;
            currentFile = st.GetFrame(i).GetFileName();
            currentLine = st.GetFrame(i).GetFileLineNumber();
        }

        //st.GetFrame(0).
        string labelFormat = $"[{label}]";
        string txt = string.Format($"{labelFormat,-15} {msg}", UnityEngine.StackTraceUtility.ExtractStackTrace());


        Entry entry = new Entry(logtype, label, txt);

        entry.id = id++;
        entry.st = fullstack;
        entry.currentFile = currentFile;
        entry.currentLine = currentLine;

        _lEntrys.Add(entry);
        if (_lEntrys.Count > 300)
        {
            _lEntrys.RemoveAt(0);
        }
        if (onEntrysAdd != null) onEntrysAdd(entry);
        if (onEntrysChanged != null) onEntrysChanged(_lEntrys);

        switch (logtype)
        {
            case UnityLogType.Log:
                UnityEngine.Debug.Log(txt);
                break;
            case UnityLogType.Warning:
                UnityEngine.Debug.LogWarning(txt);
                break;
            case UnityLogType.Error:
                UnityEngine.Debug.LogError(txt);
                break;
        }

    }

    public static System.Action<List<Entry>> onEntrysChanged = null;
    public static System.Action<Entry> onEntrysAdd = null;



}
