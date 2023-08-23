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

        //round robin
        public int roundRobins;

        public int GetVelocitySampleAt(int index) {
            return 127 - (velocitySamples - index - 1) * 127 / velocitySamples;
        }
    }
}