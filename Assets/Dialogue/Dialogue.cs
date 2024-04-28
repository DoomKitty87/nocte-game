using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Object")]
public class Dialogue : ScriptableObject
{
	public Character _character;
	[Tooltip("Color of the character's name in the subtitle box.")]
	[ColorUsage(false, false)] public Color _nameColor;
	[Tooltip("Self explanatory.")]
	[TextArea(3, 20)] public string _text;
	[Tooltip("Self explanatory.")]
	public AudioClip _audio;
}