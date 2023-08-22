using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XMusica.Editor {
    [CustomPropertyDrawer(typeof(MIDINoteAttribute))]
    public class MIDINoteDrawer : PropertyDrawer {
        private const string NOTE_STR = "CCDDEFFGGAAB";

        GUIContent noteNameContent = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            if (property.propertyType == SerializedPropertyType.Integer) {
                //EditorGUI.Slider(position, property, 1, 10, label);

                var cont = EditorGUI.BeginProperty(position, label, property);
                Rect contentRect = EditorGUI.PrefixLabel(position, cont);
                noteNameContent.text = GetNoteString(property.intValue);
                if (EditorGUI.DropdownButton(contentRect, noteNameContent, FocusType.Passive)) {
                    MIDINoteWindow.ShowWindow(property);
                }

                EditorGUI.EndProperty();
            }
            else {
                EditorGUI.LabelField(position, label.text, "Only integers are supported as MIDI note representations!");
            }
        }

        private string GetNoteString(int note) {
            if (note < 21 || note > 127) return note.ToString();
            int n = note / 12 - 1;
            int m = note % 12;

            char ch = NOTE_STR[m];
            StringBuilder sb = new StringBuilder(3);
            sb.Append(ch);
            if (m == 1 || m == 3 || m == 6 || m == 8 || m == 10) sb.Append('#');
            sb.Append(n);
            return sb.ToString();
        }
    }
}