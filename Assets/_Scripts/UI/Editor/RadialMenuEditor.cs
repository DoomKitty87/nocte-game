using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadialMenu))]
[CanEditMultipleObjects]
public class RadialMenuEditor : Editor
{
	private RadialMenu _radialMenu;

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		_radialMenu = (RadialMenu)target;
		if (GUILayout.Button("Generate Separators")) {
			_radialMenu.GenerateSeparators();
		}
	}
}