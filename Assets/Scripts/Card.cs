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
    public byte id;
    public string name;
    public Sprite sprite;

    public class Factory : PlaceholderFactory<Card>
    {
    }
}

[CreateAssetMenu(fileName = "CardsData", menuName = "ScriptableObjects/CardsScriptableObject", order = 1)]
public class CardsScriptableObject : ScriptableObject
{
    public GameObject prefab;
    public List<Card> Cards;
}
