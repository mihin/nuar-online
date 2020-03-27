using UnityEngine;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{

    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button ShootButton;
    [SerializeField] private Button MoveButton;
    [SerializeField] private Button AskButton;
    [SerializeField] private Button CancelButton;
    [SerializeField] private Text Title;
    [SerializeField] private GameObject MoveButtons;

    public delegate void OnButtonClick();
    public event OnButtonClick OnGameStartEvent;
    public event OnButtonClick OnShootChosenEvent;
    public event OnButtonClick OnAskChosenEvent;
    public event OnButtonClick OnMoveChosenEvent;
    public event OnButtonClick OnCancelEvent;

    public string TitleText {
        get { return Title.text; }
        set
        {
            if (value != null)
            {
                Title.gameObject.SetActive(true);
                Title.text = value;
            }
            else
            {
                Title.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {

    }

    private void OnEnable()
    {

    }

    void Update()
    {

    }

    public void HandleHide()
    {
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        MoveButtons.SetActive(false);

        TitleText = null;
    }

    public void HandleAnimation()
    {
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        MoveButtons.SetActive(false);
    }

    public void HandleGameInit()
    {
        StartGameButton.gameObject.SetActive(true);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        MoveButtons.SetActive(false);

        TitleText = "Press Start Game";
    }

    public void HandleTurnStart(string playerName)
    {   
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(true);
        MoveButton.gameObject.SetActive(true);
        AskButton.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(false);
        MoveButtons.SetActive(false);

        TitleText = playerName + ". Select an action";
    }

    public void HandleGameFinish(string playerName)
    {
        StartGameButton.gameObject.SetActive(true);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        MoveButtons.SetActive(false);

        TitleText = playerName + " wins!!";
    }

    public void HandleAskShoot()
    {
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(true);
        MoveButtons.SetActive(false);

        TitleText = null;
    }

    public void HandleMove()
    {
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(true);
        MoveButtons.SetActive(true);

        TitleText = null;
    }

    public void HandleShootMode()
    {
        HandleAskShoot();
        TitleText = "Select a person to kill";
    }

    public void HandleAskMode()
    {
        HandleAskShoot();
        TitleText = "Select a person to ask";
    }

    public void HandleMoveMode()
    {
        HandleMove();
        TitleText = "Select a direction to move";
    }

    public void OnStartGamePress()
    {
        OnGameStartEvent();
    }

    public void OnShootPress()
    {
        OnShootChosenEvent();
    }

    public void OnAskPress()
    {
        OnAskChosenEvent();
    }

    public void OnMovePress()
    {
        OnMoveChosenEvent();
    }

    public void OnCancelPress()
    {
        OnCancelEvent();
    }
}