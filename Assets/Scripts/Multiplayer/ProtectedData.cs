using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWNetwork;

namespace Pachik
{
    //public interface IProtectedData
    //{
    //    void SetGridCards(byte[] cardValues);
    //    byte[] GetGridCards();
    //}

    /// <summary>
    /// Stores the important data of the game
    /// We will encypt the fields in a multiplayer game.
    /// </summary>
    [Serializable]
    public class ProtectedData
    {
        const char DELIMETER = '|';

        byte[] gridCards = new byte[25];
        List<string> playerIds = new List<string>();
        List<byte> playersRoleCard = new List<byte>();
        List<byte> playersFragCount = new List<byte>();
        string currentTurnPlayerId;
        byte currentGameState;


        byte[] encryptionKey;
        byte[] safeData;

        public ProtectedData(List<string> players, string roomId)
        {
            playerIds = players;
            currentTurnPlayerId = "";
            CalculateKey(roomId);
            Encrypt();
        }

        public void SetGridCards(byte[] cardValues)
        {

            Decrypt();
            gridCards = cardValues;
            Encrypt();
        }

        public byte[] GetGridCards()
        {
            byte[] result;
            Decrypt();
            result = gridCards;
            Encrypt();
            return result;
        }


        public byte PlayerFrags(Player player)
        {
            Decrypt();
            int index = playerIds.FindIndex(id => id == player.PlayerId);
            byte result = playersFragCount[index];
            Encrypt();
            return result;
        }

        public void AddPlayerFrag(Player player)
        {
            Decrypt();
            int index = playerIds.FindIndex(id => id == player.PlayerId);
            playersFragCount[index] = (byte)(playersFragCount[index] + 1);
            Encrypt();
        }

        public void SetPlayerRole(string playerId, byte role)
        {
            Decrypt();
            int index = playerIds.FindIndex(id => id == playerId);
            playersRoleCard[index] = role;
            Encrypt();
        }

        public byte GetPlayerRole(string playerId)
        {
            Decrypt();
            int index = playerIds.FindIndex(id => id == playerId);
            byte result = playersRoleCard[index];
            Encrypt();
            return result;
        }

        public bool GameFinished()
        {
            Decrypt();
            bool result = playersFragCount.Contains(4);
            Encrypt();
            return result;
        }

        public string WinnerPlayerId()
        {
            string result;
            Decrypt();
            int index = playersFragCount.FindIndex(c => c > 3);
            result = playerIds[index];
            Encrypt();
            return result;
        }

        public void SetCurrentTurnPlayerId(string playerId)
        {
            Decrypt();
            currentTurnPlayerId = playerId;
            Encrypt();
        }

        public string GetCurrentTurnPlayerId()
        {
            string result;
            Decrypt();
            result = currentTurnPlayerId;
            Encrypt();
            return result;
        }

        public void SetGameState(byte gameState)
        {
            Decrypt();
            currentGameState = gameState;
            Encrypt();
        }
        public int GetGameState()
        {
            int result;
            Decrypt();
            result = currentGameState;
            Encrypt();
            return result;
        }


        public Byte[] ToArray()
        {
            return safeData;
        }

        public void ApplyByteArray(Byte[] byteArray)
        {
            safeData = byteArray;
        }

        void CalculateKey(string roomId)
        {
            string roomIdSubString = roomId.Substring(0, 16);
            encryptionKey = Encoding.UTF8.GetBytes(roomIdSubString);
        }

        void Encrypt()
        {
            SWNetworkMessage message = new SWNetworkMessage();
            message.Push((Byte)gridCards.Length);
            message.PushByteArray(gridCards);

            message.PushUTF8LongString(String.Concat(playerIds, DELIMETER));

            message.PushByteArray(playersFragCount.ToArray());

            message.PushByteArray(playersRoleCard.ToArray());

            message.PushUTF8ShortString(currentTurnPlayerId);
            message.Push(currentGameState);


            safeData = AES.EncryptAES128(message.ToArray(), encryptionKey);


            gridCards = new byte[25];
            playerIds = new List<string>();
            playersFragCount = new List<byte>();
            playersRoleCard = new List<byte>();
            currentTurnPlayerId = null;
            currentGameState = 0;

        }

        void Decrypt()
        {
            byte[] byteArray = AES.DecryptAES128(safeData, encryptionKey);

            SWNetworkMessage message = new SWNetworkMessage(byteArray);
            byte cardsCount = message.PopByte();
            gridCards = message.PopByteArray(cardsCount);

            playerIds = message.PopUTF8LongString().Split(DELIMETER).ToList();

            byte playersCount = (byte)playerIds.Count;

            playersFragCount = message.PopByteArray(playersCount).ToList();

            playersRoleCard = message.PopByteArray(playersCount).ToList();

            currentTurnPlayerId = message.PopUTF8ShortString();
            currentGameState = message.PopByte();
        }
    }
}