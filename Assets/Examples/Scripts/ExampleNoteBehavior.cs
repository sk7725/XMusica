using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMusica;

public class ExampleNoteBehavior : MonoBehaviour
{
    [MIDINote]
    public int note, note2;

    public string parse_note, parse_note2;

    private void Start() {
        Debug.Log($"Note: {XM_Utilities.GetNoteString(note)} ({note})");
        Debug.Log($"Note2: {XM_Utilities.GetNoteString(note2)} ({note2})");
        Debug.Log($"Parse Note: {XM_Utilities.FromNoteString(parse_note)}");
        Debug.Log($"Parse Note2: {XM_Utilities.FromNoteString(parse_note2)}");
        Debug.Assert(XM_Utilities.FromNoteString(XM_Utilities.GetNoteString(note)) == note);
        Debug.Assert(XM_Utilities.GetNoteString(XM_Utilities.FromNoteString(parse_note)) == parse_note);
    }
}
