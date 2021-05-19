using ExceptionSoftware.Logs;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
public class LogxWindow : EditorWindow, IHasCustomMenu
{
    [MenuItem("Tools/Logx", false, 3000)]
    private static void ShowWindow()
    {
        LogxWindow w = GetWindow<LogxWindow>();
        w.titleContent = new GUIContent("Logx");
    }
    private SearchField _searchField;
    static SampleTreeView _treeView;
    private TreeViewState _treeViewState;

    bool isResizing;
    static Entry _selected = null;
    Vector2 _scrollEntry = Vector2.zero;
    GUIStyle entryStyle;
    GUIContent _entryContent;


    /// <summary>
    /// Initialize
    /// </summary>
    private void Init()
    {
        if (_treeViewState == null)
        {
            _treeViewState = new TreeViewState();
        }
        if (_treeView == null)
        {
            _treeView = new SampleTreeView(_treeViewState);
        }
        if (_searchField == null)
        {
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }

        entryStyle = "CN Message";

        Logx.onEntrysChanged -= _treeView.AddItems;
        Logx.onEntrysChanged += _treeView.AddItems;
        Logx.onEntrysAdd -= _treeView.AddItem;
        Logx.onEntrysAdd += _treeView.AddItem;
        Logx.Refresh();
    }

    Rect _rentry;


    void OnEnable()
    {
        EnableClearOnPlay();
    }
    void OnDisable()
    {
        DisableClearOnPlay();
    }


    private void OnGUI()
    {
        Init();
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Clear ", EditorStyles.toolbarButton))
            {
                ClearConsole();
            }
            _clearOnPlay = GUILayout.Toggle(_clearOnPlay, "Clear on Play", EditorStyles.toolbarButton, GUILayout.Height(14));
            GUILayout.FlexibleSpace();

            _treeView.searchString = _searchField.OnToolbarGUI(_treeView.searchString);
            if (GUILayout.Button("Filter", EditorStyles.toolbarButton))
            {
                PopupWindow.Show(_rectFilterPopup, new PopupExample());
            }
            GUILayout.FlexibleSpace();

            if (Event.current.type == EventType.Repaint) _rectFilterPopup = GUILayoutUtility.GetLastRect();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Logx.Refresh();
            }
        }
        var rect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, base.position.height * _resizeFactor);
        _treeView.OnGUI(rect);
        _rresizer = GUILayoutUtility.GetRect(0, float.MaxValue, 0, 3);

        DrawResizer();
        DrawEntry();
        ProcessEvents(Event.current);
    }

    void DrawEntry()
    {
        _scrollEntry = GUILayout.BeginScrollView(_scrollEntry, GUILayout.ExpandWidth(true), GUILayout.Height(base.position.height * (1 - _resizeFactor)));
        if (_selected != null)
        {
            _entryContent = new GUIContent(_selected.fulltext);
            float num9 = entryStyle.CalcHeight(_entryContent, position.width);
            EditorGUILayout.SelectableLabel(_entryContent.text, entryStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MinHeight(num9 + 10f));
        }
        GUILayout.EndScrollView();
    }

    #region Options


    void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
    {
        GUIContent content = new GUIContent("Log Entry/1 Lines");
        menu.AddItem(content, false, LogEntry1);

        content = new GUIContent("Log Entry/2 Lines");
        menu.AddItem(content, false, LogEntry2);

        content = new GUIContent("Log Entry/3 Lines");
        menu.AddItem(content, false, LogEntry3);

        content = new GUIContent("Log Entry/4 Lines");
        menu.AddItem(content, false, LogEntry4);

        content = new GUIContent("Log Entry/5 Lines");
        menu.AddItem(content, false, LogEntry5);

        content = new GUIContent("Log Entry/6 Lines");
        menu.AddItem(content, false, LogEntry6);

        content = new GUIContent("Log Entry/7 Lines");
        menu.AddItem(content, false, LogEntry7);

        content = new GUIContent("Log Entry/8 Lines");
        menu.AddItem(content, false, LogEntry8);

        content = new GUIContent("Log Entry/9 Lines");
        menu.AddItem(content, false, LogEntry9);

        content = new GUIContent("Log Entry/10 Lines");
        menu.AddItem(content, false, LogEntry10);
    }

    private void LogEntry1() => LogEntry(1);
    private void LogEntry2() => LogEntry(2);
    private void LogEntry3() => LogEntry(3);
    private void LogEntry4() => LogEntry(4);
    private void LogEntry5() => LogEntry(5);
    private void LogEntry6() => LogEntry(6);
    private void LogEntry7() => LogEntry(7);
    private void LogEntry8() => LogEntry(8);
    private void LogEntry9() => LogEntry(9);
    private void LogEntry10() => LogEntry(10);
    private void LogEntry(int rows) => _treeView.SetRowLines(rows);


    #endregion
    #region Clear Console
    void ClearConsole()
    {
        Logx.LEntrys.Clear();
    }
    #endregion
    #region Clear on Play

    bool _clearOnPlay = true;

    void EnableClearOnPlay()
    {
        EditorApplication.playModeStateChanged -= ClearOnPlay;
        EditorApplication.playModeStateChanged += ClearOnPlay;
    }
    void DisableClearOnPlay()
    {
        EditorApplication.playModeStateChanged -= ClearOnPlay;
        EditorApplication.playModeStateChanged += ClearOnPlay;
    }

    void ClearOnPlay(PlayModeStateChange playmode)
    {
        if (playmode == PlayModeStateChange.EnteredPlayMode && _clearOnPlay)
        {
            ClearConsole();
        }
    }
    #endregion
    #region Resizer
    Rect _rresizer;
    float _resizeFactor = .8f;

    private void DrawResizer()
    {
        GUILayout.BeginArea(_rresizer);
        GUILayout.EndArea();

        EditorGUIUtility.AddCursorRect(_rresizer, MouseCursor.ResizeVertical);
    }

    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && _rresizer.Contains(e.mousePosition))
                {
                    isResizing = true;
                }
                break;

            case EventType.MouseUp:
                isResizing = false;
                break;
        }

        Resize(e);
    }
    void Resize(Event e)
    {
        if (isResizing)
        {
            _resizeFactor = Mathf.Clamp((e.mousePosition.y - 3) / position.height, .2f, .8f);
            Repaint();
        }
    }
    #endregion
    #region FilterPopUp
    Rect _rectFilterPopup;

    public class PopupExample : PopupWindowContent
    {
        public override Vector2 GetWindowSize() => new Vector2(200, 500);

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Filter Options", EditorStyles.boldLabel);
            foreach (var logtype in ExLogUtilityEditor.Asset.logstypes)
            {
                logtype.showing = EditorGUILayout.Toggle(logtype.name, logtype.showing);
                _treeView.searchString = _treeView.searchString;
                _treeView.Reload();
            }
        }

    }
    #endregion

    private class SampleTreeView : TreeView
    {
        private TreeViewItem _root;
        GUIStyle _style;
        public SampleTreeView(TreeViewState state) : base(state)
        {
            _style = EditorStyles.label;
            _style.richText = true;
            _style.alignment = TextAnchor.UpperLeft;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            _root = new TreeViewItem(0, -1, "root");
            _root.children = new System.Collections.Generic.List<TreeViewItem>();
            SetupDepthsFromParentsAndChildren(_root);
            Reload();
        }
        public void SetRowLines(int lines)
        {
            rowHeight = 16 * lines;
        }
        protected override void DoubleClickedItem(int id)
        {
            EntryItem entry = (FindItem(id, _root) as EntryItem);
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(FileUtils.ConvertPathToRelative(entry.entry.currentFile), entry.entry.currentLine);
        }

        protected override TreeViewItem BuildRoot()
        {
            _root.children.Clear();

            foreach (var e in Logx.LEntrys)
            {
                foreach (var logtype in ExLogUtilityEditor.Asset.logstypes)
                {
                    if (logtype.name == e.label && logtype.showing)
                    {
                        _root.children.Add(new EntryItem(e));
                        break;
                    }
                }
            }
            return _root;
        }

        public void AddItem(Entry entry) => Reload();
        public void AddItems(List<Entry> entrys) => Reload();

        protected override void RowGUI(RowGUIArgs args)
        {
            GUI.Label(args.rowRect, args.label, _style);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            try
            {
                EntryItem entry = (FindItem(selectedIds[0], _root) as EntryItem);
                _selected = entry.entry;
            }
            catch
            {
                _selected = null;
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            EntryItem entry = item as EntryItem;
            foreach (var logtype in ExLogUtilityEditor.Asset.logstypes)
            {
                if (logtype.name == entry.entry.label && !logtype.showing)
                {
                    return false;
                }
            }
            return entry.entry.msg.Contains(search);
        }
    }

    public class EntryItem : TreeViewItem
    {
        public Entry entry;

        public EntryItem(Entry entry) : base(entry.id, 0, entry.fulltext)
        {
            this.entry = entry;
        }
    }
}
