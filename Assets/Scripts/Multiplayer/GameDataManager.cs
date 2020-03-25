using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

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
        private const int MAX_WIDTH = 5;
        private const int MAX_HEIGHT = 5;
        private int width = MAX_WIDTH;
        private int height = MAX_HEIGHT;

        [Inject] private CardsScriptableObject CardsData;

        [SerializeField] ProtectedData protectedData;

        private Stack<Card> PoolOfCards;    // deck

        public Card[,] Cards { get; } = new Card[MAX_WIDTH, MAX_HEIGHT];
        public List<Player> Players { get; }

        public GameDataManager(List<Player> _players, string roomId = "1234567890123456")
        {
            Players = _players;
            protectedData = new ProtectedData(_players.Select(player => player.PlayerId).ToList(), roomId);

            InitCardsData();
        }

        void InitCardsData()
        {
            List<Card> newCards = new List<Card>(CardsData.Cards.Take(width * height));
            newCards.Shuffle();
            PoolOfCards = new Stack<Card>(newCards);

            List<Card> tempDeck = newCards; // new List<Card>(PoolOfCards);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = UnityEngine.Random.Range(0, tempDeck.Count);
                    Cards[i, j] = tempDeck[index];
                    tempDeck.RemoveAt(index);
                }
            }
        }

        //public void Shuffle()
        //{
        //    byte[] cardValues = new byte[25];
        //    PoolOfCards = new Stack<Card>(25);

        //    // TODO add shuffle logic

        //    protectedData.SetGridCards(cardValues);
        //}

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
            return Players.Find(p => p.PlayerId == protectedData.WinnerPlayerId());
        }

        public bool IsGameFinished()
        {
            return protectedData.GameFinished();
        }

        public void NextTurnPlayer()
        {
            string id = protectedData.GetCurrentTurnPlayerId();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].PlayerId == id)
                {
                    protectedData.SetCurrentTurnPlayerId(Players[(i + 1) % Players.Count].PlayerId);
                }
            }
        }

        public Player GetCurrentTurnPlayer()
        {
            return Players.Find(p => p.PlayerId == protectedData.GetCurrentTurnPlayerId());
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
