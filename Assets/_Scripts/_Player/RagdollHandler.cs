using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] private bool _isRagdoll = false;
    [Header("Animated Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private List<Collider> _animatedColliders = new List<Collider>();
    [Header("Ragdoll Parts")]
    [SerializeField] private Transform _pelvis;
    [SerializeField] private Transform _leftHips;
    [SerializeField] private Transform _leftKnee;
    // [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightHips;
    [SerializeField] private Transform _rightKnee;
    // [SerializeField] private Transform _rightFoot;
    [SerializeField] private Transform _leftArm;
    [SerializeField] private Transform _leftElbow;
    [SerializeField] private Transform _rightArm;
    [SerializeField] private Transform _rightElbow;
    [SerializeField] private Transform _middleSpine;
    [SerializeField] private Transform _head;
    
    [SerializeField] private List<Rigidbody> _ragdollRigidbodies = new List<Rigidbody>();
    [SerializeField] private List<Collider> _ragdollColliders = new List<Collider>();
    
    private void Start()
    {
        _ragdollRigidbodies.Add(_pelvis.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_leftHips.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_leftKnee.GetComponent<Rigidbody>());
        // _ragdollRigidbodies.Add(_leftFoot.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_rightHips.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_rightKnee.GetComponent<Rigidbody>());
        // _ragdollRigidbodies.Add(_rightFoot.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_leftArm.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_leftElbow.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_rightArm.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_rightElbow.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_middleSpine.GetComponent<Rigidbody>());
        _ragdollRigidbodies.Add(_head.GetComponent<Rigidbody>());
        _ragdollColliders.Add(_pelvis.GetComponent<Collider>());
        _ragdollColliders.Add(_leftHips.GetComponent<Collider>());
        _ragdollColliders.Add(_leftKnee.GetComponent<Collider>());
        // _ragdollColliders.Add(_leftFoot.GetComponent<Collider>());
        _ragdollColliders.Add(_rightHips.GetComponent<Collider>());
        _ragdollColliders.Add(_rightKnee.GetComponent<Collider>());
        // _ragdollColliders.Add(_rightFoot.GetComponent<Collider>());
        _ragdollColliders.Add(_leftArm.GetComponent<Collider>());
        _ragdollColliders.Add(_leftElbow.GetComponent<Collider>());
        _ragdollColliders.Add(_rightArm.GetComponent<Collider>());
        _ragdollColliders.Add(_rightElbow.GetComponent<Collider>());
        _ragdollColliders.Add(_middleSpine.GetComponent<Collider>());
        _ragdollColliders.Add(_head.GetComponent<Collider>());
        SetRagdoll(_isRagdoll);
    }

    public void SetRagdoll(bool value = false) {
        if (value) {
            _animator.enabled = false;
            foreach (Collider collider in _animatedColliders) {
                collider.enabled = false;
            }

            foreach (Rigidbody rigidbody in _ragdollRigidbodies) {
                rigidbody.isKinematic = false;
            }

            foreach (Collider collider in _ragdollColliders) {
                collider.enabled = true;
            }
        }
        else {
            _animator.enabled = true;
            foreach (Collider collider in _animatedColliders) {
                collider.enabled = true;
            }

            foreach (Rigidbody rigidbody in _ragdollRigidbodies) {
                rigidbody.isKinematic = true;
            }

            foreach (Collider collider in _ragdollColliders) {
                collider.enabled = false;
            }
        }
    }
}
