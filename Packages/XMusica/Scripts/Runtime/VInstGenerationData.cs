using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica {
    /// <summary>
    /// Holds generation data of a VInst binding.
    /// </summary>
    [System.Serializable]
    public struct VInstGenerationData {
        public static VInstGenerationData Default = new VInstGenerationData(23, 127, 6, 1, 1);

        public VInstGenerationData(int noteStartPos, int noteEndCutoff, int noteSampleDist, int velocitySamples, int roundRobins) {
            useEvenNoteSpacings = true;
            this.noteStartPos = noteStartPos;
            this.noteEndCutoff = noteEndCutoff;
            this.noteSampleDist = noteSampleDist;

            useEvenVelocitySpacings = true;
            this.velocitySamples = velocitySamples;
            volumeMultipliers = new float[velocitySamples];
            Array.Fill(volumeMultipliers, 1f);

            this.roundRobins = roundRobins;
        }

        //notes
        public bool useEvenNoteSpacings; //if this is false, the generation values below are not valid
        public int noteStartPos;
        public int noteEndCutoff;
        public int noteSampleDist;

        //velocity
        public bool useEvenVelocitySpacings;
        public int velocitySamples;

        //volume multiplier settings
        public float[] volumeMultipliers;

        //round robin
        public int roundRobins;

        public int GetNoteSampleAt(int index) {
            return noteStartPos + index * noteSampleDist;
        }

        public int GetVelocitySampleAt(int index) {
            return 127 - (velocitySamples - index - 1) * 127 / velocitySamples;
        }

        public int NoteSamples {
            get {
                if (noteEndCutoff < noteStartPos) return 0;
                return (Mathf.Min(127, noteEndCutoff) - noteStartPos) / noteSampleDist + 1;
            }
        }

        public int TotalSamples {
            get {
                return NoteSamples * velocitySamples * roundRobins;
            }
        }
    }
}