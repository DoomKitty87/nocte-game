using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Cinemachine;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
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
	[SerializeField] private int _equippedSlotIndex = -1;
	[SerializeField] private int _maxWeapons = 6;
	
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
    
	public bool AddWeapon(WeaponItem weaponItem) {
		if (_weaponInventory.Count >= _maxWeapons) return false;
		// Debug.Log($"AddWeapon: {weaponItem._weaponName}");
		GameObject instance = InstanceWeaponItem(weaponItem);
		instance.SetActive(false);
		WeaponInventorySlot newSlot = new(weaponItem, instance);
		_weaponInventory.Add(newSlot);
		return true;
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
		_currentWeaponItem = slot._weaponItem;
		_currentInstanceScript = slot._weaponInstance.GetComponent<WeaponScript>();
		_playerAnimator.SetLayerWeight(_playerAnimator.GetLayerIndex("WeaponLayer"), 1);
		slot._weaponInstance.SetActive(true);
		_equippedSlotIndex = _weaponInventory.IndexOf(slot);
		_OnInventoryChanged?.Invoke();
		float waitTime = slot._weaponInstance.GetComponent<WeaponScript>().OnEquip();
		yield return new WaitForSeconds(waitTime);
		slot._equipped = true;
		_equipping = false;
	}

	private bool EquipWeapon(WeaponInventorySlot slot) {
		if (_equipping || _unequipping) return false;
		// Debug.Log($"EquippingWeapon: {slot._weaponItem.name}");
		StartCoroutine(EquipWeaponCoroutine(slot));
		return true;
	}
	
	public bool EquipWeaponByWeaponItem(WeaponItem weaponItem) {
		for (int i = 0; i < _weaponInventory.Count; i++) {
			WeaponInventorySlot slot = _weaponInventory[i];
			if (slot._weaponItem == weaponItem) {
				return EquipWeapon(slot);
			}
		}
		return false;
	}

	public bool EquipWeaponByIndex(int index) {
		if (index < 0 || index >= _weaponInventory.Count) return false;
		return EquipWeapon(_weaponInventory[index]);
	}

	public List<WeaponInventorySlot> GetWeaponInventory() {
		return _weaponInventory;
	}

	public int GetWeaponCount() {
		return _weaponInventory.Count;
	}

	public int GetMaxWeaponCount() {
		return _maxWeapons;
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
		_currentWeaponItem = null;
		_currentInstanceScript = null;
		_equippedSlotIndex = -1;
		slot._equipped = false;
		slot._weaponInstance.SetActive(false);
		_OnInventoryChanged?.Invoke();
		_unequipping = false;
	}
	public void UnequipCurrentWeapon(bool disableImmediately = false) {
		if (_unequipping) return;
		WeaponInventorySlot slot = _weaponInventory[_equippedSlotIndex];
		_playerAnimator.SetLayerWeight(_playerAnimator.GetLayerIndex("WeaponLayer"), 0);
		if (disableImmediately) {
			_currentWeaponItem = null;
			_currentInstanceScript = null;
			_equippedSlotIndex = -1;
			slot._equipped = false;
			slot._weaponInstance.SetActive(false);
			return;
		}
		if (_equipping) return;
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

	public void RemoveWeaponBySlot(WeaponInventorySlot slot) {
		if (slot._equipped) {
			UnequipCurrentWeapon(true);
		}
		_weaponInventory.Remove(slot);
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
		(bool, int, int) output = _weaponInventory[_equippedSlotIndex]._weaponInstance.GetComponent<WeaponScript>().GetUsesAmmoCurrentAmmoAndMaxAmmo();
		if (!output.Item1) return (-1, -1);
		else {
			return (output.Item2, output.Item3);
		}
	}
	public void Weapon_RaiseAmmoChangedEvent() {
		(int, int) _ = GetAmmo();
		AmmoChanged?.Invoke(_.Item1, _.Item2);
	}
	
	public delegate void OnAmmoChanged(int currentAmmo, int maxAmmo);
	public event OnAmmoChanged AmmoChanged;
	
	public UnityEvent _OnInventoryChanged;
	
	// Controlling =============================================================
	
	private PlayerInput _input;

	[Header("Raycasting")]
	public Camera _mainCamera;
	[Header("Animation")]
	public Animator _playerAnimator;
	[SerializeField] private GameObject _weaponContainer;
	[Header("IK")]
	public Rig _playerAnimationRiggingRig;
	public Transform _leftHandGrabTarget;
	[Header("Aiming")]
	public CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
	public CinemachineThirdPersonAim _cinemachineThirdPersonAim;
	[SerializeField] private float _normalCameraDistance;
	[SerializeField] private float _aimedInCameraDistance = 2;
	[SerializeField] private float _normalCameraSide = 0.5f;
	[SerializeField] private float _aimedInCameraSide = 0.65f;
	[SerializeField] private float _aimSpeed;
	[Range(0, 1)] public float _AimParameter = 0;
	private bool _aimIn;
	public bool _useAimedAnimations = true;
	[Header("Audio")]
	public AudioSource _weaponFXAudioSource;
	[Header("Recoil")]
	public RecoilCamera _recoilCameraScript;
	[Header("Info - Dont change in editor")]
	[SerializeField] private WeaponItem _currentWeaponItem;
	[SerializeField] private GameObject _currentWeaponInstance;
	[SerializeField] private WeaponScript _currentInstanceScript;

	// See line 48
	// [SerializeField] private WeaponUI _weaponUI;

	private void LerpAimCameraAndAnim() {
		_AimParameter = Mathf.Lerp(_AimParameter, _aimIn ? 1 : 0, Time.deltaTime * _aimSpeed);
		_cinemachineThirdPersonFollow.CameraDistance = Mathf.Lerp(_normalCameraDistance, _aimedInCameraDistance, _AimParameter);
		_cinemachineThirdPersonFollow.CameraSide = Mathf.Lerp(_normalCameraSide, _aimedInCameraSide, _AimParameter);
		if (_currentWeaponItem != null && _useAimedAnimations) {
			_playerAnimationRiggingRig.weight = _AimParameter;
			_playerAnimator.SetFloat("Weapon_AimedIn", _AimParameter);
		}
		else {
			_playerAnimationRiggingRig.weight = 0;
			_playerAnimator.SetFloat("Weapon_AimedIn", 0);
		}
	}
	
	private void OnADSDown() {
		_aimIn = true;
	}

	private void OnADSUp() {
		_aimIn = false;
	}
	
	// Start is called before the first frame update
	private void Start() {
		_input = InputReader.Instance.PlayerInput;
		InstantiateInventory();
		_normalCameraDistance = _cinemachineThirdPersonFollow.CameraDistance;
		
		_input.Player.ADS.performed += _ => OnADSDown();
		_input.Player.ADS.canceled += _ => OnADSUp();
	}

	private void OnDisable() {
		_input.Player.ADS.performed -= _ => OnADSDown();
		_input.Player.ADS.canceled -= _ => OnADSUp();
	}

	private void Update() {
		LerpAimCameraAndAnim();
		if (_currentInstanceScript != null) {
			(Transform, Transform) output = _currentInstanceScript.GetLeftHandIKTargets();
			_leftHandGrabTarget.position = output.Item1.position;
			_leftHandGrabTarget.rotation = output.Item1.rotation;
		}
	}
}