using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public class VIBWindow : EditorWindow {
        #region GUIContent
        static GUIContent i_noBinding = new GUIContent("There is no Virtual Instrument Binding selected. Would you like to create a new binding?");
        static GUIContent i_currentBinding = new GUIContent();
        static GUIContent i_estimatedNoteSamplesContent = new GUIContent();
        static GUIContent i_estimatedTotalSamplesContent = new GUIContent();

        static readonly GUIContent t_noteSampler = new GUIContent("Note Sampler");
        static readonly GUIContent t_velocitySampler = new GUIContent("Velocity Sampler");
        static readonly GUIContent t_roundRobinSampler = new GUIContent("Round Robin Sampler");
        static readonly GUIContent t_notePreview = new GUIContent("Note Preview");
        static readonly GUIContent t_velocityPreview = new GUIContent("Velocity Preview");

        static readonly GUIContent k_startNote = new GUIContent("Sample Start Note", "The note where the samples start.");
        static readonly GUIContent k_endNote = new GUIContent("Sample Cutoff Note", "The note where the samples end. THis does not need to point exactly to a sample; instead any samples higher than this note will be ignored.");
        static readonly GUIContent k_sampleSpace = new GUIContent("Sample Spacing", "Space between note samples. Recommended to be a divisor of 12, a full octave.");
        static readonly GUIContent k_estimatedNoteSamples = new GUIContent("Note Samples", "The estimated number of note samples needed per velocity.");

        static readonly GUIContent k_velocitySamples = new GUIContent("Velocity Samples", "Number of velocity samples per note.");
        static readonly GUIContent k_selectedVelocity = new GUIContent("Selected Velocity");
        static readonly GUIContent k_volumeMultiplier = new GUIContent("Volume Multiplier", "Volume multiplier for current velocity sample.");
        static readonly GUIContent k_normalizeVolume = new GUIContent("Normalize", "Normalize velocity sample. Select this option if your samples with smaller velocities are quiet. (As quiet as the stated velocity, that is)");
        static readonly GUIContent k_fullVolume = new GUIContent("Full", "Full velocity sample. Select this option if your samples with smaller velocities are as loud as the bigger ones.");
        static readonly GUIContent k_roundRobins = new GUIContent("Round Robins", "Round robin refers to multiple samples of the same note and velocity. This is used to give the virtual instrument more depth.");
        static readonly GUIContent k_estimatedTotalSamples = new GUIContent("Total Samples", "The estimated number of total samples needed.");

        static readonly GUIContent k_save = new GUIContent("Apply");
        static readonly GUIContent k_revert = new GUIContent("Revert");
        static readonly GUIContent k_generate = new GUIContent("Generate");
        static readonly GUIContent k_reset = new GUIContent("Reset to Default");
        static readonly GUIContent k_selectAsset = new GUIContent("Select Asset");

        static readonly string s_unsaved = "You have unapplied binding generation settings. Would you like to apply the changes?";
        #endregion

        [SerializeField]
        private VirtualInstrumentBinding selected;

        private static float border = 3;
        private static float fieldMaxWidth = 400, velocityPreviewWidth = 400, velocityPreviewHeight = 100, buttonWidth = 140;
        private static float keyWidth = 25, keyHeight = 120, blackKeyWidth = 18, blackKeyHeight = 70;

        [SerializeField]
        private VInstGenerationData generationData;
        [SerializeField]
        private int currentEditingVelocity = 0;
        [SerializeField]
        private bool isDirty = false;

        private VirtualInstrumentBinding lastSelected;
        private VInstGenerationData lastGenerationData;

        private bool changed;
        [SerializeField] private PianoState pianoState = PianoState.None;

        [SerializeField] private Vector2 scrollPianoPos, scrollWindowPos;

        public enum PianoState {
            None,
            SelectStartNote,
            SelectEndNote
        }

        private static Color whiteKeyColor = new Color(0.8f, 0.8f, 0.8f), blackKeyColor = new Color(0.2f, 0.2f, 0.2f);
        private static Color whiteKeyHoverColor = new Color(0.7f, 0.7f, 0.7f), blackKeyHoverColor = new Color(0.3f, 0.3f, 0.3f);
        private static Color whiteKeySelectedColor = Color.yellow, blackKeySelectedColor = new Color(0.8f, 0.8f, 0f);
        private static Color whiteKeyDisabledColor = new Color(0.3f, 0.3f, 0.3f), blackKeyDisabledColor = new Color(0.25f, 0.25f, 0.25f);
        private const float whiteKeyS = 0.2f, whiteKeyV = 1f;
        private const float blackKeyS = 0.4f, blackKeyV = 0.4f;

        #region Base
        [MenuItem("XMusica/Virtual Instrument Binder", priority = 0)]
        public static void ShowWindow() {
            var window = GetWindow<VIBWindow>(XM_EditorUtilities.GetDockTargets());
            window.titleContent = new GUIContent("VInst Binder", XM_UIStyleManager.vinstBinderWindowIcon);
            window.saveChangesMessage = s_unsaved;
            window.Show();
        }

        private void OnSelectionChange() {
            Refresh();
        }

        private void OnFocus() {
            Refresh();
        }

        private void Refresh() {
            if (isDirty && selected != null && (!(Selection.activeObject is VirtualInstrumentBinding b) || b != selected)) {
                //warn save popup
                lastGenerationData = generationData;
                lastSelected = selected;

                int option = EditorUtility.DisplayDialogComplex("Unsaved Changes", s_unsaved, "Apply", "Cancel", "Discard");
                switch (option) {
                    case 0://save
                        SaveChanges();
                        break;
                    case 1://cancel
                        Selection.activeObject = lastSelected;
                        return;
                    case 2://discard
                        DiscardChanges();
                        break;
                }
            }

            if (Selection.activeObject is VirtualInstrumentBinding vb) {
                if (selected != vb) {
                    selected = vb;
                    generationData = vb.generationData;
                    currentEditingVelocity = 0;
                    isDirty = false;
                    ValidateVMultipliers();
                }
            }
            else {
                selected = null;
            }
            Repaint();
        }

        private void OnGUI() {
            wantsMouseMove = true;
            i_noBinding.image = XM_UIStyleManager.vinstBinderWindowIcon;
            i_currentBinding.image = XM_UIStyleManager.vinstBinderWindowIcon;

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
            bool create = GUILayout.Button("New Virtual Instrument Binding");
            if (create) {
                CreateNewVInst();
            }
        }

        private void SelectedMenuGUI() {
            i_currentBinding.text = $"{selected.name}{(isDirty ? "*" : "")} ({AssetDatabase.GetAssetPath(selected)})";
            GUILayout.Label(i_currentBinding, GUILayout.Height(40f));
            GUILayout.Space(5);
            if (GUILayout.Button(k_selectAsset, GUILayout.Width(buttonWidth))) {
                EditorGUIUtility.PingObject(selected);
            }
            GUILayout.Space(10);

            changed = false;

            //note
            GUILayout.Label(t_noteSampler, EditorStyles.boldLabel, GUILayout.Height(30));
            //EditorGUI.indentLevel++;
            if (generationData.useEvenNoteSpacings) {
                NoteGenerationSettings();
            }
            else {
                EnableNoteGeneration();
            }
            //EditorGUI.indentLevel--;
            GUILayout.Space(10);

            //velocity
            GUILayout.Label(t_velocitySampler, EditorStyles.boldLabel, GUILayout.Height(30));
            //EditorGUI.indentLevel++;
            if (generationData.useEvenVelocitySpacings) {
                VelGenerationSettings();
            }
            else {
                EnableVelGeneration();
            }
            //EditorGUI.indentLevel--;
            GUILayout.Space(10);

            //round robin
            GUILayout.Label(t_roundRobinSampler, EditorStyles.boldLabel, GUILayout.Height(30));
            //EditorGUI.indentLevel++;
            RoundRobinSettings();
            //EditorGUI.indentLevel--;
            GUILayout.Space(10);

            SaveMenu();

            if (changed) {
                isDirty = true;
                hasUnsavedChanges = true;
            }
        }
        #endregion

        #region NoteGUI
        private void NoteGenerationSettings() {
            NoteSelectField(k_startNote, generationData.noteStartPos, PianoState.SelectStartNote);
            NoteSelectField(k_endNote, generationData.noteEndCutoff, PianoState.SelectEndNote);

            generationData.noteSampleDist = IntField(k_sampleSpace, generationData.noteSampleDist, true);

            GUILayout.Space(5);
            int noteSamples = generationData.NoteSamples;
            i_estimatedNoteSamplesContent.text = noteSamples.ToString();
            EditorGUILayout.LabelField(k_estimatedNoteSamples, i_estimatedNoteSamplesContent, GUILayout.Width(fieldMaxWidth));

            GUILayout.Space(5);
            int input = NotePreview(pianoState == PianoState.None, GetCurrentPianoHighlight());
            if (input != -1) HandleNoteClick(input);

            GUILayout.Space(5);
            if (GUILayout.Button("Disable Even-Spaced Note Samples", GUILayout.Width(fieldMaxWidth))) {
                generationData.useEvenNoteSpacings = false;
                changed = true;
                GUI.FocusControl("");
            }
        }

        private int GetCurrentPianoHighlight() {
            switch (pianoState) {
                default:
                    return 0;
                case PianoState.SelectStartNote:
                    return generationData.noteStartPos;
                case PianoState.SelectEndNote:
                    return generationData.noteEndCutoff;
            }
        }

        private void HandleNoteClick(int note) {
            switch (pianoState) {
                case PianoState.None:
                    Debug.Log($"Note {XM_Utilities.GetNoteString(note)}: using sample #{XM_Utilities.GetEstimatedReferenceNoteIndex(note, generationData)}");
                    break;
                case PianoState.SelectStartNote:
                    if (note != generationData.noteStartPos) {
                        generationData.noteStartPos = note;
                        changed = true;
                    }

                    pianoState = PianoState.None;
                    GUI.FocusControl("");
                    break;
                case PianoState.SelectEndNote:
                    if (note != generationData.noteEndCutoff) {
                        generationData.noteEndCutoff = note;
                        changed = true;
                    }

                    pianoState = PianoState.None;
                    GUI.FocusControl("");
                    break;
            }
        }

        private int NotePreview(bool showSampleColor, int highlightNote) {
            Event evt = Event.current;
            GUILayout.Label(t_notePreview, EditorStyles.boldLabel, GUILayout.Height(30));

            EditorGUILayout.HelpBox(showSampleColor ? GetNoteSamplesGuide() : "Select a key from the virtual piano below.", MessageType.Info);

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
            int size = generationData.NoteSamples;

            //white keys
            float x = total.x;
            for (int i = 0; i < 70; i++) {
                int now = GetNoteOfWhiteKey(i);

                //test mouse hover
                int j = i % 7;
                bool hasLeftBit = j == 3 || j == 0 || i == 0; bool hasRightBit = j == 2 || j == 6 || i == 69;
                Rect r1 = new Rect(x, total.y + blackKeyHeight, keyWidth + 1f, total.height - blackKeyHeight - border);
                Rect r2 = new Rect(hasLeftBit ? x : x + blackKeyWidth * 0.5f, total.y, keyWidth - blackKeyWidth * ((hasLeftBit ? 0 : 0.5f) + (hasRightBit ? 0 : 0.5f)), blackKeyHeight);
                bool hovered = !showSampleColor && (r1.Contains(evt.mousePosition) || r2.Contains(evt.mousePosition));

                //determine key color
                if (now < 21 || now > 127) {
                    GUI.backgroundColor = whiteKeyDisabledColor;
                }
                else if (showSampleColor) {
                    int sid = XM_Utilities.GetEstimatedSampleNoteIndex(now, generationData);
                    if (sid != -1) {
                        GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(size, sid);
                    }
                    else {
                        int tid = XM_Utilities.GetEstimatedReferenceNoteIndex(now, generationData);
                        if (tid != -1) {
                            GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(size, tid, whiteKeyS, whiteKeyV);
                        }
                        else {
                            GUI.backgroundColor = whiteKeyColor;
                        }
                    }
                }
                else GUI.backgroundColor = highlightNote == now ? whiteKeySelectedColor : hovered ? whiteKeyHoverColor : whiteKeyColor;

                //render key
                bool b1 = GUI.Button(r1, XM_Utilities.GetNoteString(now), XM_UIStyleManager.whitePianoKey);
                bool b2 = GUI.Button(r2, "", XM_UIStyleManager.whitePianoKey);
                if ((b1 || b2) && (now >= 21 && now <= 127)) keyPressed = now;
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
                Rect r1 = new Rect(x - 0.5f, total.y, blackKeyWidth + 1f, blackKeyHeight);
                bool hovered = !showSampleColor && r1.Contains(evt.mousePosition);

                //determine key color
                if (now < 21 || now > 127) {
                    GUI.backgroundColor = blackKeyDisabledColor;
                }
                else if (showSampleColor) {
                    int sid = XM_Utilities.GetEstimatedSampleNoteIndex(now, generationData);
                    if (sid != -1) {
                        GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(size, sid);
                    }
                    else {
                        int tid = XM_Utilities.GetEstimatedReferenceNoteIndex(now, generationData);
                        if (tid != -1) {
                            GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(size, tid, blackKeyS, blackKeyV);
                        }
                        else {
                            GUI.backgroundColor = blackKeyColor;
                        }
                    }
                }
                else GUI.backgroundColor = highlightNote == now ? blackKeySelectedColor : hovered ? blackKeyHoverColor : blackKeyColor;

                //render key
                bool b = GUI.Button(r1, "", XM_UIStyleManager.blackPianoKey);
                if (b && (now >= 21 && now <= 127)) keyPressed = now;
                x += keyWidth;
            }

            GUI.EndScrollView(true);
            GUI.backgroundColor = defColor;

            return keyPressed;
        }

        private string GetNoteSamplesGuide() {
            StringBuilder sb = new("Samples: ");
            int size = generationData.NoteSamples;
            for(int i = 0; i < size; i++) {
                sb.Append(XM_Utilities.GetNoteString(generationData.noteStartPos + i * generationData.noteSampleDist));
                if(i < size - 1) sb.Append(',');
                sb.Append(' ');
            }

            sb.Append('(');
            sb.Append(size);
            sb.Append(')');
            return sb.ToString();
        }

        private static int[] whiteToNote = {0, 2, 4, 5, 7, 9, 11};
        private int GetNoteOfWhiteKey(int i) {
            return whiteToNote[i % 7] + (i / 7) * 12 + 12;
        }

        private int GetNoteOfBlackKey(int i) {
            return GetNoteOfWhiteKey(i) + 1;
        }

        private void EnableNoteGeneration() {
            EditorGUILayout.HelpBox("Even-spaced sample structure generation for notes is disabled for this Virtual Instrument. Would you like to enable even-spaced samples? Caution: this will override the current sample settings!", MessageType.Warning, true);
            if(GUILayout.Button("Enable Even-Spaced Note Samples", GUILayout.Width(fieldMaxWidth))){
                generationData.useEvenNoteSpacings = true;
                changed = true;
            }
        }
        #endregion

        #region VelocityGUI
        private void VelGenerationSettings() {
            generationData.velocitySamples = IntField(k_velocitySamples, generationData.velocitySamples, true);
            ValidateVMultipliers();

            //select velocity
            string[] options = new string[generationData.velocitySamples];
            for (int i = 0; i < generationData.velocitySamples; i++) {
                int v1 = i == 0 ? 0 : generationData.GetVelocitySampleAt(i - 1);
                int v2 = generationData.GetVelocitySampleAt(i);
                options[i] = $"#{i} ({v1} ~ {v2})";
            }
            currentEditingVelocity = EditorGUILayout.Popup(k_selectedVelocity, currentEditingVelocity, options, GUILayout.MaxWidth(fieldMaxWidth));

            EditorGUI.indentLevel++;
            //volume multiplier
            int v = generationData.GetVelocitySampleAt(currentEditingVelocity);
            EditorGUI.BeginDisabledGroup(v == 0);
            generationData.volumeMultipliers[currentEditingVelocity] = Slider(k_volumeMultiplier, generationData.volumeMultipliers[currentEditingVelocity], 0, v == 0 ? 1 : (127f / v));
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(fieldMaxWidth));
            GUILayout.Box("", GUILayout.Width(10));
            if (GUILayout.Button(k_normalizeVolume)) {
                generationData.volumeMultipliers[currentEditingVelocity] = 127f / v;
                changed = true;
            }
            if (GUILayout.Button(k_fullVolume)) {
                generationData.volumeMultipliers[currentEditingVelocity] = 1f;
                changed = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            GUILayout.Space(5);
            VelocityPreview();

            GUILayout.Space(5);
            if (GUILayout.Button("Disable Even-Spaced Velocity Samples", GUILayout.Width(fieldMaxWidth))) {
                generationData.useEvenVelocitySpacings = false;
                changed = true;
                GUI.FocusControl("");
            }
        }

        private void ValidateVMultipliers() {
            if (generationData.volumeMultipliers == null || generationData.volumeMultipliers.Length != generationData.velocitySamples) {
                var prev = generationData.volumeMultipliers;
                generationData.volumeMultipliers = new float[generationData.velocitySamples];
                Array.Fill(generationData.volumeMultipliers, 1f);

                //copy over previous generationData
                if(prev != null) {
                    int n = Mathf.Min(prev.Length, generationData.volumeMultipliers.Length);
                    for(int i = 0; i < n; i++) {
                        int v = generationData.GetVelocitySampleAt(generationData.volumeMultipliers.Length - i - 1);
                        float max = v == 0 ? 1 : (127f / v);
                        generationData.volumeMultipliers[generationData.volumeMultipliers.Length - i - 1] = Mathf.Min(prev[prev.Length - i - 1], max);
                    }
                }
                isDirty = true;
                Debug.Log("Regenerating volume multiplier array...");
            }
            currentEditingVelocity = Mathf.Clamp(currentEditingVelocity, 0, generationData.velocitySamples - 1);
        }

        Rect graphicRect;

        private void VelocityPreview() {
            GUILayout.Label(t_velocityPreview, EditorStyles.boldLabel, GUILayout.Height(30));

            EditorGUILayout.HelpBox(GetVelocitySamplesGuide(), MessageType.Info);

            Rect scrollRect = EditorGUILayout.GetControlRect(true, velocityPreviewHeight + border * 2, GUILayout.Width(velocityPreviewWidth + border * 2));

            Rect total = new Rect(scrollRect.x + border, scrollRect.y + border, scrollRect.width - border, scrollRect.height - border);

            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(total, "");
            GUI.backgroundColor = defColor;
            SetGraphicTarget(total);
            int size = generationData.velocitySamples;

            //draw each sample poly
            for (int i = 0; i < size; i++) {
                float f = generationData.GetVelocitySampleAt(i) / 127f;
                float f2 = i == 0 ? 0 : generationData.GetVelocitySampleAt(i - 1) / 127f;
                Handles.color = XM_EditorUtilities.GetDiagramColor(size, i, 1, 0.5f);
                Handles.DrawAAConvexPolygon(GPos(f, 1 - f), GPos(f, 1), GPos(f2, 1), GPos(f2, 1 - f2));
            }

            //draw each sample line
            for (int i = 0; i < size; i++) {
                float f = generationData.GetVelocitySampleAt(i) / 127f;
                Handles.color = XM_EditorUtilities.GetDiagramColor(size, i);
                Handles.DrawLine(GPos(f, 0), GPos(f, 1));
            }

            //draw volume multiplier line
            for (int i = 0; i < size; i++) {
                float f = generationData.GetVelocitySampleAt(i) / 127f;
                float f2 = i == 0 ? 0 : generationData.GetVelocitySampleAt(i - 1) / 127f;
                Handles.color = XM_EditorUtilities.GetDiagramColor(size, i);
                Handles.DrawLine(GPos(f, 1 - f * generationData.volumeMultipliers[i]), GPos(f2, 1 - f2 * generationData.volumeMultipliers[i]));
            }
        }

        private void SetGraphicTarget(Rect rect) {
            graphicRect = rect;
        }

        private Vector2 GPos(float x, float y) {
            return graphicRect.position + new Vector2(x * graphicRect.width, y * graphicRect.height);
        }

        private string GetVelocitySamplesGuide() {
            StringBuilder sb = new("Samples: ");
            int size = generationData.velocitySamples;
            for (int i = 0; i < size; i++) {
                sb.Append(generationData.GetVelocitySampleAt(i));
                if (i < size - 1) sb.Append(',');
                sb.Append(' ');
            }

            sb.Append('(');
            sb.Append(size);
            sb.Append(')');
            return sb.ToString();
        }

        private void EnableVelGeneration() {
            EditorGUILayout.HelpBox("Even-spaced sample structure generation for velocity is disabled for this Virtual Instrument. Would you like to enable even-spaced samples? Caution: this will override the current sample settings!", MessageType.Warning, true);
            if (GUILayout.Button("Enable Even-Spaced Velocity Samples", GUILayout.Width(fieldMaxWidth))) {
                generationData.useEvenVelocitySpacings = true;
                changed = true;
            }
        }
        #endregion

        #region RoundRobinGUI
        private void RoundRobinSettings() {
            generationData.roundRobins = IntField(k_roundRobins, generationData.roundRobins, true);

            if (generationData.useEvenNoteSpacings && generationData.useEvenVelocitySpacings) {
                GUILayout.Space(5);
                i_estimatedTotalSamplesContent.text = generationData.TotalSamples.ToString();
                EditorGUILayout.LabelField(k_estimatedTotalSamples, i_estimatedTotalSamplesContent, GUILayout.Width(fieldMaxWidth));
            }
        }
        #endregion

        private void SaveMenu() {
            if (selected.HasGeneratedSamples()) {
                if (isDirty) {
                    EditorGUILayout.HelpBox("You have unapplied changes!", MessageType.Warning, true);
                }
                EditorGUI.BeginDisabledGroup(!isDirty);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(k_save, GUILayout.Width(buttonWidth))) {
                    SaveChanges();
                }
                if (GUILayout.Button(k_revert, GUILayout.Width(buttonWidth))) {
                    DiscardChanges();
                }
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
            else {
                EditorGUILayout.HelpBox("Press generate to generate sample bindings.", MessageType.Info, true);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(k_generate, GUILayout.Width(buttonWidth))) {
                    SaveChanges();
                }
                if (GUILayout.Button(k_reset, GUILayout.Width(buttonWidth))) {
                    DiscardChanges();
                }
                GUILayout.EndHorizontal();
            }
        }

        public override void SaveChanges() {
            if (selected != null) {
                ValidateVMultipliers();
                selected.ApplyGeneration(generationData);

                Debug.Log("Generated sample bindings!");
                EditorUtility.SetDirty(selected);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                isDirty = false;
            }
            base.SaveChanges();
        }

        public override void DiscardChanges() {
            if(selected != null) {
                isDirty = false;
                generationData = selected.generationData;
            }

            base.DiscardChanges();
        }

        private int IntField(GUIContent content, int defValue, bool onlyPositive = false) {
            int value = EditorGUILayout.IntField(content, defValue, GUILayout.MaxWidth(fieldMaxWidth));
            if (onlyPositive && value <= 0) value = 1;
            if(value != defValue) changed = true;
            return value;
        }

        private float Slider(GUIContent content, float defValue, float min, float max) {
            float value = EditorGUILayout.Slider(content, defValue, min, max, GUILayout.MaxWidth(fieldMaxWidth));
            if (value != defValue) changed = true;
            return value;
        }

        private void NoteSelectField(GUIContent content, int defValue, PianoState targetState) {
            string focusname = $"{targetState}.focus";
            GUI.SetNextControlName(focusname);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(fieldMaxWidth));
            EditorGUILayout.PrefixLabel(content);
            var pos = EditorGUILayout.GetControlRect(true);
            if(EditorGUI.DropdownButton(pos, new GUIContent($"{XM_Utilities.GetNoteString(defValue)} ({defValue})"), FocusType.Passive, pianoState == targetState ? XM_UIStyleManager.selectedDropdown : XM_UIStyleManager.dropdown)) {
                if (pianoState == targetState) pianoState = PianoState.None;
                else pianoState = targetState;
            }
            GUILayout.EndHorizontal();

            if(pianoState == targetState) {
                GUI.FocusControl(focusname);
            }
            else {
                if(GUI.GetNameOfFocusedControl() == focusname) {
                    GUI.FocusControl("");
                }
            }
        }

        private void CreateNewVInst() {
            string path = XM_EditorUtilities.GetActivePathOrFallback(true);

            VirtualInstrumentBinding asset = ScriptableObject.CreateInstance<VirtualInstrumentBinding>();

            string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(Path.Join(path, "New Virtual Instrument Binding.asset"));
            AssetDatabase.CreateAsset(asset, name);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
