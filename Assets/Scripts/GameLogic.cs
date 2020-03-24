using System;
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
    private List<CardPrefab> prefabs = new List<CardPrefab>();
    private Dictionary<Card, CardPrefab> cardToPrefab = new Dictionary<Card, CardPrefab>();

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
    private List<Vector2> playersGridPos;
    public List<Transform> PlayerDeckPositions = new List<Transform>();
    //[SerializeField] private int playedId = -1;  // Player number 0..MAX_PLAYERS-1

    protected void Awake()
    {
        MAX_PLAYERS = Mathf.Min(PlayerDeckPositions.Count, MAX_PLAYERS);
        Players = new List<Player>(MAX_PLAYERS);
        playersGridPos = new List<Vector2>(MAX_PLAYERS);

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
                currState = newState;
                return;
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
                if (TurnFinish()) return;
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
    void CardClickHandler(Card card)
    {
        // TODO get direction of move
        OnTurn(card, 2, Vector2.right);
        OnGameStateChange(EGameState.TURN_FINISH);
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
        localPlayer.Position = PlayerDeckPositions[0].position;
        //localPlayer.BookPosition = BookPositions[0].position;
        Players.Add(localPlayer);


        Player remotePlayer = new Player();
        remotePlayer.PlayerId = "offline-bot";
        remotePlayer.PlayerName = "Bot1";
        remotePlayer.Position = PlayerDeckPositions[1].position;
        //remotePlayer.BookPosition = BookPositions[1].position;
        remotePlayer.IsAI = true;

        Players.Add(remotePlayer);
    }

    void RefreshGraphics()
    {
        int localXPos = (int)playersGridPos[activePlayerId].x;
        int localYPos = (int)playersGridPos[activePlayerId].y;


        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                CardPrefab cardPrefabs = cardToPrefab[cards[i, j]];

                cardPrefabs.isActive = (currState == EGameState.TURN_ASK || currState == EGameState.TURN_SHOOT) &&
                    (Mathf.Abs(i - localXPos) <= 1 && Mathf.Abs(j - localYPos) <= 1 && (localXPos != i || localYPos != j));

                cardPrefabs.RefreshGraphics();
            }
        }
    }

    void UpdateField()
    {
        cardToPrefab.Clear();

        int count = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (prefabs.Count <= count)
                {
                    CardPrefab c = CardsPrefabFactory.Create();
                    c.enabled = false;
                    prefabs.Add(c);

                    c.OnCardClickEvent += CardClickHandler;
                }

                cardToPrefab.Add(cards[i, j], prefabs[count]);
                prefabs[count].Card = cards[i, j];

                if (ActivePlayer.Card == cards[i, j])
                {
                    prefabs[count].isMy = true;
                    //playersGridPos[activePlayerId] = new Vector2(i, j);
                    playersGridPos.Insert(activePlayerId, new Vector2(i, j));
                    //playersGridPos.Add(new Vector2(i, j));
                }
                else
                {
                    prefabs[count].isMy = false;
                }

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
            Players[i].Card = GetTopDeck();
        }

        UpdateField();
    }

    void OnTurn(Card card, int movePosition, Vector2 moveDirection)
    {
        if (currState == EGameState.TURN_MOVE)
        {
            OnTurnMove(movePosition, moveDirection);
        } else
        {
            //OnShoot(card);
        }
    }

    void OnTurnMove(int position, Vector2 direction)
    {
        if (direction == Vector2.left)
        {

            int i = position;
            Card card_0 = cards[i, 0];
            for (int j = 0; j < width - 1; j++)
            {
                cards[i, j] = cards[i, (j + (int)direction.x) % width];
            }
            cards[i, width - 1] = card_0;
        }
        else
        {

        }
    }

    bool TurnFinish()
    {
        if (ActivePlayer.NumberOfFrags > 3)
        {
            OnGameStateChange(EGameState.GAME_FINISH);
            return true;
        }

        activePlayerId = ++activePlayerId % totalPlayers;
        UpdateField();

        OnGameStateChange(EGameState.TURN_IDLE);
        return true;
    }

    void EnableCards(bool enable = true)
    {
        foreach (CardPrefab card in prefabs)
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
