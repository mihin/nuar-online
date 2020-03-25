using UnityEngine;
using SWNetwork;
using Pachik;
using UnityEngine.SceneManagement;

public class MultiplayerGameLogic : GameLogic
{
    NetCode netCode;

    GameDataManager gameDataManager;

    Player currentTurnPlayer;


    //****************** NetCode Events *********************//
    public void OnGameDataReady(EncryptedData encryptedData)
    {
        if (encryptedData == null)
        {
            Debug.Log("New game");
            if (NetworkClient.Instance.IsHost)
            {
                //currState = EGameState.GAME_START;
                //gameDataManager.SetGameState(EGameState.GAME_START);
                OnGameStateChange(EGameState.GAME_START);

                netCode.ModifyGameData(gameDataManager.EncryptedData());

                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }
        else
        {
            gameDataManager.ApplyEncrptedData(encryptedData);
            EGameState currState = gameDataManager.GetGameState();
            currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
            //currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();

            if (currState > EGameState.GAME_START)
            {
                Debug.Log("Restore the game state");

                //restore player's cards
                //cardAnimator.DealDisplayingCards(localPlayer, gameDataManager.PlayerCards(localPlayer).Count, false);
                //cardAnimator.DealDisplayingCards(remotePlayer, gameDataManager.PlayerCards(remotePlayer).Count, false);


                //base.GameFlow();.
                //gameDataManager.SetGameState(EGameState.GAME_START);
                OnGameStateChange(EGameState.GAME_START);

            }
        }
    }

    public void OnGameDataChanged(EncryptedData encryptedData)
    {
        gameDataManager.ApplyEncrptedData(encryptedData);
        EGameState currState = gameDataManager.GetGameState();
        currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
        //currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();

        OnGameStateChange(currState);
    }

    //public void OnGameStateChanged()
    //{
    //    base.GameFlow();
    //}

    public void OnOppoentConfirmed()
    {
        //currState = EGameState.TURN_OPPORNENT_CONFIRMED;
        //gameDataManager.SetGameState(currState);
        OnGameStateChange(EGameState.TURN_OPPORNENT_CONFIRMED);

        netCode.ModifyGameData(gameDataManager.EncryptedData());
        netCode.NotifyOtherPlayersGameStateChanged();
    }

    public void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
}
