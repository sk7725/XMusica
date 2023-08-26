using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XMusica {
    public static class XM_Utilities {
        private const string NOTE_STR = "CCDDEFFGGAAB";

        /// <summary>
        /// Returns the octave of the note. For example, it will return 0 for A0(21) and 9 for F#9(126).
        /// </summary>
        /// <param name="note">The note number as a midi note integer. (21-127; A0-G9)</param>
        public static int GetOctave(int note) {
            return note / 12 - 1;
        }

        public static string GetNoteString(int note) {
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

        public static int FromNoteString(string note) {
            if (note.Length < 2 || note.Length > 3) return -1;
            //parse octave
            int oct = (note.Length == 2 ? note[1] : note[2]) - '0';
            if(oct < 0 || oct > 9) return -1;

            //parse sharp
            int offset = 0;
            if (note.Length == 3) {
                if (note[1] == '#') offset = 1;
                else if (note[1] == 'b') offset = -1;
                else return -1;
            }

            //parse note
            int n;
            switch (note.ToLower()[0]) {
                case 'c':
                    n = 0;
                    break;
                case 'd':
                    n = 2;
                    break;
                case 'e':
                    n = 4;
                    break;
                case 'f':
                    n = 5;
                    break;
                case 'g':
                    n = 7;
                    break;
                case 'a':
                    n = 9;
                    break;
                case 'b':
                    n = 11;
                    break;
                default: return -1;
            }

            return n + offset + (oct + 1) * 12;
        }

        public static int GetNoteSamplesRequired(VInstGenerationData data) {
            if (data.noteEndCutoff < data.noteStartPos) return 0;
            return (Mathf.Min(127, data.noteEndCutoff) - data.noteStartPos) / data.noteSampleDist + 1;
        }

        //returns whether the given note is a sample holding note. index if the given note holds a sample. -1 otherwise.
        public static int GetEstimatedSampleNoteIndex(int note, VInstGenerationData data) {
            int j = note - data.noteStartPos;
            if (j < 0 || note > data.noteEndCutoff) return -1;
            if (j % data.noteSampleDist == 0) return j / data.noteSampleDist;
            return -1;
        }

        //returns the closest sample holding note's sample index.
        public static int GetEstimatedReferenceNoteIndex(int note, VInstGenerationData data) {
            int size = GetNoteSamplesRequired(data);
            if (size == 0) return -1;
            int j = note - data.noteStartPos;
            int index = j / data.noteSampleDist;
            if (j < 0) index = -1;
            if (j > -data.noteSampleDist && (j + data.noteSampleDist) % data.noteSampleDist > data.noteSampleDist / 2) index++;

            return Mathf.Clamp(index, 0, size - 1);
        }
    }
}