using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XMusica.EditorUtilities {
    [CustomEditor(typeof(VirtualInstrumentBinding))]
    public class VIBEditor : Editor {
        static GUIContent k_bindingWindow = new GUIContent("Open Binder Window");
        static GUIContent k_sampleWindow = new GUIContent("Open Sample Assigner Window");
        static readonly GUIContent k_iKnow = new GUIContent("Enable Baked Index Editing");
        static readonly GUIContent t_editBinding = new GUIContent("Edit Binding");
        static readonly GUIContent t_bindingData = new GUIContent("Binding Info");

        private static float buttonHeight = 30;

        SerializedProperty samples, boundNote, boundVelocity;
        bool iKnowWhatImDoing;

        private void OnEnable() {
            samples = serializedObject.FindProperty("samples");
            boundNote = serializedObject.FindProperty("boundNote");
            boundVelocity = serializedObject.FindProperty("boundVelocity");
            iKnowWhatImDoing = false;
        }

        public override void OnInspectorGUI() {
            k_bindingWindow.image = XM_UIStyleManager.vinstBinderWindowIcon;
            k_sampleWindow.image = XM_UIStyleManager.vinstSampleWindowIcon;

            serializedObject.Update();

            GUILayout.Label(t_editBinding, EditorStyles.boldLabel, GUILayout.Height(30));
            if (GUILayout.Button(k_bindingWindow, GUILayout.Height(buttonHeight))){
                Selection.activeObject = serializedObject.targetObject;
                VIBWindow.ShowWindow();
            }

            if (GUILayout.Button(k_sampleWindow, GUILayout.Height(buttonHeight))) {
                Selection.activeObject = serializedObject.targetObject;
                VISampleWindow.ShowWindow();
            }

            GUILayout.Label(t_bindingData, EditorStyles.boldLabel, GUILayout.Height(30));

            string samplesText;
            if(serializedObject.targetObject == null) {
                samplesText = "No sample matrix detected.";
            }
            else {
                VirtualInstrumentBinding vib = (VirtualInstrumentBinding)serializedObject.targetObject;
                vib.GetSampleDimensions(out int n, out int m, out int r);
                samplesText = $"{n}x{m}x{r} sample matrix, total of {n*m*r} samples.";
            }
            EditorGUILayout.LabelField("Samples", samplesText);

            GUILayout.Space(5);
            //show default
            EditorGUILayout.HelpBox("Do not change the values below directly unless you know what you are doing!", MessageType.Warning, true);
            if (GUILayout.Button(k_iKnow)) {
                iKnowWhatImDoing = true;
            }
            EditorGUI.BeginDisabledGroup(!iKnowWhatImDoing);
            EditorGUILayout.PropertyField(boundNote);
            EditorGUILayout.PropertyField(boundVelocity);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}