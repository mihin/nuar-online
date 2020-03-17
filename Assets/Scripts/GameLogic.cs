using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameLogic : MonoBehaviour
{
    private const int MAX_WIDTH = 5;
    private const int MAX_HEIGHT = 5;
    private int width = MAX_WIDTH;
    private int height = MAX_HEIGHT;
    private Card[,] cards = new Card[MAX_WIDTH,MAX_HEIGHT];
    private List<CardPrefab> cardPrefabs = new List<CardPrefab>();

    [SerializeField] private GridLayoutGroup grid;
    [Inject] private CardsScriptableObject CardsData;
    [Inject] private CardPrefab.Factory CardsPrefabFactory;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cards[i, j] = CardsData.Cards[Random.Range(0, CardsData.Cards.Count)];
            }
        }

        RefreshGraphics();
    }

    void RefreshGraphics()
    {
        int count = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cardPrefabs.Count <= count)
                {
                    cardPrefabs.Add(CardsPrefabFactory.Create());
                }
                cardPrefabs[count].Card = cards[i, j];
                count++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
