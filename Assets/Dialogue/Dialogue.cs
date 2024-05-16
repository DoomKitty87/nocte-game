using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueTurn
{
	public Character _character;
	[TextArea(3, 20)] public string _text;
	public float _duration;
	public AudioClip _audioClip;
	public bool _durationIsAudioLength;
}

[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Object")]
public class Dialogue : ScriptableObject
{
	[Tooltip("List of text turns in the dialogue. Each turn will be displayed in the subtitle box.")]
	public List<DialogueTurn> _textTurns;
}