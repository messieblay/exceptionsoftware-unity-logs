using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum UnityLogType
{
    None,
    Log,
    Warning,
    Error
}

[System.Serializable]
public class Entry
{
    public int id;
    public UnityLogType logType;
    public string label;
    public string msg;
    public string fulltext;

    public string st;
    public string currentFile;
    public int currentLine;

    public Entry(UnityLogType logType, string label, string msg)
    {
        this.logType = logType;
        this.label = label;
        this.msg = msg;
    }

}
public class Logx
{

    //[SerializeField] static List<Entry> _lEntrys = new List<Entry>();
    [SerializeField] static int id = int.MinValue;
    public static List<Entry> LEntrys => ExceptionSoftware.Logs.ExLogUtility.Settings.entrys;

    public static void Log(System.Enum label, string msg, UnityLogType logtype = UnityLogType.None, bool showInUnityConsole = true) => Log(label.ToString(), msg, logtype, showInUnityConsole);
    public static void Log(string msg, UnityLogType logtype = UnityLogType.None, bool showInUnityConsole = true) => Log("Defult", msg, logtype, showInUnityConsole);

    public static void Log(string label, string msg, UnityLogType logtype = UnityLogType.None, bool showInUnityConsole = true)
    {
        //if (UnityEngine.Debug.isDebugBuild)

        string currentFile = "";
        int currentLine = 0;

        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        for (int i = 1; i < st.FrameCount; i++)
        {
            var frame = st.GetFrame(i);
            if (frame == null) continue;

            var filename = frame.GetFileName();
            if (filename == null) continue;

            if (filename.Contains("Logx.cs")) continue;

            currentFile = st.GetFrame(i).GetFileName();
            currentLine = st.GetFrame(i).GetFileLineNumber();
        }


        string labelFormat = $"<b>[{label}]</b>";
        if (ExceptionSoftware.Logs.ExLogUtility.Settings != null)
        {
            var logtypereg = ExceptionSoftware.Logs.ExLogUtility.Settings.logstypes.Find(s => s.name == label);
            if (logtypereg != null)
            {
                if (logtypereg.color != Color.clear)
                {
                    labelFormat = $"<color=#{ColorUtility.ToHtmlStringRGB(logtypereg.color)}>{labelFormat}</color>";
                }
            }
            else
            {
#if UNITY_EDITOR
                ExceptionSoftware.Logs.ExLogUtility.Settings.logstypes.Add(new ExceptionSoftware.Logs.ExLogSettings.LogsType() { name = label, showing = true });
                UnityEditor.EditorUtility.SetDirty(ExceptionSoftware.Logs.ExLogUtility.Settings);
#endif
            }
        }

        string txt = $"{labelFormat,-15} {msg}";
        Entry entry = new Entry(logtype, label, msg);

        entry.id = id++;
        entry.st = StacktraceWithHyperlinks(UnityEngine.StackTraceUtility.ExtractStackTrace(), 0);
        entry.currentFile = currentFile;
        entry.currentLine = currentLine;
        entry.fulltext = txt + "\n" + entry.st;

        LEntrys.Add(entry);
        if (LEntrys.Count > 1000)
        {
            LEntrys.RemoveAt(0);
        }
        if (onEntrysAdd != null) onEntrysAdd(entry);
        if (onEntrysChanged != null) onEntrysChanged(LEntrys);

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
    public static void Refresh()
    {
        if (onEntrysChanged != null) onEntrysChanged(LEntrys);
    }
    static string StacktraceWithHyperlinks(string stacktraceText, int callstackTextStart)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(stacktraceText.Substring(0, callstackTextStart));
        string[] array = stacktraceText.Substring(callstackTextStart).Split(new string[1]
        {
        "\n"
        }, StringSplitOptions.None);
        for (int i = 0; i < array.Length; i++)
        {
            string text = ") (at ";
            int num = array[i].IndexOf(text, StringComparison.Ordinal);
            if (num > 0)
            {
                num += text.Length;
                if (array[i][num] != '<')
                {
                    string text2 = array[i].Substring(num);
                    int num2 = text2.LastIndexOf(":", StringComparison.Ordinal);
                    if (num2 > 0)
                    {
                        int num3 = text2.LastIndexOf(")", StringComparison.Ordinal);
                        if (num3 > 0)
                        {
                            string text3 = text2.Substring(num2 + 1, num3 - (num2 + 1));
                            string text4 = text2.Substring(0, num2);
                            stringBuilder.Append(array[i].Substring(0, num));
                            stringBuilder.Append("<a href=\"" + text4 + "\" line=\"" + text3 + "\">");
                            stringBuilder.Append(text4 + ":" + text3);
                            stringBuilder.Append("</a>)\n");
                            continue;
                        }
                    }
                }
            }
            stringBuilder.Append(array[i] + "\n");
        }
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }
        return stringBuilder.ToString();
    }


    public static System.Action<List<Entry>> onEntrysChanged = null;
    public static System.Action<Entry> onEntrysAdd = null;



}
