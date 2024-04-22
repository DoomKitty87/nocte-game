using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
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
	private bool _grappleBoolLastFrame;
	private Vector3 _lastVelocity;
	private Vector3 _lastLastVelocity;

	private void Update() {
		_animator.SetBool("IdleState", _playerController.State == PlayerController.PlayerStates.Idle);
		_animator.SetBool("WalkingState", _playerController.State == PlayerController.PlayerStates.Walking);
		_animator.SetBool("SprintState", _playerController.State == PlayerController.PlayerStates.Sprinting);
		_animator.SetBool("CrouchState", _playerController.State == PlayerController.PlayerStates.Crouching);
		_animator.SetBool("SlideState", _playerController.State == PlayerController.PlayerStates.Sliding);
		_animator.SetBool("AirState", _playerController.State == PlayerController.PlayerStates.Air);
		_animator.SetBool("SwimState", _playerController.State == PlayerController.PlayerStates.Swimming);
		_animator.SetBool("GrappleState", _playerController.State == PlayerController.PlayerStates.Grappling);
		_animator.SetBool("DrivingState", _playerController.State == PlayerController.PlayerStates.Driving);
		_animator.SetBool("WalkingInput", _playerController._walking);
		_animator.SetBool("SprintInput", _playerController._sprinting);
		_animator.SetBool("CrouchInput", _playerController._crouching);
		_animator.SetBool("JumpDown", _playerController._keyJumping);
		if (!_grappleBoolLastFrame && _playerController._grappling) _animator.SetBool("GrappleDown", true);
		else {
			_animator.SetBool("GrappleDown", false);
		}
		_animator.SetBool("Grounded", _playerController._grounded);
		_animator.SetFloat("SpeedY", _playerController._velocity.y);
		_grappleBoolLastFrame = _playerController._grappling;
	}
}