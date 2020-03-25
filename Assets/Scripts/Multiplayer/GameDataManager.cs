using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pachik
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }

    [Serializable]
    public class GameDataManager
    {
        List<Player> players;

        Stack<Card> PoolOfCards;

        [SerializeField]
        ProtectedData protectedData;

        public GameDataManager(List<Player> _players, string roomId = "1234567890123456")
        {
            players = _players;
            protectedData = new ProtectedData(_players.Select(player => player.PlayerId).ToList(), roomId);
        }

        public void Shuffle()
        {
            byte[] cardValues = new byte[25];
            PoolOfCards = new Stack<Card>(25);

            // TODO add shuffle logic

            protectedData.SetGridCards(cardValues);
        }

        public void DealRoleToPlayer(Player player)
        {
            player.Card = PoolOfCards.Pop();
            protectedData.SetPlayerRole(player.PlayerId, player.Card.id);
        }

        public Player GetPlayerRole(Player player)
        {
            byte cardId = protectedData.GetPlayerRole(player.PlayerId);
            player.Card = new Card () { id = cardId };  // TODO fetch Card by ID
            return player;
        }

        public byte PlayerFrags(Player player)
        {
            return protectedData.PlayerFrags(player);
        }

        public void AddPlayerFrag(Player player)
        {
            protectedData.AddPlayerFrag(player);
        }

        public Player Winner()
        {
            return players.Find(p => p.PlayerId == protectedData.WinnerPlayerId());
        }

        public bool GameFinished()
        {
            return protectedData.GameFinished();
        }

        public void SetCurrentTurnPlayer(Player player)
        {
            protectedData.SetCurrentTurnPlayerId(player.PlayerId);
        }

        public Player GetCurrentTurnPlayer()
        {
            return players.Find(p => p.PlayerId == protectedData.GetCurrentTurnPlayerId());
        }

        public void SetGameState(EGameState gameState)
        {
            protectedData.SetGameState((byte)gameState);
        }

        public EGameState GetGameState()
        {
            return (EGameState)protectedData.GetGameState();
        }

        public EncryptedData EncryptedData()
        {
            Byte[] data = protectedData.ToArray();

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.data = data;

            return encryptedData;
        }

        public void ApplyEncrptedData(EncryptedData encryptedData)
        {
            if(encryptedData == null)
            {
                return;
            }

            protectedData.ApplyByteArray(encryptedData.data);
        }
    }
}
