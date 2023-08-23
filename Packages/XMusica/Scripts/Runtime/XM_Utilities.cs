using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XMusica {
    public static class XM_Utilities {
        private const string NOTE_STR = "CCDDEFFGGAAB";

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