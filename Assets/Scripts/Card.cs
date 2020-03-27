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
    private const string data = "{\"id\":0,\"name\":\"Веселёха\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":1,\"name\":\"Цыпа\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":2,\"name\":\"Андрэ\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":3,\"name\":\"Никитос\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":4,\"name\":\"Аль Йоша\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":5,\"name\":\"Жак Ив Ив\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":6,\"name\":\"Брыксон\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":7,\"name\":\"Дон Чепи\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":8,\"name\":\"Рожфор\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":9,\"name\":\"Богданбарт\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":10,\"name\":\"Стас\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":11,\"name\":\"Уикинг\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":12,\"name\":\"Серега 2.0\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":13,\"name\":\"Дикай\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":14,\"name\":\"Миша-мэн\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":15,\"name\":\"Анатоль\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":16,\"name\":\"Дон Хулио\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":17,\"name\":\"Сэр Огаст\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":18,\"name\":\"Каталина\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":19,\"name\":\"Макс\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":20,\"name\":\"Малекс\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":21,\"name\":\"Дэнни\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":22,\"name\":\"Алдуш\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":23,\"name\":\"Плохiш\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":24,\"name\":\"Даша\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":25,\"name\":\"Фил\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":26,\"name\":\"Сфинкстер\",\"sprite\":{\"instanceID\":0}}|Card\n{\"id\":27,\"name\":\"Алехандро\",\"sprite\":{\"instanceID\":0}}|Card\n";
    public List<Card> Cards;

    public void LoadCachedData()
    {
        List<Card> cd = data.FromJson<Card>();
        if (cd != null) Cards = cd;
    }
}
