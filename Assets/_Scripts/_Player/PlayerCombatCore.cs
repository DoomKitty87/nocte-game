using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Cinemachine;
using UnityEngine;
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

[RequireComponent(typeof(HealthInterface))]
public class PlayerCombatCore : MonoBehaviour
{
	// Inventory ================================================================

	[SerializeField] private List<WeaponInventorySlot> _weaponInventory = new();
	[SerializeField] private int _equippedSlotIndex;
	
	private GameObject InstanceWeaponItem(WeaponItem weaponItem) {
		if (_weaponContainer == null) {
			Debug.LogError("The variable _weaponContainer of PlayerCombatCore has not been assigned!");
			throw new Exception();
		}

		GameObject instance = Instantiate(weaponItem._weaponPrefab, _weaponContainer.transform);
		instance.transform.position = weaponItem._weaponContainerOffset;
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
			instance.SetActive(false);
		}
	}
    
	public void AddWeapon(WeaponItem weaponItem) {
		GameObject instance = InstanceWeaponItem(weaponItem);
		instance.SetActive(false);
		WeaponInventorySlot newSlot = new(weaponItem, instance);
		_weaponInventory.Add(newSlot);
	}

	private void EquipWeapon(WeaponInventorySlot slot) {
		if (_equippedSlotIndex != -1) {
			UnequipCurrentWeapon(true);
		}
		_equippedSlotIndex = _weaponInventory.IndexOf(slot);
		slot._equipped = true;
		slot._weaponInstance.SetActive(true);
	}
	
	public void EquipWeaponByWeaponItem(WeaponItem weaponItem) {
		for (int i = 0; i < _weaponInventory.Count; i++) {
			WeaponInventorySlot slot = _weaponInventory[i];
			if (slot._weaponItem == weaponItem) {
				_equippedSlotIndex = i;
				slot._equipped = true;
				slot._weaponInstance.SetActive(true);
				return;
			}
		}
	}

	public void EquipWeaponByIndex(int index) {
		if (index < 0 || index >= _weaponInventory.Count) return;
		_equippedSlotIndex = index;
		_weaponInventory[index]._equipped = true;
		_weaponInventory[index]._weaponInstance.SetActive(true);
		
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
	
	private IEnumerator UnequipCurrentWeaponCoroutine(WeaponInventorySlot slot) {
		float waitTime = slot._weaponInstance.GetComponent<WeaponScript>().OnUnequip();
		yield return new WaitForSeconds(waitTime);
		slot._equipped = false;
		slot._weaponInstance.SetActive(false);
		_equippedSlotIndex = -1;
	}
	public void UnequipCurrentWeapon(bool disableImmediately = false) {
		WeaponInventorySlot slot = _weaponInventory[_equippedSlotIndex];
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

	// Controlling =============================================================
	
	private PlayerInput _input;

	[Header("Dependencies")]
	public Camera _mainCamera;

	public Animator _playerAnimator;
	public CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
	public CinemachineThirdPersonAim _cinemachineThirdPersonAim;
	public AudioSource _weaponFXAudioSource;
	[SerializeField] private GameObject _weaponContainer;

	[Header("Info - Dont change in editor")]
	[SerializeField] private WeaponItem _currentWeaponItem;
	[SerializeField] private GameObject _currentWeaponInstance;
	[SerializeField] private WeaponScript _currentInstanceScript;

	// See line 48
	// [SerializeField] private WeaponUI _weaponUI;

	// Start is called before the first frame update
	private void Start() {
		_input = InputReader.Instance.PlayerInput;
		InstantiateInventory();

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

	private void OnDisable() {
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

	private bool _fire1Down;
	private bool _fire2Down;
	private bool _reloadDown;

	private void Update() {
		// Yeah its not great but it works :shrug:
		if (_fire1Down) _currentInstanceScript.FireHold();
		if (_fire2Down) _currentInstanceScript.Fire2Hold();
		if (_reloadDown) _currentInstanceScript.ReloadHold();
	}
}