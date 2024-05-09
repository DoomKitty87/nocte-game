using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShipHologramController : MonoBehaviour {

	[Header("Table")]
	[SerializeField] private Material _hologramEffectMaterial;
	[SerializeField] private GameObject table;

	private void OnEnable() {
		// Annoying process to apply the second material to the table
		Material materialToApply = new Material(_hologramEffectMaterial);
		Material[] materials = table.GetComponent<MeshRenderer>().materials;
		materials[1] = materialToApply;
		table.GetComponent<MeshRenderer>().materials = materials;
	}
}
