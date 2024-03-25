using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimHandler : MonoBehaviour
{
	private InputHandler _inputHandler;
	
	[SerializeField] private Animator _animator;
	[SerializeField] private PlayerController _playerController;
	private void Start() {
		if (_playerController == null) {
			Debug.LogError("PlayerAnimHandler: PlayerController is not set.");
		}
		_inputHandler = InputHandler.Instance;
	}

	private bool _jumpBoolLastFrame;
	private void Update() {
		// this is bad, deal with new input system later
		if (_playerController.State == PlayerController.PlayerStates.Idle) {
			_animator.SetBool("Walking", false);
		}
		if (_playerController.State == PlayerController.PlayerStates.Walking || _playerController._walking) {
			_animator.SetBool("Walking", true);
		}
		if (_playerController.State == PlayerController.PlayerStates.Sprinting || _playerController._sprinting) {
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
		_animator.SetFloat("VerticalSpeed", _playerController._velocity.y);
	}
}