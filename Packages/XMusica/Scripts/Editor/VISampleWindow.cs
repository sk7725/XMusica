using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public class VISampleWindow : EditorWindow {
        #region GUIContent
        static GUIContent i_noBinding = new GUIContent("There is no Virtual Instrument Binding selected. Please select a VIrtual Instrument Binding or create one in the Binder window.");
        static GUIContent i_currentBinding = new GUIContent();
        static readonly GUIContent t_samples = new GUIContent("Sample Matrix");

        static readonly GUIContent k_selectAsset = new GUIContent("Select Asset");
        static readonly GUIContent k_notes = new GUIContent("Notes");
        static readonly GUIContent k_velocities = new GUIContent("Velocities");
        #endregion

        [SerializeField] private VirtualInstrumentBinding selected;
        [SerializeField] private int s_notes, s_vel, s_rr;
        [SerializeField] private Vector2 scrollWindowPos, scrollMatrixPos;

        private static float buttonWidth = 140, border = 3;
        private static float lineHeight = EditorGUIUtility.singleLineHeight, lineWidth = 300, cellBorder = 2;
        private static float noteLabelWidth = 120, velocityLabelHeight = 50;

        #region Base
        [MenuItem("XMusica/Virtual Instrument Sample Assigner", priority = 1)]
        public static void ShowWindow() {
            var window = GetWindow<VISampleWindow>();
            window.titleContent = new GUIContent("VInst Samples", XM_UIStyleManager.vinstSampleWindowIcon);
            window.Show();
        }

        private void OnSelectionChange() {
            Refresh();
        }

        private void OnFocus() {
            Refresh();
        }

        private void Refresh() {
            if (Selection.activeObject is VirtualInstrumentBinding vb) {
                selected = vb;
                vb.GetSampleDimensions(out s_notes, out s_vel, out s_rr);
            }
            else {
                selected = null;
            }
            Repaint();
        }

        private void OnGUI() {
            i_noBinding.image = XM_UIStyleManager.vinstSampleWindowIcon;
            i_currentBinding.image = XM_UIStyleManager.vinstSampleWindowIcon;

            scrollWindowPos = EditorGUILayout.BeginScrollView(scrollWindowPos);
            if (selected != null) {
                SelectedMenuGUI();
            }
            else {
                CreateMenuGUI();
            }
            EditorGUILayout.EndScrollView();
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

            GUILayout.Label(t_samples, EditorStyles.boldLabel, GUILayout.Height(30));

            bool changed = DrawSampleMatrix();

            if (changed) {
                SaveAsset();
            }
        }
        #endregion

        private bool DrawSampleMatrix() {
            float totalWidth = GetMatrixWidth() + border * 2;
            float totalHeight = GetMatrixHeight() + border * 2;

            Rect scrollRect = EditorGUILayout.GetControlRect(true,totalHeight + 18); //+18 accounts for scroll bar
            scrollMatrixPos = GUI.BeginScrollView(scrollRect, scrollMatrixPos, new Rect(0, 0, totalWidth, totalHeight), false, false);
            Rect total = new Rect(border, border, totalWidth - border, totalHeight - border);

            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(total, "");
            GUI.backgroundColor = defColor;

            EditorGUI.BeginChangeCheck();

            //draw cells
            float cw = GetSingleCellWidth();
            float ch = GetSingleCellHeight();

            float x = total.x + noteLabelWidth;
            float y = total.y + velocityLabelHeight;
            for (int i = 0; i < s_notes; i++) {
                for (int j = 0; j < s_vel; j++) {
                    Rect r = new Rect(x + cellBorder, y + cellBorder, cw - cellBorder, ch - cellBorder);
                    Color c = XM_EditorUtilities.GetDiagramColor(s_notes, i, 0.5f, i % 2 == 1 ? 0.6f : 0.8f);
                    DrawCell(r, i, j, c);
                    x += cw;
                }
                x = total.x + noteLabelWidth;
                y += ch;
            }

            //draw note title cells
            y = total.y + velocityLabelHeight + 2;
            for(int i = 0; i < s_notes; i++) {
                Rect r = new Rect(total.x, y, noteLabelWidth, ch);
                GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(s_notes, i, 0.8f, i % 2 == 1 ? 0.2f : 0.3f);
                XM_UIStyleManager.matrixTitle.normal.textColor = XM_UIStyleManager.matrixTitle.hover.textColor = XM_EditorUtilities.GetDiagramColor(s_notes, i, 0.2f);
                GUI.Box(r, $"{XM_Utilities.GetNoteString(selected.Samples[i][0][0].sampleNote)} (#{i})", XM_UIStyleManager.matrixTitle);
                y += ch;
            }

            //draw velocity title cells
            x = total.x + noteLabelWidth + 2;
            for (int j = 0; j < s_vel; j++) {
                Rect r = new Rect(x, total.y, cw, velocityLabelHeight);
                GUI.backgroundColor = XM_EditorUtilities.GetDiagramColor(s_vel, j, 0.8f, 0.3f);
                XM_UIStyleManager.matrixTitle.normal.textColor = XM_UIStyleManager.matrixTitle.hover.textColor = XM_EditorUtilities.GetDiagramColor(s_vel, j, 0.2f);
                GUI.Box(r, $"Velocity {selected.Samples[0][j][0].sampleVelocity} (#{j})", XM_UIStyleManager.matrixTitle);
                x += cw;
            }

            GUI.backgroundColor = defColor;

            //draw legend
            DrawLegend(new Rect(total.x, total.y, noteLabelWidth, velocityLabelHeight));

            bool ret = EditorGUI.EndChangeCheck();
            GUI.EndScrollView(true);

            return ret;
        }

        private void DrawLegend(Rect rect) {
            Handles.color = new Color(0.1f, 0.1f, 0.1f);
            Handles.DrawAAConvexPolygon(rect.min, new Vector3(rect.xMin, rect.yMax), rect.max, new Vector3(rect.xMax, rect.yMin));
            Handles.color = new Color(0.2f, 0.2f, 0.2f);
            Handles.DrawAAConvexPolygon(rect.min, new Vector3(rect.xMax, rect.yMin), rect.max);
            Handles.color = Color.white;
            Handles.Label(rect.center + new Vector2(-40, 5), k_notes);
            Handles.Label(rect.center + new Vector2(0, -15), k_velocities);
        }

        private void DrawCell(Rect rect, int i, int j, Color color) {
            GUI.backgroundColor = color;
            GUI.Box(rect, "");
            GUI.Label(TopRect(rect, lineHeight), $"{XM_Utilities.GetNoteString(selected.Samples[i][j][0].sampleNote)}, v{selected.Samples[i][j][0].sampleVelocity}");

            float y = rect.y + lineHeight;
            for (int k = 0; k < s_rr; k++) {
                selected.Samples[i][j][k].clip = (AudioClip)EditorGUI.ObjectField(new Rect(rect.x, y, rect.width, lineHeight), selected.Samples[i][j][k].clip, typeof(AudioClip), false);
                y += lineHeight;
            }
        }

        private void SaveAsset() {
            EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private float GetMatrixWidth() {
            return GetSingleCellWidth() * s_vel + noteLabelWidth;
        }

        private float GetMatrixHeight() {
            return GetSingleCellHeight() * s_notes + velocityLabelHeight;
        }

        private float GetSingleCellHeight() {
            return (1 + s_rr) * lineHeight + cellBorder * 2 + 5;
        }

        private float GetSingleCellWidth() {
            return lineWidth + cellBorder * 2;
        }

        private Rect LeftRect(Rect rect, float a) {
            return new Rect(rect.x, rect.y, a, rect.height);
        }

        private Rect RightRect(Rect rect, float a) {
            return new Rect(rect.x + rect.width - a, rect.y, a, rect.height);
        }

        private Rect TopRect(Rect rect, float a) {
            return new Rect(rect.x, rect.y, rect.width, a);
        }

        private Rect BottomRect(Rect rect, float a) {
            return new Rect(rect.x, rect.y + rect.height - a, rect.width, a);
        }
    }
}