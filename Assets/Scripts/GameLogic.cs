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
    private int localXPos = -1, localYPos = -1;

    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private GameGUI gui;
    [Inject] private CardsScriptableObject CardsData;
    [Inject] private CardPrefab.Factory CardsPrefabFactory;

    private String errorMessage;
    private bool isOffline = true;
    private int MAX_PLAYERS = 6;
    private int activePlayerId = -1;
    private Player ActivePlayer
    {
        get { return Players[activePlayerId]; }
    }
    private int totalPlayers
    {
        get { return Players.Count; }
    }

    [SerializeField] private EGameState currState = EGameState.NONE;
    private List<Player> Players;
    public List<Transform> PlayerPositions = new List<Transform>();
    //[SerializeField] private int playedId = -1;  // Player number 0..MAX_PLAYERS-1

    protected void Awake()
    {
        MAX_PLAYERS = Mathf.Min(PlayerPositions.Count, MAX_PLAYERS);
        Players = new List<Player>(MAX_PLAYERS);

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
        gui.OnCancelEvent += CancelHandler;
    }

    void OnDisable()
    {
        gui.OnGameStartEvent -= GameStartClickHandler;
        gui.OnMoveChosenEvent -= MoveChosenHandler;
        gui.OnShootChosenEvent -= ShootChosenHandler;
        gui.OnAskChosenEvent -= AskChosenHandler;
        gui.OnCancelEvent -= CancelHandler;
    }


void OnGameStateChange(EGameState newState)
    {
        if (newState == currState)
            return;

        switch (newState)
        {
            case EGameState.IDLE:
                gui.HandleHide();
                CheckPlayers();
                Invoke("ShowStartGameGUI", 1);
                break;
            case EGameState.ERROR:
                DisplayError();
                return;
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
            case EGameState.TURN_FINISH:
                TurnFinish();
                break;
            case EGameState.GAME_ANIMATION:
                EnableCards(false);
                break;
            case EGameState.GAME_FINISH:
                EnableCards(false);
                gui.HandleGameFinish(ActivePlayer.PlayerName);
                break;
            default:
                break;
        }

        currState = newState;
        RefreshGraphics();
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
    void CancelHandler()
    {
        OnGameStateChange(EGameState.TURN_IDLE);
    }
    
    void InitCardsData()
    {
        deck = new List<Card>(CardsData.Cards);
        deck.Shuffle();
        if (deck.Count > width * height)
            deck.RemoveRange(width * height, deck.Count - width * height);
        List<Card> tempDeck = new List<Card>(deck);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int index = UnityEngine.Random.Range(0, tempDeck.Count);
                cards[i, j] = tempDeck[index];
                tempDeck.RemoveAt(index);
            }
        }
    }

    Card GetTopDeck()
    {
        Card c = deck[0];
        deck.RemoveAt(0);
        return c;
    }

    bool CheckPlayers()
    {
        if (totalPlayers > 1)
            return true;

        if (isOffline)
            InitPlayersOffline();

        // TODO Wait and Add network players

        return totalPlayers > 1;
    }

    void InitPlayersOffline()
    {
        Player localPlayer = new Player();
        localPlayer.PlayerId = "offline-player";
        localPlayer.PlayerName = "Player1";
        localPlayer.Position = PlayerPositions[0].position;
        //localPlayer.BookPosition = BookPositions[0].position;
        Players.Add(localPlayer);


        Player remotePlayer = new Player();
        remotePlayer.PlayerId = "offline-bot";
        remotePlayer.PlayerName = "Bot1";
        remotePlayer.Position = PlayerPositions[1].position;
        //remotePlayer.BookPosition = BookPositions[1].position;
        remotePlayer.IsAI = true;

        Players.Add(remotePlayer);
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
                    CardPrefab c = CardsPrefabFactory.Create();
                    c.enabled = false;
                    cardPrefabs.Add(c);
                }
                cardPrefabs[count].Card = cards[i, j];
                cardPrefabs[count].isMy = localXPos == i && localYPos == j;
                cardPrefabs[count].isActive = (currState != EGameState.TURN_ASK && currState != EGameState.TURN_SHOOT) || (Mathf.Abs(i - localXPos) <= 1 && Mathf.Abs(j - localYPos) <= 1 && (localXPos != i || localYPos != j));
                cardPrefabs[count].RefreshGraphics();
                count++;
            }
        }
    }

    void DisplayError()
    {
        // TODO display error
        Debug.LogError(errorMessage!=null ? errorMessage : "Unknown error");
    }

    void StartGame()
    {
        if (!CheckPlayers())
        {
            errorMessage = "Can not start the game, not enough players";
            OnGameStateChange(EGameState.ERROR);
            return;
        }

        activePlayerId = 0;
        HandoutRoles();

        OnGameStateChange(EGameState.TURN_IDLE); // wait curr player to choose action
    }

    void HandoutRoles()
    {
        for (int i = 0; i < totalPlayers; i++)
        {
            Players[i].PlayerRole = GetTopDeck();
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cards[i, j].name == ActivePlayer.PlayerRole.name)
                {
                    localXPos = i;
                    localYPos = j;
                    return;
                }
            }
        }
    }

    void TurnFinish()
    {
        if (ActivePlayer.NumberOfFrags > 3)
        {
            OnGameStateChange(EGameState.GAME_FINISH);
            return;
        }

        activePlayerId = ++activePlayerId % totalPlayers;
        OnGameStateChange(EGameState.TURN_IDLE);
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
        gui.HandleTurnStart(ActivePlayer.PlayerName);
    }

}
