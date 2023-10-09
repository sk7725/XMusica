using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica.Envelope {
    public abstract class VirtualEnvelope : ScriptableObject {
        /// <summary>
        /// Amplitude over time after key is pressed.
        /// </summary>
        public abstract float DownAmplitude(float timePassed);

        /// <summary>
        /// Amplitude over time after key is released. Returns -1 after the entire envelope ends.
        /// </summary>
        public abstract float UpAmplitude(float timePassed);
    }
}
