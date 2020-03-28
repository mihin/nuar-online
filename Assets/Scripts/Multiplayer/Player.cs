using System;
using System.Collections.Generic;
using SWNetwork;
using UnityEngine;

namespace Pachik
{
    /// <summary>
    /// Manages the positions of the player's cards
    /// </summary>
    [Serializable]
    public class Player : IEquatable<Player>
    {

        public static Player fromNetworkPlayer(SWPlayer nwPlayer)
        {
            return new Player(nwPlayer.id, nwPlayer.GetCustomDataString());
        }

        public string PlayerId { get; }
        public string PlayerName { get; }
        public Card Card { get; private set; }        
        private Vector2 Position;
        // public bool IsAI;
        public bool IsLocal;

        
        private List<Card> Frags = new List<Card>();
        public int NumberOfFrags
        {
            get { return Frags.Count; }
        }


        private Player(string id, string name)
        {
            PlayerId = id;
            PlayerName = name;
        }
        public Player(string id, string name, Vector2 position) : this(id, name)
        {
            Position = position;
        }

        public Player SetPosition(Vector2 position)
        {
            Position = position;
            return this;
        }

        public void ReceiveRoleCard(Card card)
        {
            Card = card;
        }
        
        public void ReceiveFragCard(Card card)
        {
            Frags.Add(Card);
        }

        public void ClearFrags()
        {
            Frags.Clear();
        }

        public bool Equals(Player other)
        {
            return other != null && PlayerId.Equals(other.PlayerId);
        }
        
        private Vector2 NextFragCardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * (GoFish.Constants.PLAYER_CARD_POSITION_OFFSET * NumberOfFrags);
            return nextPos;
        }
    }
}
