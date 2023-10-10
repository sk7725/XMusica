using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XMusica {
    public abstract class BaseInstrumentSource : MonoBehaviour {
        public enum SourceInstantiation {
            Awake,
            Start,
            Script
        }

        public VirtualInstrumentBinding binding;
        public float volume = 1f;

        [SerializeField] protected int _sourceCount = 10;
        [SerializeField] protected SourceInstantiation _instantiateOn = SourceInstantiation.Awake;

        public int SourceCount => _sourceCount;
        public SourceInstantiation InstantiateMode => _instantiateOn;
        public bool Instantiated => initialized;

        protected AudioSource[] sources;
        protected byte nextVacantSource = 0;
        private bool initialized = false;

        public void InstantiateSources() {
            if (initialized || _sourceCount < 1) return;
            sources = new AudioSource[_sourceCount];
            AudioSource template = transform.GetChild(0).GetComponent<AudioSource>();
            sources[0] = template;
            template.gameObject.name = "Source 0";


            for (int i = 1; i < _sourceCount; i++) {
                sources[i] = Instantiate<AudioSource>(template, transform);
                sources[i].gameObject.name = $"Source {i}";
            }

            AfterInitialize();
            initialized = true;
        }

        protected abstract void AfterInitialize();

        private void Awake() {
            if (_instantiateOn == SourceInstantiation.Awake) InstantiateSources();
        }

        private void Start() {
            if (_instantiateOn == SourceInstantiation.Start) InstantiateSources();
        }

        protected AudioSource PollSource() {
            AudioSource s = sources[nextVacantSource];
            nextVacantSource++;
            if (nextVacantSource == sources.Length) nextVacantSource = 0;
            return s;
        }

        protected abstract void Play(bool pressed, int note, int velocity);

#if UNITY_EDITOR
        protected virtual void Reset() {
            if(transform.childCount == 0 || !transform.GetChild(0).TryGetComponent<AudioSource>(out _)) {
                GameObject go = new GameObject("Source Template");
                go.transform.SetParent(transform, false);
                go.transform.SetAsFirstSibling();
                go.AddComponent<AudioSource>().playOnAwake = false;
            }
        }
#endif
    }
}