using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.Editor {
    public class VirtualInstrumentBindingWindow : EditorWindow {
        [MenuItem("XMusica/Virtual Instrument Binder")]
        public static void ShowWindow() {
            Debug.Log(XM_EditorUtilities.packageRelativePath);

            var window = GetWindow<VirtualInstrumentBindingWindow>();
            window.titleContent = new GUIContent("VInst Binder", XM_UIStyleManager.vinstBinderWindowIcon);
            window.Show();
        }
    }
}
