using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimHandler : MonoBehaviour
{
	private PlayerInput _input;
	
	[SerializeField] private Animator _animator;
	[SerializeField] private PlayerController _playerController;
	private void Start() {
		_input = InputReader.Instance.PlayerInput;
		
		if (_playerController == null) {
			Debug.LogError("PlayerAnimHandler: PlayerController is not set.");
		}
	}

	private bool _jumpBoolLastFrame;
	private Vector3 _lastVelocity;
	private Vector3 _lastLastVelocity;
	private void Update() {
		// this is bad, deal with new input system later
		if (_playerController.State == PlayerController.PlayerStates.Idle) {
			_animator.SetBool("Walking", false);
		}
		if (_playerController.State == PlayerController.PlayerStates.Walking || _playerController._walking) {
			_animator.SetBool("Walking", true);
		}
		if (_playerController.State == PlayerController.PlayerStates.Sprinting) {
			_animator.SetBool("Running", true);
		}
		else {
			_animator.SetBool("Running", false);
		}
		if (_playerController.State == PlayerController.PlayerStates.Crouching || _playerController._crouching) {
			_animator.SetBool("Crouching", true);
		}
		else {
			_animator.SetBool("Crouching", false);
		}
		_animator.SetFloat("VerticalSpeed", _lastLastVelocity.y);
		if (_playerController.State == PlayerController.PlayerStates.Air) {
			_animator.SetBool("Air", true);
		}
		else {
			_animator.SetBool("Air", false);
		}
		if (_playerController._jumping && !_jumpBoolLastFrame) {
			_animator.SetBool("Jump", true);
			_jumpBoolLastFrame = _playerController._jumping;
		}
		else {
			_animator.SetBool("Jump", false);
			_jumpBoolLastFrame = _playerController._jumping;
		}
		if (_playerController.State == PlayerController.PlayerStates.Grappling) {
			_animator.SetBool("Grappling", true);
		}
		else {
			// _animator.SetBool("Grappling", false);
		}
		_lastLastVelocity = _lastVelocity;
		_lastVelocity = _playerController._velocity;
	}
}