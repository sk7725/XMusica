using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMusica;

public class ExampleNoteBehavior : MonoBehaviour
{
    [MIDINote]
    public int note, note2;

    public int controlNote = 21;

    private void OnValidate() {
        note = controlNote;
    }
}
