using UnityEngine;
using UnityEditor;
using XMusica.Envelope;

namespace XMusica.EditorUtilities {
    [CustomPropertyDrawer(typeof(EnvelopeCurveAttribute))]
    public class EnvelopeCurveDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EnvelopeCurveAttribute curve = attribute as EnvelopeCurveAttribute;
            if (property.propertyType == SerializedPropertyType.AnimationCurve) {
                EditorGUI.CurveField(position, property, curve.release ? Color.white : Color.red, new Rect(0, 0, 1, 1), label);
            }
        }
    }
}