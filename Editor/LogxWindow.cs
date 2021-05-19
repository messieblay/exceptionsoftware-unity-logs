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
        GetWindow<LogxWindow>();
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

    Rect _rresizer;
    Rect _rentry;


    float _resizeFactor = .8f;
    void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
    {
        GUIContent content = new GUIContent("My Custom Entry");
        menu.AddItem(content, false, MyCallback);
    }

    private void MyCallback()
    {
        Debug.Log("My Callback was called.");
    }
    Rect buttonRect;
    private void OnGUI()
    {
        Init();
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Space(10);
            _treeView.searchString = _searchField.OnToolbarGUI(_treeView.searchString);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Filter", EditorStyles.toolbarButton))
            {
                PopupWindow.Show(buttonRect, new PopupExample());
            }
            if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Logx.Refresh();
            }
            if (GUILayout.Button("Create log", EditorStyles.toolbarButton))
            {
                Logx.Log("Wololo " + Random.value, "Audio", logtype: UnityLogType.Log);
            }

        }
        var rect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, base.position.height * _resizeFactor);
        _treeView.OnGUI(rect);
        _rresizer = GUILayoutUtility.GetRect(0, float.MaxValue, 0, 3);

        DrawResizer();
        DrawEntry();
        ProcessEvents(Event.current);
    }


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


    private void Resize(Event e)
    {
        if (isResizing)
        {
            _resizeFactor = Mathf.Clamp((e.mousePosition.y - 3) / position.height, .2f, .8f);
            Repaint();
        }
    }


    public class PopupExample : PopupWindowContent
    {

        public override Vector2 GetWindowSize() => new Vector2(200, 150);

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Filter Options", EditorStyles.boldLabel);
            foreach (var logtype in ExLogsEditorUtility.Asset.logstypes)
            {
                logtype.showing = EditorGUILayout.Toggle(logtype.name, logtype.showing);
                _treeView.searchString = _treeView.searchString;
                _treeView.Reload();
            }
        }

    }

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

        protected override void DoubleClickedItem(int id)
        {
            EntryItem entry = (FindItem(id, _root) as EntryItem);
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(FileUtils.ConvertPathToRelative(entry.entry.currentFile), entry.entry.currentLine);
        }

        protected override TreeViewItem BuildRoot()
        {
            //Debug.Log(_root.children.Count);
            _root.children.Clear();

            foreach (var e in Logx.LEntrys)
            {
                foreach (var logtype in ExLogsEditorUtility.Asset.logstypes)
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

        public void AddItem(Entry entry)
        {
            //_root.children.Add(new EntryItem(entry));
            Reload();
        }
        public void AddItems(List<Entry> entrys)
        {
            //_root.children.Clear();
            //foreach (var e in entrys)
            //    _root.children.Add(new EntryItem(e));
            Reload();
        }

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
            foreach (var logtype in ExLogsEditorUtility.Asset.logstypes)
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
