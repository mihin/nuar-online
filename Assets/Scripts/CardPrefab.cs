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

    public bool alive = true;
    //public bool Alive
    //{
    //    get { return alive; }
    //    set { alive = value; image.color = value?Color.white:Color.red; }
    //}

    public bool isMy = false;
    public bool isActive = false;

    void Start()
    {
    }

    void Update()
    {
    }

    public void RefreshGraphics()
    {
        image.color = !alive ? Color.red : isMy ? Color.green : Color.white;
        image.color *= isActive ? 1f : 0.8f;
    }

    public void PointerEnter()
    {
        if (alive && isActive)
            transform.localScale += new Vector3(0.2f, 0.2f, 0);
    }

    public void PointerExit()
    {
        if (alive && isActive)
            transform.localScale -= new Vector3(0.2f, 0.2f, 0);
    }

    public class Factory : PlaceholderFactory<CardPrefab>
    {
    }
}
