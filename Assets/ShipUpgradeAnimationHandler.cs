using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class ShipUpgradeAnimationHandler : MonoBehaviour {
	[SerializeField] private GameObject _upgradeScreenObject;
	[SerializeField] private GameObject _upgradeScreenCamera;
	[SerializeField] private GameObject _homeScreenObject;

	[SerializeField] private ShipUpgradeTableAnimationHandler _tableAnimationHandler;

	[SerializeField] private VisualEffect _holoTableVFX;

	[SerializeField] private HolographMeshSettings[] _holographMeshes;

	[System.Serializable]
	private struct HolographMeshSettings {
		public Mesh mesh;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
		public float rotationSpeed;
	}
	private HolographMeshSettings _currentMeshSettings;

	[Header("Table")]
	[SerializeField] private Material _hologramEffectMaterial;
	[SerializeField] private GameObject table;

	private Vector3 _rotation;

	private bool _initialized;

	private void OnEnable() {
		// Annoying process to apply the second material to the table
		Material materialToApply = new Material(_hologramEffectMaterial);
		Material[] materials = table.GetComponent<MeshRenderer>().materials;
		materials[1] = materialToApply;
		table.GetComponent<MeshRenderer>().materials = materials;
		ToggleUpgradeScreen(false);
	}

	public void ToggleUpgradeScreen(bool enable) {
		_homeScreenObject.SetActive(!enable);
		_upgradeScreenObject.SetActive(enable);
		_upgradeScreenCamera.SetActive(enable);

		_holoTableVFX.SetBool("ZoomedIn", enable);

		if (!enable) {
			_currentMeshSettings = _holographMeshes[Random.Range(0, _holographMeshes.Length)];
			_holoTableVFX.SetMesh("Object Mesh", _currentMeshSettings.mesh);
			_holoTableVFX.SetVector3("Position", _currentMeshSettings.position);
			_rotation = _currentMeshSettings.rotation;
			_holoTableVFX.SetVector3("Scale", _currentMeshSettings.scale);

			if (_initialized) _tableAnimationHandler.ResetPosition(); // Have to do this
			_initialized = true;
		}
	}

	private void Update() {
		_rotation += Vector3.up * (_currentMeshSettings.rotationSpeed * Time.deltaTime);
		_rotation.x %= 360;
		_rotation.y %= 360;
		_rotation.z %= 360;

		_holoTableVFX.SetVector3("Rotation", _rotation);
	}
}
