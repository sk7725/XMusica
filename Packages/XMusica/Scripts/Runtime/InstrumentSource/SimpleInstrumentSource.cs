using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static XMusica.VirtualInstrumentBinding;

namespace XMusica {
    [AddComponentMenu("Audio/XMusica - Simple Instrument Source")]
    public class SimpleInstrumentSource : BaseInstrumentSource {
        public bool stopPlayingOnRelease = false;
        protected short[] sourceNote;

        protected override void AfterInitialize() {
            sourceNote = new short[_sourceCount];
        }

        public override void Press(int note, int velocity) {
            int currrent = PollSource();
            if(sourceNote[currrent] != 0) {
                sources[currrent].Stop();
            }
            sourceNote[currrent] = (short)note;
            SampleData s = binding.GetSample(note, velocity, out float v, out float p);
            sources[currrent].pitch = p;
            sources[currrent].volume = v * volume;
            sources[currrent].clip = s.clip;
            sources[currrent].Play();
        }

        public override void Release(int note, int velocity) {
            for (int i = 0; i < _sourceCount; i++) {
                if (sourceNote[i] == note) {
                    sourceNote[i] = 0;
                    if(stopPlayingOnRelease) sources[i].Stop();
                }
            }
        }
    }
}