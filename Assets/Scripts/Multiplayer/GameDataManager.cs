﻿using System;
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

        [Inject] private CardsScriptableObject CardsData;

        [SerializeField] private ProtectedData protectedData;

        public List<Player> Players { get; }

        [SerializeField] private Stack<Card> PoolOfCards;    // deck !HOST only

        public List<byte> DeadIds { get; }

        private Dictionary<byte, Card> CardsDictionary;

        //public Card[,] Cards { get; } = new Card[MAX_WIDTH, MAX_HEIGHT];    // grid


        public GameDataManager(List<Player> _players, string roomId = "1234567890123456")
        {
            Players = _players;
            DeadIds = new List<byte>();
            // byte[] gridCards = InitCardsData(allCards);

            protectedData = new ProtectedData(_players.Select(player => player.PlayerId).ToList(), roomId/*, gridCards, MAX_WIDTH*/);
        }

        public void SetHostCards(List<Card> allCards)
        {
            byte[] gridCards = InitCardsData(allCards);
            protectedData.SetGridCards(gridCards, (byte)MAX_WIDTH);
        }

        public void SetClientCards(List<Card> allCards)
        {
            // forming static Id to Card dictionary
            if (CardsDictionary == null || CardsDictionary.Count == 0)
                CardsDictionary = allCards.ToDictionary(card => card.id);
        }
        
        private byte[] InitCardsData(List<Card> allCards)
        {
            List<Card> newCards = new List<Card>(allCards.Take(MAX_WIDTH * MAX_HEIGHT));
            newCards.Shuffle();
            // forming Deck
            PoolOfCards = new Stack<Card>(newCards);
            // forming static Id to Card dictionary
            if (CardsDictionary == null || CardsDictionary.Count == 0)
                CardsDictionary = newCards.ToDictionary(card => card.id);

            List<Card> tempDeck = newCards;

            byte[] result = new byte[MAX_WIDTH * MAX_HEIGHT];
            for (int i = 0; i < MAX_WIDTH * MAX_HEIGHT; i++)
            {
                int index = UnityEngine.Random.Range(0, tempDeck.Count);
                result[i] = tempDeck[index].id;
                tempDeck.RemoveAt(index);
            }
            return result;
        }

        public void Shuffle(byte width)
        {
            byte[] gridCards = InitCardsData(CardsData.Cards);
            protectedData.SetGridCards(gridCards, width);
        }

        public Card[,] GetGridCards()
        {
            byte[,] grid = protectedData.GetGridCards();
            Card[,] result = new Card[grid.GetLength(0), grid.GetLength(1)];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    result[i,j] = CardsDictionary[grid[i, j]];
                }
            }
            return result;
        }

        public void SetGridCards(Card[,] cards)
        {
            byte[] result = new byte[cards.Length];
            for (int i = 0; i < cards.GetLength(0); i++)
            {
                for (int j = 0; j < cards.GetLength(1); j++)
                {
                    result[i * cards.GetLength(1) + j] = cards[i, j].id;
                }
            }
            protectedData.SetGridCards(result, (byte)cards.GetLength(1));
        }

        public void DealRoleToPlayer(Player player)
        {
            player.ReceiveRoleCard(PoolOfCards.Pop());
            protectedData.SetPlayerRole(player.PlayerId, player.Card.id);
        }
        
        public byte PlayerFrags(Player player)
        {
            return protectedData.PlayerFrags(player);
        }

        public void AddPlayerFrag(Player player, Card fragCard)
        {
            player.ReceiveFragCard(fragCard);
            protectedData.AddPlayerFrag(player);
        }

        public void AddDeadId(byte id)
        {
            DeadIds.Add(id);
        }

        public void UpdatePlayerRoleAndFrags()
        {
            foreach (Player player in Players)
            {
                player.ReceiveRoleCard(CardsDictionary[protectedData.GetPlayerRole(player.PlayerId)]);
                while (player.NumberOfFrags < protectedData.PlayerFrags(player))
                {
                    player.ReceiveFragCard(player.Card);    // FIXME update with actual frag card
                }
            }
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
            //Debug.Log("GetCurrentTurnPlayer " + string.Join("::", Players.Select(p => p.PlayerId)) + ", id! " + protectedData.GetCurrentTurnPlayerId());
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
