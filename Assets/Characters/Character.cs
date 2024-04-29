using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Character", menuName = "Characters/New Character")]
public class Character : ScriptableObject
{
    [SerializeField] private string _characterName;
    [SerializeField] private string age, height, gender, occupation, status;
    [SerializeField] private Sprite _characterPortrait;
    
    public string CharacterName => _characterName;
    public string Age => age;
    public string Height => height;
    public string Gender => gender;
    public string Occupation => occupation;
    public string Status => status;
    public Sprite CharacterPortrait => _characterPortrait;
}
