/*
using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class DamageFeedback : MonoBehaviour
{
	// TODO: Complete this list
	
	// What feedback do we want?
	// Camera shake - Cinemachine Impulse (DONE)
	// Directional Camera Translation? (DONE)
	// Camera Static (DONE)
	// Full Screen Effects - Blurring, Static, (Vingette?), Tinting (DONE)
	// -- Later --
	// Screen Edge Effects - Edge Blurring, Edge "Corruption", Edge Static
	// Sound
 	// Particles

	[Header("References")]
	[SerializeField] private CinemachineImpulseSource _impulseSource;
	[SerializeField] private Volume _effectsVolume;
	[Header("Settings")]
	[SerializeField] private float _impulseForceMultiplier = 0.25f;
	[SerializeField] private AnimationCurve _volumeWeightCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
	[SerializeField] private float _effectsDurationSeconds = 0.5f;
	
	private void OnValidate() {
		_impulseSource = gameObject.GetComponent<CinemachineImpulseSource>();
	}

	public void ActivateFeedback(Vector3 damagePosition) {
		_impulseSource.GenerateImpulseAtPositionWithVelocity(damagePosition, (transform.position - damagePosition).normalized * _impulseForceMultiplier);
		StopCoroutine(ActivateFeedbackCoroutine(_effectsDurationSeconds));
		StartCoroutine(ActivateFeedbackCoroutine(_effectsDurationSeconds));
	}

	private IEnumerator ActivateFeedbackCoroutine(float duration) {
		float t = 0;
		while (t < duration) {
			_effectsVolume.weight = _volumeWeightCurve.Evaluate(t/duration);
			t += Time.deltaTime;
			yield return null;
		}
	}
}
*/