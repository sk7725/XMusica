using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public class MIDINoteWindow : EditorWindow {
        private const int EDGE_KEYS = 5;

        private static float offset = 5, border = 3;
        private static float lineHeight = EditorGUIUtility.singleLineHeight, octavebuttonWidth = 30, currentOctaveHeight = 30;
        private static float keyWidth = 25, keyHeight = 120, blackKeyWidth = 18, blackKeyHeight = 70;

        private static Color whiteKeyColor = new Color(0.8f, 0.8f, 0.8f), blackKeyColor = new Color(0.2f, 0.2f, 0.2f);
        private static Color whiteKeySelectedColor = Color.cyan, blackKeySelectedColor = new Color(0f, 0.8f, 0.8f);
        private static Color whiteKeyOutrangeColor = Color.gray, blackKeyOutrangeColor = new Color(0.15f, 0.15f, 0.15f);
        private static Color whiteKeyHoverColor = new Color(0.7f, 0.7f, 0.7f), blackKeyHoverColor = new Color(0.3f, 0.3f, 0.3f);
        private static Color whiteKeyDisabledColor = new Color(0.3f, 0.3f, 0.3f), blackKeyDisabledColor = new Color(0.25f, 0.25f, 0.25f);

        private static GUIContent octaveLabel = new GUIContent("Octave", "Octave of the note. (0~9)");
        private static GUIContent noteLabel = new GUIContent("Note", "Named note value. Use the piano below to select the note.");

        private SerializedObject target;
        private SerializedProperty property;
        private int viewOctave;
        GUIContent noteNameContent = new();

        public static void ShowWindow(SerializedProperty property) {
            var window = GetWindow<MIDINoteWindow>(true);

            window.target = property.serializedObject;
            window.property = property;
            window.viewOctave = XM_Utilities.GetOctave(property.intValue);

            window.titleContent = new GUIContent($"Editing Note: {property.name}");
            window.minSize = new Vector2(keyWidth * (7 + 2 * EDGE_KEYS) + offset * 2 + octavebuttonWidth * 2 + border * 2, lineHeight * 2 + offset * 2 + keyHeight + currentOctaveHeight);
            window.maxSize = window.minSize;

            window.ShowUtility();
        }

        private void OnSelectionChange() {
            Close();
        }

        private void OnGUI() {
            wantsMouseMove = true;
            target.Update();

            GUI.skin.box.alignment = TextAnchor.MiddleCenter;

            int noteValue = property.intValue % 12;
            noteNameContent.text = $"<b>{XM_Utilities.GetNoteString(property.intValue)}</b> ({property.intValue})";
            EditorGUILayout.LabelField(noteLabel, noteNameContent, XM_UIStyleManager.richLabel);
            int prevNote = property.intValue;
            int octave = EditorGUILayout.IntSlider(octaveLabel, XM_Utilities.GetOctave(property.intValue), 0, 9);

            EditorGUILayout.Space(offset);

            var pianoRect = EditorGUILayout.GetControlRect(true, keyHeight);

            pianoRect.y -= offset;

            //note value changer
            OnPianoGUI(pianoRect, prevNote, out int newNote);

            EditorGUILayout.Space(offset);
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(pianoRect.x, pianoRect.y + offset + pianoRect.height, pianoRect.width, currentOctaveHeight), $"Currently viewing octave: {viewOctave}");

            int changedNote;
            if(newNote != prevNote) {
                changedNote = newNote; //piano button pressed
            }
            else {
                changedNote = (octave + 1) * 12 + noteValue; //octave slider pulled
            }

            changedNote = Mathf.Clamp(changedNote, 21, 127);
            if(changedNote != prevNote) {
                property.intValue = changedNote;
                viewOctave = XM_Utilities.GetOctave(changedNote);
                target.ApplyModifiedProperties();
            }

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void OnPianoGUI(Rect total, int currentNote, out int newNote) {
            Event evt = Event.current;
            newNote = currentNote;
            Color defColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(total, "");
            GUI.backgroundColor = defColor;

            EditorGUI.BeginDisabledGroup(viewOctave <= 0);
            bool leftPressed = GUI.Button(LeftRect(total, octavebuttonWidth), "<");
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(viewOctave >= 9);
            bool rightPressed = GUI.Button(RightRect(total, octavebuttonWidth), ">");
            EditorGUI.EndDisabledGroup();

            int keyPressed = -1;

            //white keys
            float x = total.x + octavebuttonWidth + offset;
            for(int i = 0; i < EDGE_KEYS * 2 + 7; i++) {
                int now = GetNoteOfWhiteKey(i);

                //mouse hover
                int j = (i + 7 - EDGE_KEYS) % 7;
                bool hasLeftBit = j == 3 || j == 0 || i == 0; bool hasRightBit = j == 2 || j == 6 || i == EDGE_KEYS * 2 + 6;
                Rect r1 = new Rect(x, total.y + blackKeyHeight, keyWidth + 1f, total.height - blackKeyHeight - border);
                Rect r2 = new Rect(hasLeftBit ? x : x + blackKeyWidth * 0.5f, total.y, keyWidth - blackKeyWidth * ((hasLeftBit ? 0 : 0.5f) + (hasRightBit ? 0 : 0.5f)) + 1f, blackKeyHeight);
                bool hovered = r1.Contains(evt.mousePosition) || r2.Contains(evt.mousePosition);

                //color
                if(now == currentNote) {
                    GUI.backgroundColor = whiteKeySelectedColor;
                }
                else if(now < 21 || now > 127) {
                    GUI.backgroundColor = whiteKeyDisabledColor;
                }
                else {
                    if (hovered) GUI.backgroundColor = (i < EDGE_KEYS || i >= EDGE_KEYS + 7) ? whiteKeyOutrangeColor * 0.9f : whiteKeyHoverColor;
                    else GUI.backgroundColor = (i < EDGE_KEYS || i >= EDGE_KEYS + 7) ? whiteKeyOutrangeColor : whiteKeyColor;
                }

                //render
                bool b1 = GUI.Button(r1, XM_Utilities.GetNoteString(now), XM_UIStyleManager.whitePianoKey);
                bool b2 = GUI.Button(r2, "", XM_UIStyleManager.whitePianoKey);
                if ((b1 || b2) && (now >= 21 && now <= 127)) keyPressed = now;
                x += keyWidth;
            }

            //black keys
            x = total.x + octavebuttonWidth + offset + keyWidth * 0.5f + (keyWidth - blackKeyWidth) * 0.5f;
            for (int i = 0; i < EDGE_KEYS * 2 + 7 - 1; i++) {
                int j = (i + 7 - EDGE_KEYS) % 7;
                if (j == 2 || j == 6) {
                    x += keyWidth;
                    continue;
                }
                int now = GetNoteOfBlackKey(i);

                //mouse hover
                Rect r1 = new Rect(x - 0.5f, total.y, blackKeyWidth + 0.5f, blackKeyHeight);
                bool hovered = r1.Contains(evt.mousePosition);

                //color
                if (now == currentNote) {
                    GUI.backgroundColor = blackKeySelectedColor;
                }
                else if (now < 21 || now > 127) {
                    GUI.backgroundColor = blackKeyDisabledColor;
                }
                else {
                    if (hovered) GUI.backgroundColor = (i < EDGE_KEYS || i >= EDGE_KEYS + 7) ? blackKeyOutrangeColor + new Color(0.1f, 0.1f, 0.1f) : blackKeyHoverColor;
                    else GUI.backgroundColor = (i < EDGE_KEYS || i >= EDGE_KEYS + 7) ? blackKeyOutrangeColor : blackKeyColor;
                }

                //render
                bool b = GUI.Button(r1, "", XM_UIStyleManager.blackPianoKey);
                if(b && (now >= 21 && now <= 127)) keyPressed = now;
                x += keyWidth;
            }

            GUI.backgroundColor = blackKeyColor;
            GUI.Box(new Rect(total.x + octavebuttonWidth, total.y, offset, total.height), "", XM_UIStyleManager.blackPianoKey);
            GUI.Box(new Rect(total.x + total.width - octavebuttonWidth - offset, total.y, offset, total.height), "", XM_UIStyleManager.blackPianoKey);

            if (keyPressed != -1) newNote = keyPressed;

            if (leftPressed) viewOctave -= 1;
            if (rightPressed) viewOctave += 1;
            viewOctave = Mathf.Clamp(viewOctave, 0, 9);
            GUI.backgroundColor = defColor;
        }

        private static int[] whiteToNote = {-8, -7, -5, -3, -1, 0, 2, 4, 5, 7, 9, 11, 12, 14, 16, 17, 19};

        private int GetNoteOfWhiteKey(int index) {
            return whiteToNote[index] + 12 * (viewOctave + 1);
        }

        private int GetNoteOfBlackKey(int index) {
            return whiteToNote[index] + 12 * (viewOctave + 1) + 1;
        }

        private Rect LeftRect(Rect rect, float a) {
            return new Rect(rect.x, rect.y, a, rect.height);
        }

        private Rect RightRect(Rect rect, float a) {
            return new Rect(rect.x + rect.width - a, rect.y, a, rect.height);
        }
    }
}
