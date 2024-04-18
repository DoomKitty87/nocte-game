using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class FootstepType
{

    public string _materialID;
    public List<AudioClip> _footstepSounds;
    [Header("Settings")] 
    public float _groundNormalYValue;
    public NormalTest _normalTest;
    public enum NormalTest
    {
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
    }
    public LayerMask _materialLayer;
    public string _materialTag = "Untagged";

    public AudioClip GetRandomFootstep() {
        if (_footstepSounds.Count < 1) {
            Debug.LogError("FootstepAudio: No AudioClips assigned to FootstepType! Please assign at least one AudioClip.");
            return null;
        }
        return _footstepSounds[Random.Range(0, _footstepSounds.Count - 1)];
    }

    public bool OnMaterial(Vector3 raycastOrigin) {
        bool hitSomething = Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, _materialLayer);
        if (!hitSomething) return false;
        bool correctTag = hit.collider.gameObject.CompareTag(_materialTag);
        bool correctNormal;
        switch (_normalTest) {
            case NormalTest.GreaterThan:
                correctNormal = _groundNormalYValue < hit.normal.y;
                break;
            case NormalTest.LessThan:
                correctNormal = _groundNormalYValue > hit.normal.y;
                break;
            case NormalTest.GreaterThanOrEqual:
                correctNormal = _groundNormalYValue <= hit.normal.y;
                break;
            case NormalTest.LessThanOrEqual:
                correctNormal = _groundNormalYValue >= hit.normal.y;
                break;
            default:
                correctNormal = false;
                // if you get this then somehow you set NormalTest to a value that doesn't exist
                Debug.LogError("What");
                break;
        }
        if (correctNormal && correctTag) return true;
        return false;
    }

}

public class FootstepAudio : MonoBehaviour
{

    [Header("Dependencies")] 
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<FootstepType> _footstepTypes;
    [SerializeField] private List<Transform> _footTransforms;
    
    private void Start() {
        if (_audioSource == null || _footstepTypes.Count < 1) {
            Debug.LogError("FootstepAudio: Please assign references!");
        }
    }

    public void CallFootstep(int footID) {
        if (footID >= _footTransforms.Count || footID < 0) {
            Debug.LogError("FootstepAudio: CallFootstep invoked with invalid footID!");
        }
        if (_footTransforms.Count < 1) {
            Debug.LogError("FootstepAudio: FootTransforms array is empty! Assign positions in the array.");
        }
        foreach (FootstepType type in _footstepTypes) {
            if (type.OnMaterial(_footTransforms[footID].position)) {
                _audioSource.PlayOneShot(type.GetRandomFootstep());
                break;
            }
        }
    }
}
