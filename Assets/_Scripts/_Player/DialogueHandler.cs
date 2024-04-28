using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    [Header("Dialogue Inventory")]
    [SerializeField] private List<Dialogue> _dialogues = new List<Dialogue>();
    
    private IEnumerator PlayDialogueCoroutine(Dialogue dialogue) {
        Debug.Log(dialogue._text);
        yield break;
    }
    public void PlayDialogue(Dialogue dialogue) {
        StartCoroutine(PlayDialogueCoroutine(dialogue));
    }
    
    public void AddDialogueToInventory(Dialogue dialogue) {
        if (_dialogues.Contains(dialogue)) return;
        _dialogues.Add(dialogue);
    }
    public Dialogue GetDialogueByIndex(int index) {
        if (index < 0 || index >= _dialogues.Count) return null;
        return _dialogues[index];
    }
}
