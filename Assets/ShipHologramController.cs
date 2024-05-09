using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShipHologramController : MonoBehaviour {
	[Header("Projector")]
	[SerializeField] private VisualEffect _VFX;
	[SerializeField] private float _rotationSpeed;
	[SerializeField] private Vector3 _rotation;

	[Header("Table")]
	[SerializeField] private Material _hologramEffectMaterial;
	[SerializeField] private GameObject table;
	private Material _mat;

	private void OnEnable() {
		// Annoying process to apply the second material to the table
		Material materialToApply = new Material(_hologramEffectMaterial);
		Material[] materials = table.GetComponent<MeshRenderer>().materials;
		materials[1] = materialToApply;
		table.GetComponent<MeshRenderer>().materials = materials;
	}

	private void Update() {
		_rotation += Vector3.up * (_rotationSpeed * Time.deltaTime);
		_VFX.SetVector3("Rotation", _rotation);
	}
}
