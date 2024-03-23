using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ParallaxLayer
{
	public RectTransform _transform;
	[HideInInspector] public Vector2 _startPos;
	[Header("Offset by X when mouseX is highest, same for Y")]
	public Vector2 _offset;
}

public class ParallaxEffect : MonoBehaviour
{
	[Header("Layers - Back To Front")]
	[SerializeField] private List<ParallaxLayer> _layers;
	// Start is called before the first frame update
	private void Start() {
		foreach (var layer in _layers) {
			layer._transform.anchoredPosition = layer._startPos;
		}
	}

	private Vector2 GetMouseUVPosistion() {
		// Make this use the new input system later
		Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		Vector2 mouseUV = mousePos / screenSize - Vector2.one * 0.5f;
		return mouseUV;
	}
	
	// Update is called once per frame
	private void Update() {
		foreach (var layer in _layers) {
			layer._transform.anchoredPosition = layer._startPos + layer._offset * GetMouseUVPosistion();
		}
	}
}
