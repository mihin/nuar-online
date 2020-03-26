using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Pachik;

public class GameLogic : MonoBehaviour
{
    [Inject] private CardPrefab.Factory CardsPrefabFactory;
    [Inject] private MoveButton.Factory MoveButtonPrefabFactory;
    [Inject] private CardsScriptableObject CardsData;

    private List<CardPrefab> prefabs = new List<CardPrefab>();
    private List<MoveButton> moveButtons = new List<MoveButton>();
    private Dictionary<Card, CardPrefab> cardToPrefab = new Dictionary<Card, CardPrefab>();

    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private HorizontalOrVerticalLayoutGroup moveUp, moveDown, moveLeft, moveRight;
    [SerializeField] private GameGUI gui;
    protected GameDataManager gameDataManager;


    private String errorMessage;
    private bool isOffline = true;
    private int MAX_PLAYERS = 6;


    //[SerializeField] protected EGameState currState = EGameState.NONE;
    private Dictionary<int, Vector2> playersGridPos = new Dictionary<int, Vector2>();
    public List<Transform> PlayerDeckPositions = new List<Transform>();
    //[SerializeField] private int playedId = -1;  // Player number 0..MAX_PLAYERS-1

    protected void Awake()
    {
        MAX_PLAYERS = Mathf.Min(PlayerDeckPositions.Count, MAX_PLAYERS);
        playersGridPos = new Dictionary<int, Vector2>(MAX_PLAYERS);

        InitGameData();
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

    // TODO override for multiplayer
    protected void InitGameData()
    {
        List<Player> players = InitPlayersOffline();

        gameDataManager = new GameDataManager(players, CardsData.Cards);

        OnGameStateChange(EGameState.IDLE);
    }

    //protected void GameFlow()
    //{
    //    OnGameStateChange(currState);
    //}

    protected void OnGameStateChange(EGameState newState)
    {
        if (newState == gameDataManager.GetGameState())
            return;

        switch (newState)
        {
            case EGameState.IDLE:
                gui.HandleHide();
                Invoke("ShowStartGameGUI", 1);
                gameDataManager.SetGameState(newState);
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
                gui.HandleHide();
                if (TurnFinish()) return;
                break;
            case EGameState.GAME_ANIMATION:
                EnableCards(false);
                break;
            case EGameState.GAME_FINISH:
                EnableCards(false);
                gui.HandleGameFinish(gameDataManager.Winner().PlayerName);
                break;
            default:
                break;
        }

        Debug.Log("OnGameStateChange " + newState);

        gameDataManager.SetGameState(newState);
        RefreshGraphics();
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
        OnTurn(2, MoveButton.Direction.Right);
        OnGameStateChange(EGameState.TURN_FINISH);
    }
    void MoveButtonClickHandler(int index, MoveButton.Direction direction)
    {
        OnTurn(index, direction);
        OnGameStateChange(EGameState.TURN_FINISH);
    }

    private List<Player> InitPlayersOffline()
    {
        List<Player> Players = new List<Player>(MAX_PLAYERS);

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

        return Players;
    }

    void RefreshGraphics()
    {
        int activePlayerId = gameDataManager.GetCurrentTurnPlayer().PlayerId.GetHashCode();
        int localXPos = (int)playersGridPos[activePlayerId].x;
        int localYPos = (int)playersGridPos[activePlayerId].y;

        Card[,] cards = gameDataManager.GetGridCards();
        EGameState currState = gameDataManager.GetGameState();

        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                CardPrefab cardPrefabs = cardToPrefab[cards[i,j]];

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
        Card[,] cards = gameDataManager.GetGridCards();
        Player ActivePlayer = gameDataManager.GetCurrentTurnPlayer();

        //Debug.Log("UpdateField, " + ActivePlayer);

        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            { 
                if (prefabs.Count <= count)
                {
                    CardPrefab c = CardsPrefabFactory.Create(grid.transform);
                    c.enabled = false;
                    prefabs.Add(c);

                    c.OnCardClickEvent += CardClickHandler;
                }

                cardToPrefab.Add(cards[i, j], prefabs[count]);
                prefabs[count].Card = cards[i, j];
                prefabs[count].gameObject.SetActive(true);

                if (ActivePlayer.Card == cards[i, j])
                {
                    prefabs[count].isMy = true;
                    playersGridPos[ActivePlayer.PlayerId.GetHashCode()] = new Vector2(i, j);
                    //playersGridPos.Insert(ActivePlayer.PlayerId.GetHashCode(), new Vector2(i, j));
                    //playersGridPos.Add(new Vector2(i, j));
                }
                else
                {
                    prefabs[count].isMy = false;
                }

                count++;
            }
        }

        for (int i = count; i < cardToPrefab.Count; i++)
        {
            prefabs[i].gameObject.SetActive(false);
        }

        count = 0;
        UpdateMoveButtons(moveUp.transform, MoveButton.Direction.Up, cards.GetLength(0), ref count);
        UpdateMoveButtons(moveDown.transform, MoveButton.Direction.Down, cards.GetLength(0), ref count);
        UpdateMoveButtons(moveLeft.transform, MoveButton.Direction.Left, cards.GetLength(1), ref count);
        UpdateMoveButtons(moveRight.transform, MoveButton.Direction.Right, cards.GetLength(1), ref count);

        for (int i = count; i < moveButtons.Count; i++)
        {
            moveButtons[i].gameObject.SetActive(false);
        }
    }

    void UpdateMoveButtons(Transform parent, MoveButton.Direction direction, int count, ref int all)
    {
        for (int i = 0; i < count; i++)
        {
            if (moveButtons.Count <= all + i)
            {
                MoveButton mb = MoveButtonPrefabFactory.Create(parent, direction, i);
                mb.OnClick += MoveButtonClickHandler;
                moveButtons.Add(mb);
            }
            moveButtons[i].gameObject.SetActive(true);
        }

        all += count;
    }

    void DisplayError()
    {
        // TODO display error
        Debug.LogError(errorMessage != null ? errorMessage : "Unknown error");
    }

    void StartGame()
    {
        //if (!CheckPlayers())
        //{
        //    errorMessage = "Can not start the game, not enough players";
        //    OnGameStateChange(EGameState.ERROR);
        //    return;
        //}

        //activePlayerId = 0;
        HandoutRoles();

        OnGameStateChange(EGameState.TURN_IDLE); // wait curr player to choose action
    }

    void HandoutRoles()
    {
        foreach (Player player in gameDataManager.Players)
        {
            gameDataManager.DealRoleToPlayer(player);
        }

        UpdateField();
    }

    void OnTurn(int index, MoveButton.Direction direction)
    {
        if (gameDataManager.GetGameState() == EGameState.TURN_MOVE)
        {
            OnTurnMove(index, direction);
        }
        else
        {
            //OnShoot(card);
        }
    }

    void OnTurnMove(int index, MoveButton.Direction direction)
    {
        Card[,] cards = gameDataManager.GetGridCards();
        Card temp = null;
        switch (direction)
        {
            case MoveButton.Direction.Up:
                temp = cards[0, index];
                for (int i = 0; i < cards.GetLength(1) - 1; i++)
                    cards[i, index] = cards[i + 1, index];
                cards[cards.GetLength(1) - 1, index] = temp;
                break;
            case MoveButton.Direction.Down:
                temp = cards[cards.GetLength(1) - 1, index];
                for (int i = cards.GetLength(1) - 1; i > 0; i--)
                    cards[i, index] = cards[i - 1, index];
                cards[0, index] = temp;
                break;
            case MoveButton.Direction.Left:
                temp = cards[index, 0];
                for (int i = 0; i < cards.GetLength(0) - 1; i++)
                    cards[index, i] = cards[index, i + 1];
                cards[index, cards.GetLength(1) - 1] = temp;
                break;
            case MoveButton.Direction.Right:
                temp = cards[index, cards.GetLength(0) - 1];
                for (int i = cards.GetLength(0) - 1; i > 0; i--)
                    cards[index, i] = cards[index, i - 1];
                cards[index, 0] = temp;
                break;
        }
        gameDataManager.SetGridCards(cards);
        UpdateField();
    }

    bool TurnFinish()
    {
        if (gameDataManager.IsGameFinished())
        {
            OnGameStateChange(EGameState.GAME_FINISH);
            return true;
        }

        gameDataManager.NextTurnPlayer();

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
        gui.HandleTurnStart(gameDataManager.GetCurrentTurnPlayer().PlayerName);
    }

}