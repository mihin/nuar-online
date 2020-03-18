using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameLogic : MonoBehaviour
{
    private const int MAX_PLAYERS = 6;
    private const int MAX_WIDTH = 5;
    private const int MAX_HEIGHT = 5;
    private int width = MAX_WIDTH;
    private int height = MAX_HEIGHT;
    private int totalPlayers;
    private Card[,] cards = new Card[MAX_WIDTH, MAX_HEIGHT];
    private List<CardPrefab> cardPrefabs = new List<CardPrefab>();

    [SerializeField] private EGameState currState = EGameState.NONE;

    [SerializeField] private int playedId = -1;  // Player number 0..MAX_PLAYERS-1

    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private GameGUI gui;
    [Inject] private CardsScriptableObject CardsData;
    [Inject] private CardPrefab.Factory CardsPrefabFactory;


    void Start()
    {
        InitCardsData();
        OnGameStateChange(EGameState.IDLE);
    }

    void Update()
    {

    }


    void OnGameStateChange(EGameState newState)
    {
        if (newState == currState)
            return;

        switch (newState)
        {
            case EGameState.IDLE:
                RefreshGraphics();
                InitPlayers();
                break;
            default:
                break;
        }

        currState = newState;
        Debug.Log("OnGameStateChange " + currState);

    }

    void InitCardsData()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cards[i, j] = CardsData.Cards[UnityEngine.Random.Range(0, CardsData.Cards.Count)];
            }
        }
    }

    void InitPlayers()
    {
        totalPlayers = 2;
        playedId = 0;
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

}
