using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMusica.Envelope;
using static XMusica.VirtualInstrumentBinding;

namespace XMusica {
    [AddComponentMenu("Audio/XMusica - Instrument Source")]
    public class InstrumentSource : BaseInstrumentSource {
        public VirtualEnvelope envelope;
        public bool scaledTime = true;

        protected short[] sourceNote;
        protected bool[] processingPressed;
        protected float[] time; //note that this time is updated every frame because we don't want loss of accuracy as Time.time gets bigger and bigger.
        protected float[] initialVelocity;

        protected override void AfterInitialize() {
            sourceNote = new short[_sourceCount];
            time = new float[_sourceCount];
            processingPressed = new bool[_sourceCount];
            initialVelocity = new float[_sourceCount];
        }

        public override void Press(int note, int velocity) {
            int current = PollSource();
            if (sourceNote[current] != 0) {
                sources[current].Stop();
            }
            sourceNote[current] = (short)note;
            processingPressed[current] = true;
            time[current] = 0;

            SampleData s = binding.GetSample(note, velocity, out float v, out float p);
            sources[current].pitch = p;
            initialVelocity[current] = v;
            sources[current].volume = initialVelocity[current] * envelope.DownAmplitude(0) * volume;
            sources[current].clip = s.clip;
            sources[current].Play();
        }

        public override void Release(int note, int velocity) {
            for (int i = 0; i < _sourceCount; i++) {
                if (sourceNote[i] == note) {
                    processingPressed[i] = false;
                    initialVelocity[i] = envelope.DownAmplitude(time[i]) * initialVelocity[i];
                    time[i] = 0;
                }
            }
        }

        private void Update() {
            for (int i = 0; i < _sourceCount; i++) {
                if (sourceNote[i] != 0) {
                    time[i] += scaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (processingPressed[i]) {
                        //press
                        sources[i].volume = initialVelocity[i] * envelope.DownAmplitude(time[i]) * volume;
                    }
                    else {
                        //release
                        float a = envelope.UpAmplitude(time[i]);
                        if(a == -1) {
                            sourceNote[i] = 0;
                            sources[i].Stop();
                            continue;
                        }
                        sources[i].volume = initialVelocity[i] * a * volume;
                    }
                }
            }
        }
    }
}