using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private DialogueHandler _dialogueHandler;
    [SerializeField] private Dialogue _dialogueToPlay;
    [Header("Collider - Optional")]
    [SerializeField] private Collider _collider;
    [SerializeField] private bool _useTrigger;
    [Header("Settings")]
    [SerializeField] private bool _playOnce;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_dialogueHandler == null) {
            _dialogueHandler = DialogueHandler.Instance;
        }
        if (_useTrigger) {
            _collider.isTrigger = true;
        }
    }

    public void PlayDialogue() {
        _dialogueHandler.PlayDialogue(_dialogueToPlay);
    }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (_playOnce) {
                _collider.enabled = false;
            }
            _dialogueHandler.PlayDialogue(_dialogueToPlay);
        }
    }
}
