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

    public bool alive;
    //public bool Alive
    //{
    //    get { return alive; }
    //    set { alive = value; image.color = value?Color.white:Color.red; }
    //}

    void Start()
    {
        alive = true;
    }

    void Update()
    {
        image.color = alive ? Color.white : Color.red;
    }

    public void PointerEnter()
    {
        if (alive)
            transform.localScale += new Vector3(0.2f, 0.2f, 0);
    }

    public void PointerExit()
    {
        if (alive)
            transform.localScale -= new Vector3(0.2f, 0.2f, 0);
    }

    public class Factory : PlaceholderFactory<CardPrefab>
    {
    }
}
