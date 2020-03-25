using UnityEngine;
using SWNetwork;
using Pachik;
using UnityEngine.SceneManagement;

public class MultiplayerGameLogic : GameLogic
{
    NetCode netCode;

    GameDataManager gameDataManager;

    Player currentTurnPlayer;
    protected new Player ActivePlayer
    {
        get
        {
            return currentTurnPlayer;
        }
    }


    //****************** NetCode Events *********************//
    public void OnGameDataReady(EncryptedData encryptedData)
    {
        if (encryptedData == null)
        {
            Debug.Log("New game");
            if (NetworkClient.Instance.IsHost)
            {
                currState = EGameState.GAME_START;
                gameDataManager.SetGameState(currState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());

                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }
        else
        {
            gameDataManager.ApplyEncrptedData(encryptedData);
            currState = gameDataManager.GetGameState();
            currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
            //currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();

            if (currState > EGameState.GAME_START)
            {
                Debug.Log("Restore the game state");

                //restore player's cards
                //cardAnimator.DealDisplayingCards(localPlayer, gameDataManager.PlayerCards(localPlayer).Count, false);
                //cardAnimator.DealDisplayingCards(remotePlayer, gameDataManager.PlayerCards(remotePlayer).Count, false);


                base.GameFlow();
            }
        }
    }

    public void OnGameDataChanged(EncryptedData encryptedData)
    {
        gameDataManager.ApplyEncrptedData(encryptedData);
        currState = gameDataManager.GetGameState();
        currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
        //currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
    }

    public void OnGameStateChanged()
    {
        base.GameFlow();
    }

    public void OnOppoentConfirmed()
    {
        currState = EGameState.TURN_OPPORNENT_CONFIRMED;

        gameDataManager.SetGameState(currState);

        netCode.ModifyGameData(gameDataManager.EncryptedData());
        netCode.NotifyOtherPlayersGameStateChanged();
    }

    public void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
}
