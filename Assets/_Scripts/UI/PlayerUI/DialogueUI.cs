using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private Animator _dialogueBoxAnimator;
    [SerializeField] private AnimationClip _dialogueBoxShowClip;
    [SerializeField] private AnimationClip _dialogueBoxHideClip;
    [SerializeField] private TMPro.TextMeshProUGUI _dialogueText;

    public void SetDialogue(Dialogue dialogue, int turnIndex) {
        _dialogueText.text = $"<color={ColorUtility.ToHtmlStringRGB(dialogue._nameColor)}>{dialogue._character.name}:</color> {dialogue._textTurns[turnIndex]._text}";
    }
    
    public void ShowDialogueBox() {
        _dialogueBoxAnimator.SetBool("Show", true);
    }
    
    public void HideDialogueBox() {
        _dialogueBoxAnimator.SetBool("Show", false);
    }
    
    public float GetDialogueBoxShowTime() {
        return _dialogueBoxShowClip.length;
    } 
    
    public float GetDialogueBoxHideTime() {
        return _dialogueBoxHideClip.length;
    }

}
