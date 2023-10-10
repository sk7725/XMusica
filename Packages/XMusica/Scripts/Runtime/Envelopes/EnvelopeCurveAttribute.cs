using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica.Envelope {
    public class EnvelopeCurveAttribute : PropertyAttribute {
        public bool release;

        public EnvelopeCurveAttribute(bool release) {
            this.release = release;
        }
    }
}
