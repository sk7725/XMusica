using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    [CustomPropertyDrawer(typeof(MIDINoteAttribute))]
    public class MIDINoteDrawer : PropertyDrawer {
        GUIContent noteNameContent = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            if (property.propertyType == SerializedPropertyType.Integer) {
                //EditorGUI.Slider(position, property, 1, 10, label);

                var cont = EditorGUI.BeginProperty(position, label, property);
                Rect contentRect = EditorGUI.PrefixLabel(position, cont);
                noteNameContent.text = $"<b>{XM_Utilities.GetNoteString(property.intValue)}</b> ({property.intValue})";
                if (EditorGUI.DropdownButton(contentRect, noteNameContent, FocusType.Passive, XM_UIStyleManager.richTextDropDown)) {
                    MIDINoteWindow.ShowWindow(property);
                }

                EditorGUI.EndProperty();
            }
            else {
                EditorGUI.LabelField(position, label.text, "Only integers are supported as MIDI note representations!");
            }
        }
    }
}