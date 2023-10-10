using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XMusica.Envelope;

namespace XMusica.EditorUtilities {
    [CustomEditor(typeof(ADSREnvelope))]
    public class ADSREditor : Editor {
        #region GUIContent
        static readonly GUIContent t_settings = new GUIContent("Settings");
        static readonly GUIContent t_preview = new GUIContent("Preview");

        static readonly GUIContent k_attack = new GUIContent("Attack", "The time it takes for the amplitude to peak.");
        static readonly GUIContent k_decay = new GUIContent("Decay", "The time it takes for the amplitude to transition to sustain.");
        static readonly GUIContent k_sustain = new GUIContent("Sustain", "The sustain amplitude multiplier.");
        static readonly GUIContent k_release = new GUIContent("Release", "The time it takes for the amplitude to fade out completely.");
        #endregion

        private static float border = 3, previewHeight = 100, previewTapeHeight = 24, markerWidth = 9, markerHeight = 9;
        private static float minLabelWidth = 42, minTimeWidth = 23;

        SerializedProperty s_attack, s_decay, s_sustain, s_release;

        private void OnEnable() {
            s_attack = serializedObject.FindProperty("attack");
            s_decay = serializedObject.FindProperty("decay");
            s_sustain = serializedObject.FindProperty("sustain");
            s_release = serializedObject.FindProperty("release");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            GUILayout.Label(t_preview, EditorStyles.boldLabel, GUILayout.Height(30));
            GeneratePreview((ADSREnvelope)target);

            GUILayout.Space(5);
            GUILayout.Label(t_settings, EditorStyles.boldLabel, GUILayout.Height(30));

            EditorGUILayout.PropertyField(s_attack, k_attack);
            EditorGUILayout.PropertyField(s_decay, k_decay);
            EditorGUILayout.PropertyField(s_sustain, k_sustain);
            EditorGUILayout.PropertyField(s_release, k_release);
            serializedObject.ApplyModifiedProperties();
        }

        Rect graphicRect;

        private void GeneratePreview(ADSREnvelope env) {
            float timeScale;
            float maxTimeOccupy = Mathf.Max(env.attack + env.decay, env.release);
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
            if(timeScale == 2f) {
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
            for(int i = 0; i * timeScale <= 10; i++) {
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

            //draw adsr
            Handles.color = Color.gray;
            Handles.DrawDottedLine(GPos(0.5f, 1), GPos(0.5f, 1 - env.sustain), 0.2f);
            Handles.color = Color.red;
            Handles.DrawLine(GPos(0, 1), GPos(env.attack * timeScale, 0));
            Handles.DrawLine(GPos(env.attack * timeScale, 0), GPos((env.attack + env.decay) * timeScale, 1 - env.sustain));
            Handles.DrawLine(GPos((env.attack + env.decay) * timeScale, 1 - env.sustain), GPos(0.5f, 1 - env.sustain));
            Handles.color = Color.white;
            Handles.DrawLine(GPos(0.5f, 1 - env.sustain), GPos(0.5f + timeScale * env.release, 1));
            Handles.DrawLine(GPos(0.5f + timeScale * env.release, 1), GPos(1, 1));

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