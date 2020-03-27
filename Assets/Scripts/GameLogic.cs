using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Pachik;

public class GameLogic : MonoBehaviour
{
    [Inject] private CardPrefab.Factory CardsPrefabFactory;
    [Inject] private MoveButton.Factory MoveButtonPrefabFactory;
    [Inject] protected CardsScriptableObject CardsData;

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
    private Vector2 askPosition = Vector2.one * -1;

    private Dictionary<int, Vector2> playersGridPos = new Dictionary<int, Vector2>();
    public List<Transform> PlayerDeckPositions = new List<Transform>();

    protected Player ActivePlayer
    {
        get { return gameDataManager.GetCurrentTurnPlayer(); }
    }
    
    protected List<Player> NonActivePlayers
    {
        get { return gameDataManager.Players.FindAll(p => p != ActivePlayer); }
    }

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

    protected void GameFlow()
    {
        OnGameStateChange(gameDataManager.GetGameState());
    }

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
                gui.HandleAnimation();
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
        bool result = false;
        switch (gameDataManager.GetGameState())
        {
            case EGameState.TURN_SHOOT:
                result = OnTurnShoot(card);
                break;
            case EGameState.TURN_ASK:
                result = OnTurnAsk(card);
                break;
        }
        if (result)
            OnGameStateChange(EGameState.TURN_FINISH);
    }
    void MoveButtonClickHandler(int index, MoveButton.Direction direction)
    {
        OnTurnMove(index, direction);
        OnGameStateChange(EGameState.TURN_FINISH);
    }

    private List<Player> InitPlayersOffline()
    {
        List<Player> Players = new List<Player>(MAX_PLAYERS);

        Player localPlayer = new Player("offline-player", "Player1", PlayerDeckPositions[0].position);
        localPlayer.IsLocal = true;
        Players.Add(localPlayer);

        Player remotePlayer = new Player("offline-bot", "Bot1", PlayerDeckPositions[1].position);
        // remotePlayer.IsAI = true;
        Players.Add(remotePlayer);

        return Players;
    }

    void RefreshGraphics()
    {
        int activePlayerId = ActivePlayer.PlayerId.GetHashCode();
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

                cardPrefabs.isAsk = (askPosition.x >= 0 && askPosition.y >= 0) && (Mathf.Abs(i - askPosition.x) <= 1 && Mathf.Abs(j - askPosition.y) <= 1);

                cardPrefabs.alive = !gameDataManager.DeadIds.Contains(cardPrefabs.Card.id);

                cardPrefabs.RefreshGraphics();
            }
        }
    }

    void UpdateField()
    {
        cardToPrefab.Clear();

        int count = 0;
        Card[,] cards = gameDataManager.GetGridCards();
        grid.constraintCount = cards.GetLength(1);

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

                Player player = gameDataManager.Players.Find(p => p.Card == cards[i, j]);
                if (player != null)
                {
                    playersGridPos[player.PlayerId.GetHashCode()] = new Vector2(i, j);
                    //playersGridPos.Insert(ActivePlayer.PlayerId.GetHashCode(), new Vector2(i, j));
                    //playersGridPos.Add(new Vector2(i, j));
                }

                prefabs[count].isMy = player != null && player.PlayerId == ActivePlayer.PlayerId;

                count++;
            }
        }

        for (int i = count; i < prefabs.Count; i++)
        {
            prefabs[i].gameObject.SetActive(false);
        }

        UpdateMoveButtons(moveUp.transform, MoveButton.Direction.Up, cards.GetLength(1));
        UpdateMoveButtons(moveLeft.transform, MoveButton.Direction.Left, cards.GetLength(0));
        UpdateMoveButtons(moveDown.transform, MoveButton.Direction.Down, cards.GetLength(1));
        UpdateMoveButtons(moveRight.transform, MoveButton.Direction.Right, cards.GetLength(0));
    }

    void UpdateMoveButtons(Transform parent, MoveButton.Direction direction, int count)
    {
        int index = (int)direction * 5;
        for (int i = index; i < index + count; i++)
        {
            if (moveButtons.Count <= i)
            {
                MoveButton mb = MoveButtonPrefabFactory.Create(parent, direction, i - index);
                mb.OnClick += MoveButtonClickHandler;
                moveButtons.Add(mb);
            }
            moveButtons[i].gameObject.SetActive(true);
        }

        count += index;

        for (int i = count; i < index + 5; i++)
        {
            moveButtons[i].gameObject.SetActive(false);
        }
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

    bool OnTurnShoot(Card card)
    {
        foreach (Player player in NonActivePlayers)
        {
            if (player.Card.id == card.id)
            {
                prefabs.Find(c => c.Card.id == card.id).Kill();
                gameDataManager.AddDeadId(card.id);
                gameDataManager.AddPlayerFrag(ActivePlayer, card);
                gameDataManager.DealRoleToPlayer(player);
                return true;
            }
        }

        return true;
    }

    bool OnTurnAsk(Card card)
    {
        Card[,] cards = gameDataManager.GetGridCards();
        List<Player> players = new List<Player>();
        bool breakLoop = false;
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                if (cards[i, j] == card)
                {
                    foreach (Player player in NonActivePlayers)
                    {
                        Vector2 pos = playersGridPos[player.PlayerId.GetHashCode()];
                        if (Mathf.Abs(i - pos.x) <= 1 && Mathf.Abs(j - pos.y) <= 1)
                            players.Add(player);
                    }

                    askPosition = new Vector2(i, j);

                    breakLoop = true;
                    break;
                }
            }

            if (breakLoop)
                break;
        }

        if (players.Count == 0)
            gui.TitleText = "There's no players in this area";
        else
        {
            string result = "Players in area: ";
            for (int i = 0; i < players.Count; i++)
                result += players[i].PlayerName + ", ";
            result = result.Substring(0, result.Length - 2);
            gui.TitleText = result;
        }

        OnGameStateChange(EGameState.GAME_ANIMATION);
        StartCoroutine(AskAnimationCoroutine());
        return false;
    }

    IEnumerator AskAnimationCoroutine()
    {
        yield return new WaitForSeconds(5);
        askPosition = Vector2.one * -1;
        OnGameStateChange(EGameState.TURN_FINISH);
    }

    void OnTurnMove(int index, MoveButton.Direction direction)
    {
        Card[,] cards = gameDataManager.GetGridCards();
        Card temp = null;
        switch (direction)
        {
            case MoveButton.Direction.Up:
                temp = cards[0, index];
                for (int i = 0; i < cards.GetLength(0) - 1; i++)
                    cards[i, index] = cards[i + 1, index];
                cards[cards.GetLength(0) - 1, index] = temp;
                break;
            case MoveButton.Direction.Down:
                temp = cards[cards.GetLength(0) - 1, index];
                for (int i = cards.GetLength(0) - 1; i > 0; i--)
                    cards[i, index] = cards[i - 1, index];
                cards[0, index] = temp;
                break;
            case MoveButton.Direction.Left:
                temp = cards[index, 0];
                for (int i = 0; i < cards.GetLength(1) - 1; i++)
                    cards[index, i] = cards[index, i + 1];
                cards[index, cards.GetLength(1) - 1] = temp;
                break;
            case MoveButton.Direction.Right:
                temp = cards[index, cards.GetLength(1) - 1];
                for (int i = cards.GetLength(1) - 1; i > 0; i--)
                    cards[index, i] = cards[index, i - 1];
                cards[index, 0] = temp;
                break;
        }
        gameDataManager.SetGridCards(cards);
    }

    bool TurnFinish()
    {
        if (gameDataManager.IsGameFinished())
        {
            OnGameStateChange(EGameState.GAME_FINISH);
            return true;
        }

        TryToCollapse();

        gameDataManager.NextTurnPlayer();

        UpdateField();

        OnGameStateChange(EGameState.TURN_IDLE);
        return true;
    }

    void TryToCollapse()
    {
        Card[,] cards = gameDataManager.GetGridCards();
        List<Vector2> cardsToRemove = new List<Vector2>();
        bool verticalCollapse = false;
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                if (gameDataManager.DeadIds.Contains(cards[i, j].id))
                {
                    verticalCollapse = true;
                    break;
                }
                verticalCollapse = false;
            }

            if (!verticalCollapse)
                break;
        }

        if (verticalCollapse)
        {
            Card[,] newCards = new Card[cards.GetLength(0),cards.GetLength(1) - 1];
            for (int i = 0; i < cards.GetLength(0); i++)
            {
                int count = 0;
                for (int j = 0; j < cards.GetLength(1); j++)
                {
                    if (gameDataManager.DeadIds.Contains(cards[i, j].id))
                        continue;
                    newCards[i, count++] = cards[i, j];
                }
            }
            gameDataManager.SetGridCards(newCards);
            return;
        }

        bool horizontalCollapse = false;
        for (int i = 0; i < cards.GetLength(1); i++)
        {
            for (int j = 0; j < cards.GetLength(0); j++)
            {
                if (gameDataManager.DeadIds.Contains(cards[j, i].id))
                {
                    horizontalCollapse = true;
                    break;
                }
                horizontalCollapse = false;
            }

            if (!horizontalCollapse)
                break;
        }

        if (horizontalCollapse)
        {
            Card[,] newCards = new Card[cards.GetLength(0) - 1, cards.GetLength(1)];
            for (int i = 0; i < cards.GetLength(1); i++)
            {
                int count = 0;
                for (int j = 0; j < cards.GetLength(0); j++)
                {
                    if (gameDataManager.DeadIds.Contains(cards[j, i].id))
                        continue;

                    newCards[count++, i] = cards[j, i];
                }
            }
            gameDataManager.SetGridCards(newCards);
        }
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