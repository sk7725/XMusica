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
        #endregion

        [SerializeField]
        private VirtualInstrumentBinding selected;
        [SerializeField] private Vector2 scrollWindowPos, scrollMatrixPos;

        private static float fieldMaxWidth = 400, buttonWidth = 140;

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
                if (selected != vb) {
                    selected = vb;
                }
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
            EditorGUI.BeginChangeCheck();

            DrawSampleMatrix();

            if (EditorGUI.EndChangeCheck()) {
                Debug.Log("uwu");
                SaveAsset();
            }
        }
        #endregion

        private void DrawSampleMatrix() {
            //todo
        }

        private void SaveAsset() {
            EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private float GetMatrixWidth() {
            return 700;//todo
        }

        private float GetMatrixHeight() {
            return 400;//todo
        }
    }
}