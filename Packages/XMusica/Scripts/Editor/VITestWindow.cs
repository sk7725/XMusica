using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public class VITestWindow : EditorWindow {
        #region GUIContent
        static GUIContent i_noBinding = new GUIContent("There is no Virtual Instrument Binding selected. Please select a Virtual Instrument Binding or create one in the Binder window.");
        static GUIContent i_currentBinding = new GUIContent();
        static readonly GUIContent k_selectAsset = new GUIContent("Select Asset");
        static readonly GUIContent k_audioSource = new GUIContent("Audio Source", "Select an audio source to be used as an output in the current scene. FOr this to work in edit mode, Audio Preview is required to be on.");
        #endregion

        [SerializeField]
        private VirtualInstrumentBinding selected;

        private static Color whiteKeyColor = new Color(0.8f, 0.8f, 0.8f), blackKeyColor = new Color(0.2f, 0.2f, 0.2f);
        private static Color whiteKeyHoverColor = new Color(0.7f, 0.7f, 0.7f), blackKeyHoverColor = new Color(0.3f, 0.3f, 0.3f);
        private static Color whiteKeySelectedColor = Color.yellow, blackKeySelectedColor = new Color(0.8f, 0.8f, 0f);
        private static Color whiteKeyDisabledColor = new Color(0.3f, 0.3f, 0.3f), blackKeyDisabledColor = new Color(0.25f, 0.25f, 0.25f);

        private static float border = 3;
        private static float buttonWidth = 140;
        private static float keyWidth = 25, keyHeight = 120, blackKeyWidth = 18, blackKeyHeight = 70;

        [SerializeField] private Vector2 scrollPianoPos, scrollWindowPos;
        private string lastPlayedSample = "";
        private int lastPressed = -1;
        private AudioSource source;

        #region Base
        [MenuItem("XMusica/Virtual Instrument Tester", priority = 2)]
        public static void ShowWindow() {
            var window = GetWindow<VITestWindow>(XM_EditorUtilities.GetDockTargets());
            window.titleContent = new GUIContent("VInst Tester", XM_UIStyleManager.vinstTesterWindowIcon);
            window.wantsMouseMove = true;
            window.Show();
        }

        private void OnSelectionChange() {
            lastPressed = -1;
            lastPlayedSample = "";
            Refresh();
        }

        private void OnFocus() {
            Refresh();
        }

        private void Refresh() {
            if (Selection.activeObject is VirtualInstrumentBinding vb) {
                selected = vb;
            }
            else {
                selected = null;
            }
            Repaint();
        }

        private void OnGUI() {
            wantsMouseMove = true;
            i_noBinding.image = XM_UIStyleManager.vinstTesterWindowIcon;
            i_currentBinding.image = XM_UIStyleManager.vinstTesterWindowIcon;

            scrollWindowPos = EditorGUILayout.BeginScrollView(scrollWindowPos);
            if (selected != null) {
                SelectedMenuGUI();
            }
            else {
                CreateMenuGUI();
            }
            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }
        #endregion

        #region RootMenuGUI
        private void CreateMenuGUI() {
            GUILayout.Label(i_noBinding, GUILayout.Height(40f));
            bool create = GUILayout.Button("Open Binder Window");
            if (create) {
                VIBWindow.ShowWindow();
            }
        }

        private void SelectedMenuGUI() {
            i_currentBinding.text = $"{selected.name} ({AssetDatabase.GetAssetPath(selected)})";
            GUILayout.Label(i_currentBinding, GUILayout.Height(40f));
            GUILayout.Space(5);
            if (GUILayout.Button(k_selectAsset, GUILayout.Width(buttonWidth))) {
                EditorGUIUtility.PingObject(selected);
            }
            GUILayout.Space(10);

            bool valid = selected.HasGeneratedSamples();
            source = (AudioSource)EditorGUILayout.ObjectField(k_audioSource, source, typeof(AudioSource), true);
            if (valid) {
                EditorGUILayout.HelpBox(lastPlayedSample, MessageType.Info, true);
            }
            else {
                EditorGUILayout.HelpBox("Generate sample bindings first!", MessageType.Error, true);
            }
            GUILayout.Space(5);

            int pressed = NotePreview(lastPressed, valid, out float fvel);

            if(pressed != -1 && valid) {
                HandleNotePressed(pressed, Mathf.CeilToInt(fvel * 127));
            }
        }
        #endregion

        #region PianoGUI
        private int NotePreview(int highlightNote, bool enabled, out float fvelocity) {
            Event evt = Event.current;
            Rect scrollRect = EditorGUILayout.GetControlRect(true, keyHeight + border * 2 + 18);
            float totalWidth = keyWidth * 70 + border * 2;
            float totalHeight = keyHeight + border * 2;

            //generate scroll pane
            scrollPianoPos = GUI.BeginScrollView(scrollRect, scrollPianoPos, new Rect(0, 0, totalWidth, totalHeight), true, false);
            Rect total = new Rect(border, border, keyWidth * 70, keyHeight);

            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(total, "");
            GUI.backgroundColor = defColor;

            int keyPressed = -1;
            fvelocity = 1f;

            //white keys
            float x = total.x;
            for (int i = 0; i < 70; i++) {
                int now = GetNoteOfWhiteKey(i);

                //test mouse hover
                int j = i % 7;
                bool hasLeftBit = j == 3 || j == 0 || i == 0; bool hasRightBit = j == 2 || j == 6 || i == 69;
                Rect r1 = new Rect(x, total.y + blackKeyHeight, keyWidth, total.height - blackKeyHeight - border);
                Rect r2 = new Rect(hasLeftBit ? x : x + blackKeyWidth * 0.5f, total.y, keyWidth - blackKeyWidth * ((hasLeftBit ? 0 : 0.5f) + (hasRightBit ? 0 : 0.5f)), blackKeyHeight);
                bool hovered = r1.Contains(evt.mousePosition) || r2.Contains(evt.mousePosition);

                //determine key color
                if (now < 21 || now > 127 || !enabled) {
                    GUI.backgroundColor = whiteKeyDisabledColor;
                }
                else GUI.backgroundColor = highlightNote == now ? whiteKeySelectedColor : hovered ? whiteKeyHoverColor : whiteKeyColor;

                if (hovered && (evt.type == EventType.MouseDown)) {
                    keyPressed = now;
                    fvelocity = (evt.mousePosition.y - total.y) / keyHeight;
                }

                //render key
                GUI.Button(r1, XM_Utilities.GetNoteString(now), XM_UIStyleManager.whitePianoKey);
                GUI.Button(r2, "", XM_UIStyleManager.whitePianoKey);
                /*if ((b1 || b2) && (now >= 21 && now <= 127)) {
                    keyPressed = now;
                    fvelocity = (evt.mousePosition.y - total.y) / keyHeight;
                }*/
                x += keyWidth;
            }

            //black keys
            x = total.x + keyWidth * 0.5f + (keyWidth - blackKeyWidth) * 0.5f;
            for (int i = 0; i < 69; i++) {
                int j = i % 7;
                if (j == 2 || j == 6) {
                    x += keyWidth;
                    continue;
                }
                int now = GetNoteOfBlackKey(i);

                //test mouse hover
                Rect r1 = new Rect(x, total.y, blackKeyWidth, blackKeyHeight);
                bool hovered = r1.Contains(evt.mousePosition);

                //determine key color
                if (now < 21 || now > 127 || !enabled) {
                    GUI.backgroundColor = blackKeyDisabledColor;
                }
                else GUI.backgroundColor = highlightNote == now ? blackKeySelectedColor : hovered ? blackKeyHoverColor : blackKeyColor;

                if (hovered && evt.type == EventType.MouseDown) {
                    keyPressed = now;
                    fvelocity = (evt.mousePosition.y - total.y) / blackKeyHeight;
                }

                //render key
                GUI.Button(r1, "", XM_UIStyleManager.blackPianoKey);
                /*if (b && (now >= 21 && now <= 127)) {
                    keyPressed = now;
                    fvelocity = (evt.mousePosition.y - total.y) / blackKeyHeight;
                }*/
                x += keyWidth;
            }

            GUI.EndScrollView(true);
            GUI.backgroundColor = defColor;

            return keyPressed;
        }

        private static int[] whiteToNote = { 0, 2, 4, 5, 7, 9, 11 };
        private int GetNoteOfWhiteKey(int i) {
            return whiteToNote[i % 7] + (i / 7) * 12 + 12;
        }

        private int GetNoteOfBlackKey(int i) {
            return GetNoteOfWhiteKey(i) + 1;
        }
        #endregion

        private void HandleNotePressed(int note, int velocity) {
            lastPressed = note;
            var sd = selected.GetSample(note, velocity, out float volume, out float pitch);
            if(sd.clip == null) {
                lastPlayedSample = "No clip assigned";
                return;
            }
            lastPlayedSample = $"{AssetDatabase.GetAssetPath(sd.clip)} pitch offset: {pitch} volume: {volume}";
            if(source != null) {
                source.clip = sd.clip;
                source.volume = volume;
                source.pitch = pitch;
                source.Play();
            }
        }
    }
}