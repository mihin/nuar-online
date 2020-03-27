using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum CardState
{
    Alive,
    Dead
}

[Serializable]
public class Card
{
    [SerializeField] public byte id;
    [SerializeField] public string name;
    [SerializeField] public Sprite sprite;

    public class Factory : PlaceholderFactory<Card>
    {
    }
}

[CreateAssetMenu(fileName = "CardsData", menuName = "ScriptableObjects/CardsScriptableObject", order = 1)]
public class CardsScriptableObject : ScriptableObject
{
    public List<Card> Cards;
}
