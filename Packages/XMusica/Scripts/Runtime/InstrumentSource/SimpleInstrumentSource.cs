using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica {
    [AddComponentMenu("Audio/XMusica - Simple Instrument Source")]
    public class SimpleInstrumentSource : BaseInstrumentSource {
        protected short[] sourceNote;

        protected override void AfterInitialize() {
            sourceNote = new short[_sourceCount];
        }

        protected override void Play(bool pressed, int note, int velocity) {
            throw new System.NotImplementedException();
        }
    }
}