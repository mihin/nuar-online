using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GoFish;

public class GameLogic : MonoBehaviour
{
    private const int MAX_WIDTH = 5;
    private const int MAX_HEIGHT = 5;
    private int width = MAX_WIDTH;
    private int height = MAX_HEIGHT;
    private Card[,] cards = new Card[MAX_WIDTH, MAX_HEIGHT];
    private List<Card> deck = new List<Card>(MAX_WIDTH * MAX_HEIGHT);
    private List<CardPrefab> cardPrefabs = new List<CardPrefab>();


    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private GameGUI gui;
    [Inject] private CardsScriptableObject CardsData;
    [Inject] private CardPrefab.Factory CardsPrefabFactory;


    private int MAX_PLAYERS = 6;

    [SerializeField] private EGameState currState = EGameState.NONE;
    [SerializeField] protected Player localPlayer;
    [SerializeField] protected Player remotePlayer;
    private List<Player> Players;
    public List<Transform> PlayerPositions = new List<Transform>();
    //[SerializeField] private int playedId = -1;  // Player number 0..MAX_PLAYERS-1

    protected void Awake()
    {
        //Debug.Log("GameLogic Awake");

        MAX_PLAYERS = Mathf.Min(PlayerPositions.Count, MAX_PLAYERS);

        localPlayer = new Player();
        localPlayer.PlayerId = "offline-player";
        localPlayer.PlayerName = "Player";
        localPlayer.Position = PlayerPositions[0].position;
        //localPlayer.BookPosition = BookPositions[0].position;

        remotePlayer = new Player();
        remotePlayer.PlayerId = "offline-bot";
        remotePlayer.PlayerName = "Bot";
        remotePlayer.Position = PlayerPositions[1].position;
        //remotePlayer.BookPosition = BookPositions[1].position;
        remotePlayer.IsAI = true;

        Players.Add(localPlayer);
        Players.Add(remotePlayer);

        //cardAnimator = FindObjectOfType<CardAnimator>();

        gui.HandleHide();
        InitCardsData();
        OnGameStateChange(EGameState.IDLE);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    void OnEnable()
    {
        gui.OnGameStartEvent += GameStartClickHandler;
        gui.OnMoveChosenEvent += MoveChosenHandler;
        gui.OnShootChosenEvent += ShootChosenHandler;
        gui.OnAskChosenEvent += AskChosenHandler;
    }

    void OnDisable()
    {
        gui.OnGameStartEvent -= GameStartClickHandler;
        gui.OnMoveChosenEvent -= MoveChosenHandler;
        gui.OnShootChosenEvent -= ShootChosenHandler;
        gui.OnAskChosenEvent -= AskChosenHandler;
    }


    void OnGameStateChange(EGameState newState)
    {
        if (newState == currState)
            return;

        switch (newState)
        {
            case EGameState.IDLE:
                RefreshGraphics();
                CheckPlayers();
                Invoke("ShowStartGameGUI", 1);
                break;
            case EGameState.GAME_START:
                StartGame();
                break;
            case EGameState.TURN_IDLE:
                Invoke("ShowMakeTurnGUI", 1);
                break;
            case EGameState.TURN_SHOOT:
                gui.HandleShootMode();
                EnableCards();
                break;
            case EGameState.TURN_ASK:
                gui.HandleAskMode();
                EnableCards();
                break;
            case EGameState.TURN_MOVE:
                gui.HandleMoveMode();
                EnableCards();
                break;
            case EGameState.GAME_ANIMATION:
                EnableCards(false);
                break;
            default:
                break;
        }

        currState = newState;
        Debug.Log("OnGameStateChange " + currState);
    }

    void GameStartClickHandler()
    {
        OnGameStateChange(EGameState.GAME_START); // wait curr player to choose action
    }
    void ShootChosenHandler()
    {
        OnGameStateChange(EGameState.TURN_SHOOT);
    }
    void AskChosenHandler()
    {
        OnGameStateChange(EGameState.TURN_ASK);
    }
    void MoveChosenHandler()
    {
        OnGameStateChange(EGameState.TURN_MOVE);
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

        for (int i = 0; i < width* height; i++)
            deck.Add(CardsData.Cards[UnityEngine.Random.Range(0, CardsData.Cards.Count)]);
    }

    void CheckPlayers()
    {
        // TODO Players.Add()
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
                cardPrefabs[count].enabled = false;
                count++;
            }
        }

        // TODO deck.Shuffle();
    }

    void StartGame()
    {
        HandoutRoles();

        OnGameStateChange(EGameState.TURN_IDLE); // wait curr player to choose action
    }

    void HandoutRoles()
    {

    }

    void EnableCards(bool enable = true)
    {
        foreach (CardPrefab card in cardPrefabs)
        {
            card.enabled = enable;
        }
    }

    void ShowStartGameGUI()
    {
        gui.HandleGameInit();
    }

    void ShowMakeTurnGUI()
    {
        gui.HandleTurnStart();
    }

}
