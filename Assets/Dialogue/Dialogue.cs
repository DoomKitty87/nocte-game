using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueTurn
{
	public string _text;
	public float _duration;
	public AudioClip _audioClip;
	public bool _durationIsAudioLength;
}

[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Object")]
public class Dialogue : ScriptableObject
{
	
	
	public Character _character;
	[Tooltip("Color of the character's name in the subtitle box.")]
	[ColorUsage(false, false)] public Color _nameColor;
	[Tooltip("List of text turns in the dialogue. Each turn will be displayed in the subtitle box.")]
	[TextArea(3, 20)] public List<DialogueTurn> _textTurns;
}