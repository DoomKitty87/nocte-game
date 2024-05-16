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
        DialogueTurn dialogueTurn = dialogue._textTurns[turnIndex];
        _dialogueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(dialogueTurn._character._nameColor)}>{dialogueTurn._character.CharacterName}:</color> {dialogueTurn._text}";
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
