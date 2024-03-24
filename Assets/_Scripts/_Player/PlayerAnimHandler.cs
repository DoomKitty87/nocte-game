using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimHandler : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private PlayerController _playerController;
	private void Start() {
		if (_playerController == null) {
			Debug.LogError("PlayerAnimHandler: PlayerController is not set.");
		}
	}
	
	private void Update() {
		if (_playerController.State == PlayerController.PlayerStates.Idle) {
			_animator.SetBool("Walking", false);
		}
		if (_playerController.State == PlayerController.PlayerStates.Walking) {
			_animator.SetBool("Walking", true);
		}

		if (_playerController.State == PlayerController.PlayerStates.Crouching) {
			_animator.SetBool("Crouching", true);
		}
		else {
			_animator.SetBool("Crouching", false);
		}
	}
}