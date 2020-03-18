using UnityEngine;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{

    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button ShootButton;
    [SerializeField] private Button MoveButton;
    [SerializeField] private Button AskButton;
    [SerializeField] private Text Title;

    public delegate void OnButtonClick();
    public event OnButtonClick OnGameStartEvent;
    public event OnButtonClick OnShootChosenEvent;
    public event OnButtonClick OnAskChosenEvent;
    public event OnButtonClick OnMoveChosenEvent;

    private string TitleText {
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

        TitleText = null;
    }

    public void HandleGameInit()
    {
        StartGameButton.gameObject.SetActive(true);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);

        TitleText = "Press Start Game";
    }

    public void HandleTurnStart()
    {   
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(true);
        MoveButton.gameObject.SetActive(true);
        AskButton.gameObject.SetActive(true);

        TitleText = "Select an action";
    }

    public void HandleShootMode()
    {
        HandleHide();
        TitleText = "Select a person to kill";
    }

    public void HandleAskMode()
    {
        HandleHide();
        TitleText = "Select a person to ask";
    }

    public void HandleMoveMode()
    {
        HandleHide();
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

}