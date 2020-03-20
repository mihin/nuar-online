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

    public delegate void OnCardClick(Card card);
    public event OnCardClick OnCardClickEvent;

    private Vector3 cachedScale = Vector3.one;

    void Start()
    {
        cachedScale = transform.localScale;
    }

    void Update()
    {
    }

    public void RefreshGraphics()
    {
        image.color = !alive ? Color.red : isMy ? Color.green : Color.white;
        image.color *= isActive ? 1f : 0.8f;
    }

    public void OnClick()
    {
        if (isActive)
            OnCardClickEvent(card);

        Vector3.Lerp(cachedScale * 1.3f, cachedScale, 0.3f);
    }

    public void PointerEnter()
    {
        if (alive && isActive)
            transform.localScale = cachedScale + new Vector3(0.2f, 0.2f, 0);
    }

    public void PointerExit()
    {
        //if (alive && isActive)
        //    transform.localScale -= new Vector3(0.2f, 0.2f, 0);
        transform.localScale = cachedScale;
    }

    public class Factory : PlaceholderFactory<CardPrefab>
    {
    }
}
