using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.Editor {
    public static class XM_UIStyleManager {
        public static GUIStyle whitePianoKey;
        public static GUIStyle blackPianoKey;
        public static GUIStyle dropdown, selectedDropdown;

        public static Texture2D vinstBinderWindowIcon;
        public static Texture2D whiteKeyTexture;

        static XM_UIStyleManager() {
            var path = XM_EditorUtilities.packageRelativePath;

            vinstBinderWindowIcon = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/vinst_binder_window.png", typeof(Texture2D)) as Texture2D;
            whiteKeyTexture = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/key_white.png", typeof(Texture2D)) as Texture2D;

            whitePianoKey = new GUIStyle("button");
            whitePianoKey.normal.textColor = Color.black;
            whitePianoKey.normal.background = whiteKeyTexture;
            whitePianoKey.active.textColor = Color.black;
            whitePianoKey.active.background = whiteKeyTexture;
            whitePianoKey.hover.textColor = Color.black;
            whitePianoKey.hover.background = whiteKeyTexture;
            whitePianoKey.focused.textColor = Color.black;
            whitePianoKey.focused.background = whiteKeyTexture;
            whitePianoKey.alignment = TextAnchor.LowerCenter;
            whitePianoKey.clipping = TextClipping.Overflow;

            blackPianoKey = new GUIStyle("button");
            blackPianoKey.normal.background = whiteKeyTexture;
            blackPianoKey.active.background = whiteKeyTexture;
            blackPianoKey.hover.background = whiteKeyTexture;
            blackPianoKey.focused.background = whiteKeyTexture;

            dropdown = new GUIStyle(EditorStyles.miniButton);
            dropdown.alignment = TextAnchor.MiddleLeft;
            selectedDropdown = new GUIStyle(EditorStyles.miniButton);
            selectedDropdown.alignment = TextAnchor.MiddleLeft;
            selectedDropdown.normal.textColor = selectedDropdown.hover.textColor = selectedDropdown.focused.textColor = selectedDropdown.active.textColor = Color.yellow;
            selectedDropdown.fontStyle = FontStyle.BoldAndItalic;
        }
    }
}