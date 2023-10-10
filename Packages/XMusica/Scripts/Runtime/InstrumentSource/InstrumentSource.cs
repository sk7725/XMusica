using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMusica.Envelope;

namespace XMusica {
    [AddComponentMenu("Audio/XMusica - Instrument Source")]
    public class InstrumentSource : BaseInstrumentSource {
        public VirtualEnvelope envelope;

        protected short[] sourceNote;
        protected bool[] processingPressed;
        protected float[] time; //note that this time is updated every frame because we don't want loss of accuracy as Time.time gets bigger and bigger.

        protected override void AfterInitialize() {
            sourceNote = new short[_sourceCount];
            time = new float[_sourceCount];
            processingPressed = new bool[_sourceCount];
        }

        public override void Press(int note, int velocity) {
            throw new System.NotImplementedException(); //todo
        }

        public override void Release(int note, int velocity) {
            throw new System.NotImplementedException(); //todo
        }
    }
}