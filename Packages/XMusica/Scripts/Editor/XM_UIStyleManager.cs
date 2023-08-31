using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public static class XM_UIStyleManager {
        public static GUIStyle whitePianoKey;
        public static GUIStyle blackPianoKey;
        public static GUIStyle dropdown, selectedDropdown, richTextDropDown, richLabel;
        public static GUIStyle matrixTitle;

        public static Texture2D vinstBinderWindowIcon;
        public static Texture2D vinstSampleWindowIcon;
        public static Texture2D vinstTesterWindowIcon;
        public static Texture2D whiteKeyTexture;

        static XM_UIStyleManager() {
            var path = XM_EditorUtilities.packageRelativePath;

            vinstBinderWindowIcon = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/vinst_binder_window.png", typeof(Texture2D)) as Texture2D;
            vinstSampleWindowIcon = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/vinst_sample_window.png", typeof(Texture2D)) as Texture2D;
            vinstTesterWindowIcon = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/vinst_tester_window.png", typeof(Texture2D)) as Texture2D;
            whiteKeyTexture = AssetDatabase.LoadAssetAtPath(path + "/Editor Resources/Textures/xmusica_key_white.png", typeof(Texture2D)) as Texture2D;

            whitePianoKey = new GUIStyle();
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

            blackPianoKey = new GUIStyle();
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

            richTextDropDown = new GUIStyle(EditorStyles.miniPullDown);
            richTextDropDown.richText = true;
            richLabel = new GUIStyle(EditorStyles.label);
            richLabel.richText = true;

            matrixTitle = new GUIStyle(EditorStyles.boldLabel);
            matrixTitle.normal.background = matrixTitle.hover.background = matrixTitle.focused.background = matrixTitle.active.background = Texture2D.whiteTexture;
            matrixTitle.alignment = TextAnchor.MiddleCenter;
        }
    }
}