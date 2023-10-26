using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static XMusica.VirtualInstrumentBinding;

namespace XMusica {
    [AddComponentMenu("Audio/XMusica - Simple Instrument Source")]
    public class SimpleInstrumentSource : BaseInstrumentSource {
        public bool stopPlayingOnRelease = false;

        protected override void AfterInitialize() {}

        public override void Press(int note, int velocity) {
            int current = PollSource();
            if(sourceNote[current] != 0) {
                sources[current].Stop();
            }
            sourceNote[current] = (short)note;
            SampleData s = binding.GetSample(note, velocity, out float v, out float p);
            sources[current].pitch = p;
            sources[current].volume = v * volume;
            sources[current].clip = s.clip;
            sources[current].Play();
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