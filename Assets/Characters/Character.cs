using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Character", menuName = "Characters/New Character")]
public class Character : ScriptableObject
{
    [SerializeField] private string _characterName;
    [SerializeField] private string _Nickname;
    [SerializeField] private string age, height, gender, occupation, status;
    [SerializeField] private Sprite _characterPortrait;
    [SerializeField] private bool _isHidden; 
    
    
    public string CharacterName => _characterName;
    public string Nickname => _Nickname;
    public string Age => age;
    public string Height => height;
    public string Gender => gender;
    public string Occupation => occupation;
    public string Status => status;
    public Sprite CharacterPortrait => _characterPortrait;
    public bool IsHidden => _isHidden;
}
