using UnityEngine;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{

    [SerializeField] private Button StartGameButton;

    [SerializeField] private Button ShootButton;
    [SerializeField] private Button MoveButton;
    [SerializeField] private Button AskButton;

    public delegate void OnStartGameClick();
    public event OnStartGameClick OnGameStartEvent;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        //ShootButton.onClick += onShootClick;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleHide()
    {
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
    }

    public void HandleGameInit()
    {
        StartGameButton.gameObject.SetActive(true);
        ShootButton.gameObject.SetActive(false);
        MoveButton.gameObject.SetActive(false);
        AskButton.gameObject.SetActive(false);
    }

    public void HandleTurnStart()
{
        StartGameButton.gameObject.SetActive(false);
        ShootButton.gameObject.SetActive(true);
        MoveButton.gameObject.SetActive(true);
        AskButton.gameObject.SetActive(true);
    }


    public void OnStartGamePress()
    {
        OnGameStartEvent();
    }

    public void OnShootPress()
    {

    }

}