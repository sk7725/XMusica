using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XMusica.Envelope;

namespace XMusica.EditorUtilities {
    [CustomEditor(typeof(CurveEnvelope))]
    public class CurveEnvelopeEditor : Editor {
        #region GUIContent
        static readonly GUIContent t_settings = new GUIContent("Settings");
        static readonly GUIContent t_preview = new GUIContent("Preview");

        static readonly GUIContent k_pressCurve = new GUIContent("Press Curve", "The amplitude over time after the key is pressed.");
        static readonly GUIContent k_pressDuration = new GUIContent("Press Curve Duration", "The length of the curve in seconds.");
        static readonly GUIContent k_releaseCurve = new GUIContent("Release Curve", "The amplitude over time after the key is released. Multiplied by the last value of the press curve (the sustain value).");
        static readonly GUIContent k_releaseDuration = new GUIContent("Release Curve Duration", "The time it takes for the amplitude to fade out completely.");
        #endregion

        private static float border = 3, previewHeight = 100, previewTapeHeight = 24, markerWidth = 9, markerHeight = 9;
        private static float minLabelWidth = 42, minTimeWidth = 23;

        SerializedProperty s_pressCurve, s_pressDuration, s_releaseCurve, s_releaseDuration;

        private void OnEnable() {
            s_pressCurve = serializedObject.FindProperty("pressCurve");
            s_pressDuration = serializedObject.FindProperty("pressCurveDuration");
            s_releaseCurve = serializedObject.FindProperty("releaseCurve");
            s_releaseDuration = serializedObject.FindProperty("releaseCurveDuration");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            GUILayout.Label(t_preview, EditorStyles.boldLabel, GUILayout.Height(30));
            GeneratePreview((CurveEnvelope)target);

            GUILayout.Space(5);
            GUILayout.Label(t_settings, EditorStyles.boldLabel, GUILayout.Height(30));

            EditorGUILayout.PropertyField(s_pressCurve, k_pressCurve);
            EditorGUILayout.PropertyField(s_pressDuration, k_pressDuration);
            EditorGUILayout.PropertyField(s_releaseCurve, k_releaseCurve);
            EditorGUILayout.PropertyField(s_releaseDuration, k_releaseDuration);
            serializedObject.ApplyModifiedProperties();
        }

        Rect graphicRect;

        private void GeneratePreview(CurveEnvelope env) {
            float timeScale;
            float maxTimeOccupy = Mathf.Max(env.pressCurveDuration, env.releaseCurveDuration);
            if (maxTimeOccupy < 0.22f) timeScale = 2f;
            else if (maxTimeOccupy < 0.45f) timeScale = 1f;
            else timeScale = 1 / (Mathf.Ceil(maxTimeOccupy + 0.1f) * 2);

            Rect scrollRect = EditorGUILayout.GetControlRect(true, previewHeight + border * 2 + previewTapeHeight);
            Rect total = new Rect(scrollRect.x + border, scrollRect.y + border, scrollRect.width - border, scrollRect.height - border - previewTapeHeight);

            bool newlineTimeTitle = total.width * 0.1f < minLabelWidth;
            bool newlineTimeContent = total.width * 0.1f < minTimeWidth;

            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(total, "");
            SetGraphicTarget(total);

            //shading per 0.1
            Handles.color = new Color(0, 0, 0, 0.3f);
            if (timeScale == 2f) {
                for (int i = 0; i < 3; i++) {
                    float f = i * 0.4f + 0.2f;
                    float f2 = i * 0.4f + 0.4f;
                    //GUI.Box(new Rect(total.x + f1 * total.width, total.y, total.width * 0.2f, total.height), "");
                    Handles.DrawAAConvexPolygon(GPos(f, 0), GPos(f, 1), GPos(f2, 1), GPos(f2, 0));
                }
            }
            else {
                for (int i = 0; i < 5; i++) {
                    float f = i * 0.2f + 0.1f;
                    float f2 = i * 0.2f + 0.2f;
                    GUI.backgroundColor = Color.black;
                    Handles.DrawAAConvexPolygon(GPos(f, 0), GPos(f, 1), GPos(f2, 1), GPos(f2, 0));
                }
            }

            //draw timeline
            Handles.color = Color.white;
            for (int i = 0; i * timeScale <= 10; i++) {
                float l = i % 10 == 0 ? 9f : i % 5 == 0 ? 7f : 4f;
                float x = i * 0.1f * timeScale;
                Handles.DrawLine(GPos(x, 1), GPos(x, 1) + Vector2.up * l);
            }

            DrawMarker(0, 1);
            DrawMarker(0.5f, 1);

            Handles.Label(GPos(0, 1) + new Vector2(-5f, 15f), "Press");
            Handles.Label(GPos(0.5f, 1) + new Vector2(-5f, 15f), "Release");

            Handles.color = Color.gray;
            for (int i = 1; i < 5; i++) {
                string s = string.Format("{0:0.0}", i * 0.1f / timeScale);
                Handles.Label(GPos(i * 0.1f, 0f) + Vector2.up * (newlineTimeContent && i % 2 == 0 ? 15f : 2f), s);
                Handles.Label(GPos(i * 0.1f + 0.5f, 0f) + Vector2.up * (newlineTimeContent && i % 2 == 0 ? 15f : 2f), s);
            }

            //draw curve
            float sus = env.Sustain;
            Handles.color = Color.gray;
            Handles.DrawDottedLine(GPos(0.5f, 1), GPos(0.5f, 1 - sus), 0.2f);

            //probably not the best way to draw a bezier curve, but i do not giveth a fuketh
            Handles.color = Color.red;
            int segments = 60;
            Vector2 prev = GPos(0, 1 - env.DownAmplitude(0));
            for(int i = 1; i <= segments; i++) {
                float t = (i / (float)segments * 0.5f) / timeScale;
                Vector2 next = GPos(i / (float)segments * 0.5f, 1 - env.DownAmplitude(t));
                Handles.DrawLine(prev, next);
                prev = next;
            }

            Handles.color = Color.white;
            prev = GPos(0.5f, 1 - env.UpAmplitude(0) * sus);
            for (int i = 1; i <= segments; i++) {
                float t = (i / (float)segments * 0.5f) / timeScale;
                if(t >= env.releaseCurveDuration) {
                    Handles.DrawLine(prev, GPos(i / (float)segments * 0.5f + 0.5f, 1));
                    Handles.color = Color.gray;
                    Handles.DrawDottedLine(GPos(i / (float)segments * 0.5f + 0.5f, 1), GPos(1, 1), 0.2f);
                    break;
                }
                Vector2 next = GPos(i / (float)segments * 0.5f + 0.5f, 1 - env.UpAmplitude(t) * sus);
                Handles.DrawLine(prev, next);
                prev = next;
            }

            GUI.backgroundColor = defColor;
        }

        private void SetGraphicTarget(Rect rect) {
            graphicRect = rect;
        }

        private Vector2 GPos(float x, float y) {
            return graphicRect.position + new Vector2(x * graphicRect.width, y * graphicRect.height);
        }

        private void DrawMarker(float x, float y) {
            Vector2 pos = graphicRect.position + new Vector2(x * graphicRect.width, y * graphicRect.height);
            Handles.DrawAAConvexPolygon(pos, pos + new Vector2(-markerWidth * 0.5f, markerHeight), pos + new Vector2(markerWidth * 0.5f, markerHeight));
        }
    }
}