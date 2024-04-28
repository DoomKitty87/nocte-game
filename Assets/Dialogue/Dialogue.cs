using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Object")]
public class Dialogue : ScriptableObject
{
	[Tooltip("Shown as bold before the dialogue text.")]
	public string _characterName;
	[Tooltip("Color of the character's name in the subtitle box.")]
	[ColorUsage(false, false)] public Color _nameColor;
	[Tooltip("Self explanatory.")]
	[TextArea(3, 20)] public string _text;
	[Tooltip("Self explanatory.")]
	public AudioClip _audio;
}