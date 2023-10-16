using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica.Envelope {
    [CreateAssetMenu(fileName = "New ADSR Envelope", menuName = "XMusica/Envelopes/ADSR Envelope", order = 225)]
    public class ADSREnvelope : VirtualEnvelope {
        [Range(0f, 3f)] public float attack = 0.01f;
        [Range(0f, 3f)] public float decay = 0.15f;
        [Range(0f, 1f)] public float sustain = 0.3f;
        [Range(0f, 5f)] public float release = 0.4f;

        public override float DownAmplitude(float timePassed) {
            if(timePassed < attack) {
                return timePassed / attack;
            }
            timePassed -= attack;
            if(timePassed < decay) {
                float t = timePassed / decay;
                return 1 + t * (-1 + sustain);
            }
            return sustain;
        }

        public override float UpAmplitude(float timePassed) {
            if(timePassed < release) {
                return 1 - timePassed / release;
            }
            return -1; // end
        }
    }
}
