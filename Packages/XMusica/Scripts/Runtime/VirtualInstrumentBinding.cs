using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica {
    public sealed class VirtualInstrumentBinding : ScriptableObject {
        private const int MIN_NOTE = 21, MAX_NOTE = 127;
        /// <summary>
        /// Converts requested note -> sample id(i)
        /// </summary>
        [SerializeField] private int[] boundNote = new int[107];
        /// <summary>
        /// Converts requested velocity -> sample id(j)
        /// </summary>
        [SerializeField] private int[] boundVelocity = new int[127];
        [SerializeField] private SampleData[][] samples = { };


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
            return samples[boundNote[note - MIN_NOTE]][boundVelocity[velocity]];
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

        public static bool IsValidNote(int note) {
            return note >= MIN_NOTE && note <= MAX_NOTE;
        }

        public static bool IsValidVelocity(int velocity) {
            return velocity >= 0 && velocity <= 127;
        }
    }
}
