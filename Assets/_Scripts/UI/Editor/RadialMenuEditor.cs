using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadialMenu))]
[CanEditMultipleObjects]
public class RadialMenuEditor : Editor
{
	private RadialMenu _radialMenu;
	private SerializedProperty _menuCenter;
	private SerializedProperty _separatorGameObject;
	private SerializedProperty _selectorTransform;
	private SerializedProperty _selectionCount;
	private SerializedProperty _separatorOffset;

	private void OnEnable() {
		_radialMenu = (RadialMenu)target;
		_menuCenter = serializedObject.FindProperty("_menuCenter");
		_separatorGameObject = serializedObject.FindProperty("_separatorGameObject");
		_selectorTransform = serializedObject.FindProperty("_selectorTransform");
		_selectionCount = serializedObject.FindProperty("_selectionCount");
		_separatorOffset = serializedObject.FindProperty("_separatorOffset");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUILayout.PropertyField(_menuCenter);
		EditorGUILayout.PropertyField(_separatorGameObject);
		EditorGUILayout.PropertyField(_selectorTransform);
		EditorGUILayout.PropertyField(_selectionCount);
		EditorGUILayout.PropertyField(_separatorOffset);
		serializedObject.ApplyModifiedProperties();
		if (GUILayout.Button("Generate Separators")) _radialMenu.GenerateSeparators();
	}
}