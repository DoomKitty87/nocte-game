using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI _dialogueUI;
    [SerializeField] private AudioSource _audioSource;
    [Header("Dialogue Queue")]
    [SerializeField] private List<Dialogue> _dialoguesQueue = new List<Dialogue>();
    private void AddDialogueToQueue(Dialogue dialogue) {
        if (_dialoguesQueue.Contains(dialogue)) return;
        _dialoguesQueue.Add(dialogue);
    }
    public Dialogue GetDialogueByIndex(int index) {
        if (index < 0 || index >= _dialoguesQueue.Count) return null;
        return _dialoguesQueue[index];
    }
    
    private bool _isPlayingDialogue = false;
    private IEnumerator PlayDialogueCoroutine(Dialogue dialogue) {
        _isPlayingDialogue = true;
        for (int i = 0; i < dialogue._textTurns.Count; i++) {
            _dialogueUI.SetDialogue(dialogue, i);
            _dialogueUI.ShowDialogueBox();
            yield return new WaitForSeconds(_dialogueUI.GetDialogueBoxShowTime());
            if (dialogue._textTurns[i]._audioClip != null) {
                _audioSource.PlayOneShot(dialogue._textTurns[i]._audioClip);
            }
            if (dialogue._textTurns[i]._durationIsAudioLength) {
                yield return new WaitForSeconds(_audioSource.clip.length);
            }
            else {
                yield return new WaitForSeconds(dialogue._textTurns[i]._duration);
            }
            _dialogueUI.HideDialogueBox();
            yield return new WaitForSeconds(_dialogueUI.GetDialogueBoxHideTime());
        }
        _isPlayingDialogue = false;
    }
    public void PlayDialogue(Dialogue dialogue, bool forcePlay = false) {
        if (forcePlay) {
            _dialoguesQueue.Clear();
            AddDialogueToQueue(dialogue);
        }
        else {
            AddDialogueToQueue(dialogue);
        }
    }

    private void Update() {
        if (_dialoguesQueue.Count > 0 && !_isPlayingDialogue) {
            StartCoroutine(PlayDialogueCoroutine(_dialoguesQueue[0]));
            _dialoguesQueue.RemoveAt(0);
        }
    }

}
