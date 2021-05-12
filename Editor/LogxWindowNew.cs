using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
public class LogxWindowNew : EditorWindow
{
    [MenuItem("Tools/SampleTreeView")]
    private static void ShowWindow()
    {
        GetWindow<LogxWindowNew>();
    }
    private SearchField _searchField;
    private SampleTreeView _treeView;
    private TreeViewState _treeViewState;
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

        LogxNew.onEntrysAdd -= _treeView.AddItem;
        LogxNew.onEntrysAdd += _treeView.AddItem;
    }
    private void OnGUI()
    {
        Init();
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Space(10);
            _treeView.searchString = _searchField.OnToolbarGUI(_treeView.searchString);
            if (GUILayout.Button("Create log"))
            {
                LogxNew.Log("Wololo " + Random.value, logtype: UnityLogType.Log);
            }

        }
        var rect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
        _treeView.OnGUI(rect);
    }
    private class SampleTreeView : TreeView
    {
        private TreeViewItem _root;
        public SampleTreeView(TreeViewState state) : base(state)
        {
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
            return _root;
        }

        public void AddItem(Entry entry)
        {
            _root.children.AddFirst(new EntryItem(entry));
            Reload();
        }

    }

    public class EntryItem : TreeViewItem
    {
        public Entry entry;

        public EntryItem(Entry entry) : base(entry.id, 0, entry.text)
        {
            this.entry = entry;
        }
    }
}
