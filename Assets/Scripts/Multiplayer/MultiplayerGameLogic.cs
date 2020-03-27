using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SWNetwork;
using Pachik;
using UnityEngine.SceneManagement;

public class MultiplayerGameLogic : GameLogic
{
    NetCode netCode;
    
    protected override void InitGameData()
    {
        netCode = FindObjectOfType<NetCode>();
        
        NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
        {
            if (successful)
            {
                int count = 0;
                List<Player> players = reply.players.Select(swPlayer => Player.fromNetworkPlayer(swPlayer).SetPosition(PlayerDeckPositions[count++].position)).ToList();

                players.Find(p => p.PlayerId == NetworkClient.Instance.PlayerId).IsLocal = true;

                if (CardsData == null || CardsData.Cards == null || CardsData.Cards.Count == 0)
                {
                    CardsData = new CardsScriptableObject();
                    CardsData.LoadCachedData();
                }

                gameDataManager = new GameDataManager(players, CardsData.Cards, NetworkClient.Lobby.RoomId);
                netCode.EnableRoomPropertyAgent();
                
                OnGameStateChange(EGameState.IDLE);
            }
            else
            {
                Debug.LogError("Failed to get players in room.");
            }
        });
    }
    
    
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
            OnGameDataChanged(encryptedData);
        }
    }

    public void OnGameDataChanged(EncryptedData encryptedData)
    {
        gameDataManager.ApplyEncrptedData(encryptedData);
        //EGameState currState = gameDataManager.GetGameState();
        //currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
        //OnGameStateChange(currState);
        GameFlow();
    }

    public void OnGameStateChanged()
    {
        base.GameFlow();
    }

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
