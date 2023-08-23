using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.Editor {
    public class VirtualInstrumentBindingWindow : EditorWindow {
        [MenuItem("XMusica/Virtual Instrument Binder")]
        public static void ShowWindow() {
            var window = GetWindow<VirtualInstrumentBindingWindow>();
            window.titleContent = new GUIContent("VInst Binder", (Texture2D)EditorGUIUtility.Load("xmusica.vinst_window.icon.png"));
            window.Show();
        }
    }
}
