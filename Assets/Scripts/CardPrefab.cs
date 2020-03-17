using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CardPrefab : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Text text;

    private Card card;
    public Card Card
    {
        get { return card; }
        set { card = value; text.text = card.name; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class Factory : PlaceholderFactory<CardPrefab>
    {
    }
}
