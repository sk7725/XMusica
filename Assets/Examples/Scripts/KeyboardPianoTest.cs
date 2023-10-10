using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMusica;

public class KeyboardPianoTest : MonoBehaviour {
    [System.Serializable]
    public struct PianoBinding {
        public string key;
        [MIDINote] public int note;
    }

    public BaseInstrumentSource instrument;
    public PianoBinding[] bindings = { };
    public AudioSource test;

    private void Update() {
        for (int i = 0; i < bindings.Length; i++) {
            if (Input.GetKeyDown(bindings[i].key)) {
                instrument.Press(bindings[i].note, 127);
                Debug.Log($"PRESS {bindings[i].note} 127");
            }
            else if (Input.GetKeyUp(bindings[i].key)) {
                instrument.Release(bindings[i].note, 0);
                Debug.Log($"RELEASE {bindings[i].note} 0");
            }
        }

        if (Input.GetKeyDown("v")) {
            test.Play();
        }
    }
}
