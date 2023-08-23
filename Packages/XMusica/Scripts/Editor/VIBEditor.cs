using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XMusica.EditorUtilities {
    [CustomEditor(typeof(VirtualInstrumentBinding))]
    public class VIBEditor : Editor {
        SerializedProperty samples, boundNote, boundVelocity;

        private void OnEnable() {
            samples = serializedObject.FindProperty("samples");
            boundNote = serializedObject.FindProperty("boundNote");
            boundVelocity = serializedObject.FindProperty("boundVelocity");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            //todo
            serializedObject.ApplyModifiedProperties();
        }
    }
}