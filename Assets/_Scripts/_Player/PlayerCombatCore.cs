using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Object = UnityEngine.Object;


[Serializable]
public class WeaponInventorySlot
{
	public WeaponItem _weaponItem;
	public GameObject _weaponInstance;
	public bool _equipped;

	public WeaponInventorySlot(WeaponItem weaponItem, GameObject weaponInstance) {
		_weaponItem = weaponItem;
		_weaponInstance = weaponInstance;
		_equipped = false;
	}
}

// Note: This depends on the animcontroller layer containing weapon animations to be called WeaponLayer 

[RequireComponent(typeof(HealthInterface))]
public class PlayerCombatCore : MonoBehaviour
{
	// Inventory ================================================================

	[SerializeField] private List<WeaponInventorySlot> _weaponInventory = new();
	[SerializeField] private int _equippedSlotIndex;
	
	private GameObject InstanceWeaponItem(WeaponItem weaponItem) {
		GameObject instance = Instantiate(weaponItem._weaponPrefab, _weaponContainer.transform);
		_currentWeaponInstance = instance;
		_currentInstanceScript = instance.GetComponent<WeaponScript>();
		if (_currentInstanceScript == null) {
			Debug.LogError($"{gameObject.name} CombatCore: Could not find WeaponScript or subclass on weaponInstance!");
			throw new Exception();
		}
		_currentInstanceScript._instancingPlayerCombatCoreScript = this;
		return instance;
	}
	
	private void InstantiateInventory() {
		if (_weaponInventory.Count == 0) return;
		foreach (WeaponInventorySlot slot in _weaponInventory) {
			GameObject instance = InstanceWeaponItem(slot._weaponItem);
			slot._weaponInstance = instance;
			instance.SetActive(false);
		}
	}
    
	public void AddWeapon(WeaponItem weaponItem) {
		GameObject instance = InstanceWeaponItem(weaponItem);
		instance.SetActive(false);
		WeaponInventorySlot newSlot = new(weaponItem, instance);
		_weaponInventory.Add(newSlot);
	}

	private bool _equipping;
	private IEnumerator EquipWeaponCoroutine(WeaponInventorySlot slot) {
		_equipping = true;
		if (_equippedSlotIndex != -1) {
			UnequipCurrentWeapon(true);
		}
		while (_unequipping) {
			yield return null;
		}
		_currentInstanceScript = slot._weaponInstance.GetComponent<WeaponScript>();
		_playerAnimator.SetLayerWeight(_playerAnimator.GetLayerIndex("WeaponLayer"), 1);
		slot._weaponInstance.SetActive(true);
		_leftHandGrabTarget.position = _currentInstanceScript._leftHandPosMarker.position;
		_leftHandGrabTarget.rotation = _currentInstanceScript._leftHandPosMarker.rotation;
		float waitTime = slot._weaponInstance.GetComponent<WeaponScript>().OnEquip();
		yield return new WaitForSeconds(waitTime);
		AssignInputEventsToCurrentInstanceScript();
		slot._equipped = true;
		_equippedSlotIndex = _weaponInventory.IndexOf(slot);
		_equipping = false;
	}
	private void EquipWeapon(WeaponInventorySlot slot) {
		if (_equipping) return;
		StartCoroutine(EquipWeaponCoroutine(slot));
	}
	
	public void EquipWeaponByWeaponItem(WeaponItem weaponItem) {
		for (int i = 0; i < _weaponInventory.Count; i++) {
			WeaponInventorySlot slot = _weaponInventory[i];
			if (slot._weaponItem == weaponItem) {
				EquipWeapon(slot);
				return;
			}
		}
	}

	public void EquipWeaponByIndex(int index) {
		if (index < 0 || index >= _weaponInventory.Count) return;
		EquipWeapon(_weaponInventory[index]);
	}

	public List<WeaponInventorySlot> GetWeaponInventory() {
		return _weaponInventory;
	}

	public int GetWeaponCount() {
		return _weaponInventory.Count;
	}

	public WeaponInventorySlot GetCurrentlyEquippedWeaponSlot() {
		if (_equippedSlotIndex < 0 || _equippedSlotIndex > _weaponInventory.Count) return null;
		return _weaponInventory[_equippedSlotIndex];
	}
	
	private bool _unequipping;
	private IEnumerator UnequipCurrentWeaponCoroutine(WeaponInventorySlot slot) {
		_unequipping = true;
		float waitTime = slot._weaponInstance.GetComponent<WeaponScript>().OnUnequip();
		yield return new WaitForSeconds(waitTime);
		UnassignInputEventsFromCurrentInstanceScript();
		_currentInstanceScript = null;
		slot._equipped = false;
		slot._weaponInstance.SetActive(false);
		_equippedSlotIndex = -1;
		_unequipping = false;
	}
	public void UnequipCurrentWeapon(bool disableImmediately = false) {
		if (_equipping || _unequipping) return;
		WeaponInventorySlot slot = _weaponInventory[_equippedSlotIndex];
		_playerAnimator.SetLayerWeight(_playerAnimator.GetLayerIndex("WeaponLayer"), 0);
		if (disableImmediately) {
			slot._equipped = false;
			slot._weaponInstance.SetActive(false);
			_equippedSlotIndex = -1;
			return;
		}
		StartCoroutine(UnequipCurrentWeaponCoroutine(slot));
	}

	public void RemoveWeaponByWeaponItem(WeaponItem weaponItem) {
		foreach (WeaponInventorySlot slot in _weaponInventory) {
			if (slot._weaponItem == weaponItem) {
				if (slot._equipped) {
					UnequipCurrentWeapon(true);
				}
				_weaponInventory.Remove(slot);
				return;
			}
		}
	}

	public void RemoveWeaponByIndex(int index) {
		if (index < 0 || index >= _weaponInventory.Count) return;
		if (_weaponInventory[index]._equipped) {
			UnequipCurrentWeapon(true);
		}
		_weaponInventory.RemoveAt(index);
	}

	public void RemoveAllWeapons() {
		if (_equippedSlotIndex != -1) {
			UnequipCurrentWeapon(true);
		}
		_weaponInventory.Clear();
	}

	// Ammo Helpers
	public bool WeaponUsesAmmo() {
		if (_equippedSlotIndex == -1) return false;
		if (GetAmmo() == (-1, -1)) return false;
		return true;
	}
	public (int, int) GetAmmo() {
		if (_equippedSlotIndex == -1) return (-1, -1);
		return _weaponInventory[_equippedSlotIndex]._weaponInstance.GetComponent<WeaponScript>().GetAmmo;
	}
	public void Weapon_RaiseAmmoChangedEvent() {
		(int, int) _ = GetAmmo();
		AmmoChanged?.Invoke(_.Item1, _.Item2);
	}
	
	public delegate void OnAmmoChanged(int currentAmmo, int maxAmmo);
	public event OnAmmoChanged AmmoChanged;
	
	// Controlling =============================================================
	
	private PlayerInput _input;

	[Header("Dependencies")]
	public Camera _mainCamera;
	public Animator _playerAnimator;
	public Rig _playerAnimationRiggingRig;
	public CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
	public CinemachineThirdPersonAim _cinemachineThirdPersonAim;
	public AudioSource _weaponFXAudioSource;
	public RecoilCamera _recoilCameraScript;
	public Transform _leftHandGrabTarget;
	[SerializeField] private GameObject _weaponContainer;

	[Header("Settings")]
	[Range(0, 1)] public float _AimParameter = 0;
	[SerializeField] private float _normalCameraDistance;
	[SerializeField] private float _aimedInCameraDistance = 2;
    
	[Header("Info - Dont change in editor")]
	[SerializeField] private WeaponItem _currentWeaponItem;
	[SerializeField] private GameObject _currentWeaponInstance;
	[SerializeField] private WeaponScript _currentInstanceScript;

	// See line 48
	// [SerializeField] private WeaponUI _weaponUI;

	private void ManageAimParameter() {
		_cinemachineThirdPersonFollow.CameraDistance = Mathf.Lerp(_normalCameraDistance, _aimedInCameraDistance, _AimParameter);
		_playerAnimationRiggingRig.weight = _AimParameter;
		_playerAnimator.SetFloat("Weapon_AimedIn", _AimParameter);
	}

	private void AssignInputEventsToCurrentInstanceScript() {
		_input.Player.Shoot.performed += _ => _currentInstanceScript.FireDown();
		_input.Player.Shoot.performed += _ => _fire1Down = true;
		_input.Player.Shoot.canceled += _ => _currentInstanceScript.FireUp();
		_input.Player.Shoot.canceled += _ => _fire1Down = false;

		_input.Player.ADS.performed += _ => _currentInstanceScript.Fire2Down();
		_input.Player.ADS.performed += _ => _fire2Down = true;
		_input.Player.ADS.canceled += _ => _currentInstanceScript.Fire2Up();
		_input.Player.ADS.canceled += _ => _fire2Down = false;

		_input.Player.Reload.performed += _ => _currentInstanceScript.ReloadDown();
		_input.Player.Reload.performed += _ => _reloadDown = true;
		_input.Player.Reload.canceled += _ => _currentInstanceScript.ReloadUp();
		_input.Player.Reload.canceled += _ => _reloadDown = false;
	}

	private void UnassignInputEventsFromCurrentInstanceScript() {
		_input.Player.Shoot.performed -= _ => _currentInstanceScript.FireDown();
		_input.Player.Shoot.performed -= _ => _fire1Down = true;
		_input.Player.Shoot.canceled -= _ => _currentInstanceScript.FireUp();
		_input.Player.Shoot.canceled -= _ => _fire1Down = false;

		_input.Player.ADS.performed -= _ => _currentInstanceScript.Fire2Down();
		_input.Player.ADS.performed -= _ => _fire2Down = true;
		_input.Player.ADS.canceled -= _ => _currentInstanceScript.Fire2Up();
		_input.Player.ADS.canceled -= _ => _fire2Down = false;

		_input.Player.Reload.performed -= _ => _currentInstanceScript.ReloadDown();
		_input.Player.Reload.performed -= _ => _reloadDown = true;
		_input.Player.Reload.canceled -= _ => _currentInstanceScript.ReloadUp();
		_input.Player.Reload.canceled -= _ => _reloadDown = false;
	}
	
	// Start is called before the first frame update
	private void Start() {
		_input = InputReader.Instance.PlayerInput;
		InstantiateInventory();

		
		_normalCameraDistance = _cinemachineThirdPersonFollow.CameraDistance;
	}

	private void OnDisable() {
		if (_currentInstanceScript != null) {
			UnassignInputEventsFromCurrentInstanceScript();
		}
	}

	private bool _fire1Down;
	private bool _fire2Down;
	private bool _reloadDown;

	private void Update() {
		ManageAimParameter();
		if (_currentInstanceScript != null) {
			_leftHandGrabTarget.position = _currentInstanceScript._leftHandPosMarker.position;
			_leftHandGrabTarget.rotation = _currentInstanceScript._leftHandPosMarker.rotation;
		}
		// Yeah its not great but it works :shrug:
		if (_fire1Down) _currentInstanceScript.FireHold();
		if (_fire2Down) _currentInstanceScript.Fire2Hold();
		if (_reloadDown) _currentInstanceScript.ReloadHold();
	}
}