using ExceptionSoftware.ExEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ExceptionSoftware.Logs
{
    public class LogxWindow : ExWindow<LogxWindow>
    {
        #region Window


        [MenuItem("Tools/Logx Window", false, 3000)]
        public static LogxWindow Open()
        {

            //System.Type wtype = System.Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
            //var window = LogxWindow.GetWindow<LogxWindow>(wtype);
            var window = LogxWindow.GetWindow<LogxWindow>();
            window.titleContent = new GUIContent("ReConsole");
            window.Focus();
            window.Repaint();
            return window;
            //LogxWindow.TryOpenWindow();
        }

        static void WriteENum(System.Enum e)
        {
            Debug.Log(e.ToString());
            Debug.Log(System.Enum.GetName(e.GetType(), e));
        }

        public override string GetTitle() { return "ReConsole"; }

        string[] _labels;

        protected override void DoEnable()
        {

            //_labels = System.Enum.GetNames(typeof(LogxEnum));
            _labels = new string[] { "None" };

            Logx.onEntrysAdd -= OnEntryAdd;
            Logx.onEntrysAdd += OnEntryAdd;

            FilterEntrys();
            EnableClearOnPlay();
        }

        protected override void DoDisable()
        {
            Logx.onEntrysAdd -= OnEntryAdd;

            DisableClearOnPlay();
        }

        void OnEntryAdd(List<LogEntry> entrys)
        {
            foreach (var e in entrys)
            {
                if (_labels.Contains(e.msgType))
                {
                    _labels = _labels.Add(e.msgType);
                }
            }
            FilterEntrys();
            if (_isScrollOnBottom)
            {
                _scroll.y = _scrollContentHeight;
            }
            Repaint();
        }

        protected override void DoRecompile()
        {
            base.DoRecompile();
            DoEnable();
        }

        #endregion
        #region Styles

        GUIStyle MessageStyle;
        GUIStyle Box;
        GUIStyle EvenBackground;
        GUIStyle OddBackground;
        GUIStyle resizerStyle;
        GUIStyle rowStyle;
        static bool _stylesLoaded = false;
        void TryLoadStyles()
        {
            if (_stylesLoaded) return;
            EvenBackground = new GUIStyle("CN EntryBackEven");

            RectOffset padding = EvenBackground.padding;
            padding.bottom = padding.top = 5;

            EvenBackground.wordWrap = true;
            EvenBackground.margin = new RectOffset();
            EvenBackground.padding = padding;

            OddBackground = new GUIStyle("CN EntryBackodd");
            OddBackground.wordWrap = true;
            OddBackground.margin = new RectOffset();
            OddBackground.padding = padding;

            resizerStyle = new GUIStyle();
            resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;
            MessageStyle = new GUIStyle("CN Message");
            Box = new GUIStyle("CN Box");
            _stylesLoaded = true;

        }
        #endregion
        #region Layout

        Rect[] _lv = null;
        Rect _toolbar;
        Rect _logs;
        Rect _logsStackResizer;
        Rect _logsStack;
        Rect _content;
        Rect[] _lhToolbar;
        Rect resizer;
        Rect stackPanel;

        protected override void DoLayout()
        {
            _lv = position.CopyToZero().SplitSuperFixedFlexible(true, 20, (position.height * (stackPanelRatio)), position.height * (1 - stackPanelRatio));
            _toolbar = _lv[0];
            _logs = _lv[1];
            _logsStack = _lv[2];
            _lhToolbar = _toolbar.SplitSuperFixedFlexible(false, 50, 5, 100, 100, 100, 30, -1);
            resizer = _lv[2].Copy().SetHeight(10);
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
        #region Toolbar

        //Filter
        int _maskView = ~0;
        List<string> _labelsSelected = new List<string>();
        bool _filterNormalLog = true;
        bool _filterWarningLog = true;
        bool _filterErrorLog = true;
        int _filterNormalLogCount = 0;
        int _filterWarningLogCount = 0;
        int _filterErrorLogCount = 0;


        void DrawToolbar()
        {
            GUILayout.BeginArea(_toolbar, EditorStyles.toolbar);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    ExGUI.Button("Clear", EditorStyles.toolbarButton, ClearConsole);
                    _clearOnPlay = GUILayout.Toggle(_clearOnPlay, "Clear on Play", EditorStyles.toolbarButton);

                    ExGUI.Button("Clear Current", EditorStyles.toolbarButton, Removecurrent);

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    _maskView = EditorGUILayout.MaskField(_maskView, _labels, EditorStyles.toolbarDropDown);
                    _filterNormalLog = GUILayout.Toggle(_filterNormalLog, new GUIContent((_filterNormalLogCount > 999) ? "999+" : _filterNormalLogCount.ToString(), EditorGUIUtility.FindTexture("d_console.infoicon.sml")), EditorStyles.toolbarButton, new GUILayoutOption[0]);
                    _filterWarningLog = GUILayout.Toggle(_filterWarningLog, new GUIContent((_filterWarningLogCount > 999) ? "999+" : _filterWarningLogCount.ToString(), EditorGUIUtility.FindTexture("d_console.warnicon.sml")), EditorStyles.toolbarButton, new GUILayoutOption[0]);
                    _filterErrorLog = GUILayout.Toggle(_filterErrorLog, new GUIContent((_filterErrorLogCount > 999) ? "999+" : _filterErrorLogCount.ToString(), EditorGUIUtility.FindTexture("d_console.erroricon.sml")), EditorStyles.toolbarButton, new GUILayoutOption[0]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        FilterEntrys();
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        void FilterLavbelsSelectedToView()
        {
            _labelsSelected.Clear();
            for (int x = 0; x < _labels.Length; x++)
            {
                int layer = 1 << x;
                if ((_maskView & layer) != 0)
                {
                    _labelsSelected.Add(_labels[x]);
                }
            }
        }

        bool ContainsLabel(string label)
        {
            for (int x = 0; x < _labelsSelected.Count; x++)
            {
                if (_labelsSelected[x] == label)
                {
                    return true;
                }
            }
            return false;
        }

        void FilterEntrys()
        {
            _filterNormalLogCount = _lEntrys.Count(s => s.logType == LogType.Log);
            _filterWarningLogCount = _lEntrys.Count(s => s.logType == LogType.Warning);
            _filterErrorLogCount = _lEntrys.Count(s => s.logType == LogType.Error);

            FilterLavbelsSelectedToView();
            _lEntrysFiltred = _lEntrys.Where(s => ContainsLabel(s.msgType)).ToList();
            if (!_filterNormalLog)
            {
                _lEntrysFiltred.RemoveAll(s => s.logType == LogType.Log);
            }
            if (!_filterWarningLog)
            {
                _lEntrysFiltred.RemoveAll(s => s.logType == LogType.Warning);
            }
            if (!_filterErrorLog)
            {
                _lEntrysFiltred.RemoveAll(s => s.logType == LogType.Error);
            }

        }
        void Removecurrent()
        {
            FilterLavbelsSelectedToView();
            _lEntrys.RemoveAll(s => ContainsLabel(s.msgType));
            FilterEntrys();
        }
        void ClearConsole()
        {
            _lEntrys.Clear();
            FilterEntrys();
        }
        #endregion
        #region Logs 

        Vector2 _scroll;
        [SerializeField] List<LogEntry> _lEntrysFiltred = new List<LogEntry>();
        public List<LogEntry> _lEntrys { get { return Logx.LEntrys; } }

        float _currentWidth = 0;
        float _height;
        LogEntry _selectedEntry = null;
        int _selectedEntryIndex = -1;
        Rect _controlRect;

        float _scrollContentHeight;
        bool _isScrollOnBottom = false;
        Color[] logTypeColors = { Color.red, Color.white, Color.yellow, Color.white, Color.red };
        public override void DoGUI()
        {
            if (_currentWidth != base.position.width)
            {
                _currentWidth = base.position.width;
            }
            TryLoadStyles();
            ProcessEvents();
            DrawToolbar();

            GUILayout.BeginArea(_logs);
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                {
                    for (int x = 0; x < _lEntrysFiltred.Count; x++)
                    {
                        GUI.color = logTypeColors[(int)_lEntrysFiltred[x].logType];
                        rowStyle = x % 2 == 0 ? EvenBackground : OddBackground;
                        _height = rowStyle.CalcHeight(_lEntrysFiltred[x].content, _logs.width);
                        EditorGUI.BeginChangeCheck();
                        _lEntrysFiltred[x].active = GUILayout.Toggle(_lEntrysFiltred[x].active, _lEntrysFiltred[x].content, rowStyle, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(_logs.width), /*GUILayout.ExpandHeight(true),*/ GUILayout.Height(_height));
                        if (EditorGUI.EndChangeCheck())
                        {
                            _lEntrys.Where(s => s.count != _lEntrysFiltred[x].count).ForEach(s => s.active = false);
                            _selectedEntry = _lEntrysFiltred[x];

                            if (_lEntrysFiltred[x].active)
                            {
                                _selectedEntryIndex = x;
                            }
                            else
                            {
                                _selectedEntryIndex = -1;
                            }

                            if (xEvents.MouseRight)
                            {
                                System.Diagnostics.Process.Start("devenv", string.Format("/edit \"{0}\" /command \"Edit.GoTo {1}\"", _lEntrysFiltred[x].currentFile, _lEntrysFiltred[x].currentLine.ToString()));
                                //System.Diagnostics.Process.Start("devenv", " /edit \"" + _lEntrysFiltred[x].currentFile + "\" /command \"edit.goto " + _lEntrysFiltred[x].currentLine.ToString() + " \" ");
                            }
                        }
                    }
                    GUI.color = Color.white;

                    if (_lEntrysFiltred.Count > 0)
                    {
                        _controlRect = GUILayoutUtility.GetLastRect();
                        if (_controlRect.height > 1)
                        {
                            _scrollContentHeight = _controlRect.y;
                        }
                    }
                }
                _isScrollOnBottom = _scroll.y > _scrollContentHeight - _logs.height;
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            DrawLowerPanel();
            DrawResizer();


        }

        #endregion
        #region Resize Panel
        Vector2 _stackScroll;
        float stackPanelRatio = 0.8f;
        bool isResizing = false;

        void DrawLowerPanel()
        {
            GUILayout.BeginArea(_lv[2]);
            {
                _stackScroll = GUILayout.BeginScrollView(this._stackScroll, Box, GUILayout.Height(_lv[2].height - 20));
                if (_selectedEntry != null)
                {
                    float minHeight = Mathf.Max(100, MessageStyle.CalcHeight(new GUIContent(_selectedEntry.st), base.position.width));
                    EditorGUILayout.SelectableLabel(_selectedEntry.st, MessageStyle, new GUILayoutOption[]
                    {
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true),
                    GUILayout.MinHeight(minHeight)
                    });
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        void DrawResizer()
        {

            GUILayout.BeginArea(new Rect(resizer.position + (Vector2.up * 5f), new Vector2(position.width, 2)));
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeVertical);
        }

        void ProcessEvents()
        {
            if (xEvents.MouseLeft)
            {
                if (xEvents.MouseDown)
                {
                    if (resizer.Contains(e.mousePosition))
                    {
                        isResizing = true;
                    }
                }
                if (xEvents.MouseUp)
                {
                    isResizing = false;
                }
            }
            if (xEvents.KeyDown)
            {
                if (xEvents.KeyCode == KeyCode.DownArrow)
                {
                    _selectedEntryIndex = Mathf.Min(_selectedEntryIndex + 1, _lEntrysFiltred.Count - 1);
                    _selectedEntry.active = false;
                    _selectedEntry = _lEntrysFiltred[_selectedEntryIndex];
                    _selectedEntry.active = true;
                    Repaint();
                }

                if (xEvents.KeyCode == KeyCode.UpArrow)
                {
                    _selectedEntryIndex = Mathf.Max(_selectedEntryIndex - 1, 0);
                    _selectedEntry.active = false;
                    _selectedEntry = _lEntrysFiltred[_selectedEntryIndex];
                    _selectedEntry.active = true;
                    Repaint();
                }
            }

            Resize(e);
        }

        void Resize(Event e)
        {
            if (isResizing)
            {
                stackPanelRatio = Mathf.Max(0.1f, Mathf.Min(.9f, (e.mousePosition.y - 20) / (position.height - 20)));
                DoLayout();
                Repaint();
            }
        }
        #endregion
    }
}
