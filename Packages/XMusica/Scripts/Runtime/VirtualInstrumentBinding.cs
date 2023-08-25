using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica {
    public sealed class VirtualInstrumentBinding : ScriptableObject, ISerializationCallbackReceiver {
        private const int MIN_NOTE = 21, MAX_NOTE = 127;
        /// <summary>
        /// Converts requested note -> sample id(i)
        /// </summary>
        [SerializeField] private int[] boundNote = new int[107];
        /// <summary>
        /// Converts requested velocity -> sample id(j)
        /// </summary>
        [SerializeField] private int[] boundVelocity = new int[128];
        [SerializeField] public VInstGenerationData generationData = VInstGenerationData.Default;

        private SampleData[][][] samples = { };

        public SampleData[][][] Samples => samples;

        private int roundRobin;

        [System.Serializable]
        public struct SampleData {
            public AudioClip clip;
            public int sampleNote;
            public int sampleVelocity; //note that if velocity > sampleVelocity, velocity representation will no longer be accurate
            public float volumeMultiplier; //same here; it must be <=1
        }

        private void GetBindInput(int note, int velocity, out int boundNote, out int boundVelocity) {
            boundNote = this.boundNote[note - MIN_NOTE];
            boundVelocity = this.boundVelocity[velocity];
        }

        private SampleData GetBindData(int note, int velocity) {
            return samples[boundNote[note - MIN_NOTE]][boundVelocity[velocity]][PollRoundRobin()];
        }

        private int PollRoundRobin() {
            int l = samples[0][0].Length;
            if (l <= 1) return 0;
            roundRobin = (roundRobin + 1) % l;
            return roundRobin;
        }

        /// <summary>
        /// Returns whether the virtual instrument has valid binding.
        /// </summary=
        public bool HasGeneratedSamples() {
            return samples != null && samples.Length > 0;
        }

        /// <summary>
        /// Returns a SampleData and the suggested volume and pitch offsets to play this sample at.
        /// </summary>
        /// <param name="note">Note index (21-127)</param>
        /// <param name="velocity">Velocity (0-127)</param>
        /// <param name="volume">The suggested volume to play the clip at</param>
        /// <param name="pitch">The suggested pitch to play the clip at</param>
        /// <returns>The sample data. Access the clip with sampleData.clip</returns>
        public SampleData GetSample(int note, int velocity, out float volume, out float pitch) {
            SampleData sample = GetBindData(note, velocity);
            volume = (velocity / 127f) * sample.volumeMultiplier;
            pitch = Mathf.Pow(2, (note - sample.sampleNote) / 12f);
            return sample;
        }

        /// <summary>
        /// Plays an AudioSource with the requested note and velocity.
        /// </summary>
        /// <param name="note">Note index (21-127)</param>
        /// <param name="velocity">Velocity (0-127)</param>
        public void PlayAudioSource(int note, int velocity, AudioSource source) {
            SampleData s = GetSample(note, velocity, out float v, out float p);
            source.pitch = p;
            source.volume = v;
            source.clip = s.clip;
            source.Play();
        }

        /// <summary>
        /// Returns a SampleData and the suggested volume and pitch offsets to play this sample at. Supports float note index and velocity.
        /// </summary>
        /// <param name="note">Note index (21.0-127.0)</param>
        /// <param name="velocity">Velocity (0.0-127.0)</param>
        /// <param name="volume">The suggested volume to play the clip at</param>
        /// <param name="pitch">The suggested pitch to play the clip at</param>
        /// <returns>The sample data. Access the clip with sampleData.clip</returns>
        public SampleData GetHiDefSample(float note, float velocity, out float volume, out float pitch) {
            SampleData sample = GetBindData(Mathf.FloorToInt(note), Mathf.FloorToInt(velocity));
            volume = (velocity / 127f) * sample.volumeMultiplier;
            pitch = Mathf.Pow(2, (note - sample.sampleNote) / 12f);
            return sample;
        }

        /// <summary>
        /// Plays an AudioSource with the requested note and velocity. Supports float note index and velocity.
        /// </summary>
        /// <param name="note">Note index (21.0-127.0)</param>
        /// <param name="velocity">Velocity (0.0-127.0)</param>
        public void PlayHiDefAudioSource(float note, float velocity, AudioSource source) {
            SampleData s = GetHiDefSample(note, velocity, out float v, out float p);
            source.pitch = p;
            source.volume = v;
            source.clip = s.clip;
            source.Play();
        }

        public void GetSampleDimensions(out int n, out int m, out int r) {
            n = samples.Length;
            m = n == 0 ? 0 : samples[0].Length;
            r = m == 0 ? 0 : samples[0][0].Length;
        }

        private void GetSampleDimensionsInternal(SampleData[][][] samples, out int n, out int m, out int r) {
            n = samples.Length;
            m = n == 0 ? 0 : samples[0].Length;
            r = m == 0 ? 0 : samples[0][0].Length;
        }

        public static bool IsValidNote(int note) {
            return note >= MIN_NOTE && note <= MAX_NOTE;
        }

        public static bool IsValidVelocity(int velocity) {
            return velocity >= 0 && velocity <= 127;
        }

        public void ApplyGeneration(VInstGenerationData data) {
            generationData = data;

            if (samples == null) samples = new SampleData[0][][];
            int noteSamples = data.useEvenNoteSpacings ? XM_Utilities.GetNoteSamplesRequired(data) : Mathf.Max(1, samples.Length);
            int velocitySamples = data.useEvenVelocitySpacings ? data.velocitySamples : Mathf.Max(1, samples.GetLength(1));
            int rrSamples = data.roundRobins;

            var oldSamples = samples;
            samples = new SampleData[noteSamples][][];
            for (int i = 0; i < noteSamples; i++) {
                samples[i] = new SampleData[velocitySamples][];
                for (int j = 0; j < velocitySamples; j++) {
                    samples[i][j] = new SampleData[rrSamples];
                }
            }

            //fry sample ids
            for(int i = MIN_NOTE; i <= MAX_NOTE; i++) {
                boundNote[i - MIN_NOTE] = XM_Utilities.GetEstimatedReferenceNoteIndex(i, data);
            }
            int current_vel = 0;
            for(int i = 0; i < 128; i++) {
                int v = data.GetVelocitySampleAt(current_vel);
                if (i > v) {
                    current_vel++;
                }
                boundVelocity[i] = current_vel;
            }

            //recover old samples
            if (samples.Length > 0 && oldSamples.Length > 0) {
                GetSampleDimensionsInternal(oldSamples, out int nn, out int mm, out int rr);
                GetSampleDimensionsInternal(samples, out int ni, out int mi, out int ri);
                int nz = Mathf.Min(nn, ni); int mz = Mathf.Min(mm, mi); int rz = Mathf.Min(rr, ri);
                for (int i = 0; i < nz; i++) {
                    for (int j = 0; j < mz; j++) {
                        for (int k = 0; k < rz; k++) {
                            samples[i][samples[0].Length - j - 1][k] = oldSamples[i][oldSamples[0].Length - j - 1][k];
                        }
                    }
                }
            }

            //fill in sample metadata
            GetSampleDimensionsInternal(samples, out int n, out int m, out int r);
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    for (int k = 0; k < r; k++) {
                        samples[i][j][k].sampleNote = data.GetNoteSampleAt(i);
                        samples[i][j][k].sampleVelocity = data.GetVelocitySampleAt(j);
                        samples[i][j][k].volumeMultiplier = data.volumeMultipliers[j];
                    }
                }
            }
        }

        #region Serialization
        [SerializeField] private SampleData[] serialized_samples = { };
        [SerializeField] private int samples_dim_0, samples_dim_1, samples_dim_2;

        public void OnBeforeSerialize() {
            GetSampleDimensions(out samples_dim_0, out samples_dim_1, out samples_dim_2);
            int length = samples_dim_0 * samples_dim_1 * samples_dim_2;

            if(serialized_samples.Length != length) serialized_samples = new SampleData[length];
            for (int i = 0; i < samples_dim_0; i++) {
                for (int j = 0; j < samples_dim_1; j++) {
                    for (int k = 0; k < samples_dim_2; k++) {
                        serialized_samples[i * samples_dim_1 * samples_dim_2 + j * samples_dim_2 + k] = samples[i][j][k];
                    }
                }
            }
        }

        public void OnAfterDeserialize() {
            samples = new SampleData[samples_dim_0][][];
            for (int i = 0; i < samples_dim_0; i++) {
                samples[i] = new SampleData[samples_dim_1][];
                for (int j = 0; j < samples_dim_1; j++) {
                    samples[i][j] = new SampleData[samples_dim_2];
                }
            }

            for (int i = 0; i < samples_dim_0; i++) {
                for (int j = 0; j < samples_dim_1; j++) {
                    for (int k = 0; k < samples_dim_2; k++) {
                        samples[i][j][k] = serialized_samples[i * samples_dim_1 * samples_dim_2 + j * samples_dim_2 + k];
                    }
                }
            }
        }
        #endregion
    }
}
