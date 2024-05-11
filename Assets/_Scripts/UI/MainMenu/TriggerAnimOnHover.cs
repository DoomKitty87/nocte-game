using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TriggerAnimOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerOnHover;
    [SerializeField] private string _triggerOnExit;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _animator.SetTrigger(_triggerOnHover);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _animator.SetTrigger(_triggerOnExit);
    }
    
    
}
