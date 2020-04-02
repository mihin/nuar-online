using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SWNetwork;

namespace Pachik
{
    public class Lobby : MonoBehaviour
    {
        public int MAX_PLAYERS = 6;
        
        public enum LobbyState
        {
            Default,
            JoinedRoom,
            RoomsList,
        }
        public LobbyState State = LobbyState.Default;
        public bool Debugging = false;
        public bool QuickMatchMode = false;
        private bool hostGame = false;

        public GameObject PopoverBackground;
        // public GameObject EnterNicknamePopover;
        public GameObject WaitForOpponentPopover;
        public GameObject RoomsListPopover;
        public GameObject RoomsListContent;
        public GameObject RoomsListItemPrefab;
        public GameObject StartRoomButton;
        public InputField NicknameInputField;

        // public GameObject Player1Portrait;
        // public GameObject Player2Portrait;

        string nickname;

        private void Start()
        {
            // disable all online UI elements
            HideAllPopover();
            NetworkClient.Lobby.OnLobbyConnectedEvent += OnLobbyConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent += OnRoomReadyEvent;
        }

        private void OnDestroy()
        {
            if (NetworkClient.Lobby != null)
            {
                NetworkClient.Lobby.OnLobbyConnectedEvent -= OnLobbyConnected;
                NetworkClient.Lobby.OnNewPlayerJoinRoomEvent -= OnNewPlayerJoinRoomEvent;
            }
        }

        void ShowEnterNicknamePopover()
        {
            PopoverBackground.SetActive(true);
        }

        void ShowJoinedRoomPopover()
        {
            PopoverBackground.SetActive(true);
            WaitForOpponentPopover.SetActive(true);
            StartRoomButton.SetActive(false);
        }
        
        void ShowRoomsListPopover()
        {
            PopoverBackground.SetActive(true);
            RoomsListPopover.SetActive(true);
            WaitForOpponentPopover.SetActive(false);
            StartRoomButton.SetActive(false);
        }

        void ShowReadyToStartUI()
        {
            StartRoomButton.SetActive(true);
        }

        void HideAllPopover()
        {
            PopoverBackground.SetActive(false);
            WaitForOpponentPopover.SetActive(false);
            StartRoomButton.SetActive(false);
        }

		//****************** Matchmaking *********************//
        void Checkin()
		{
			NetworkClient.Instance.CheckIn(nickname, (bool successful, string error) =>
			{
				if (!successful)
				{
					Debug.LogError(error);
				}
			});
		}

        void RegisterToTheLobbyServer()
        {
            if (hostGame)
                CreateRoom();
            else
                DisplayRooms();

            // NetworkClient.Lobby.Register(nickname, (successful, reply, error) => {
            //     if (successful)
            //     {
            //         Debug.Log("Lobby registered " + reply);
            //         if (string.IsNullOrEmpty(reply.roomId))
            //         {
            //             if (QuickMatchMode) 
            //                 JoinOrCreateRoom();
            //             else
            //             {
            //                 if (hostGame)
            //                     CreateRoom();
            //                 else
            //                     DisplayRooms();
            //             }
            //         }
            //         else if (reply.started)
            //         {
            //             State = LobbyState.JoinedRoom;
            //             ConnectToRoom();
            //         }
            //         else
            //         {
            //             State = LobbyState.JoinedRoom;
            //             ShowJoinedRoomPopover();
            //             GetPlayersInTheRoom();
            //         }
            //     }
            //     else
            //     {
            //         Debug.Log("Lobby failed to register " + reply);
            //     }
            // });
        }

        void JoinOrCreateRoom()
        {
            NetworkClient.Lobby.JoinOrCreateRoom(false, MAX_PLAYERS, 60, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Joined or created room " + reply);
                    State = LobbyState.JoinedRoom;
                    ShowJoinedRoomPopover();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to join or create room " + error);
                }
            });
        }
        
        void CreateRoom()
        {
            NetworkClient.Lobby.CreateRoom(nickname, false, MAX_PLAYERS, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Created room " + reply);
                    State = LobbyState.JoinedRoom;
                    ShowJoinedRoomPopover();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to create room " + error);
                }
            });
        }
        
        void DisplayRooms()
        {
            NetworkClient.Lobby.GetRooms(0, 15, (successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Created room " + reply);
                    State = LobbyState.RoomsList;
                    ShowRoomsListPopover();
                }
                else
                {
                    Debug.Log("Failed to create room " + error);
                }
            });
        }

        void GetPlayersInTheRoom()
        {
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Got players " + reply);
                    if(reply.players.Count == 1)
                    {
                        // Player1Portrait.SetActive(true);
                    }
                    else
                    {
                        // Player1Portrait.SetActive(true);
                        // Player2Portrait.SetActive(true);

                        if (NetworkClient.Lobby.IsOwner)
                        {
                            ShowReadyToStartUI();
                        }
                    }
                }
                else
                {
                    Debug.Log("Failed to get players " + error);
                }
            });
        }

        void LeaveRoom()
        {
            NetworkClient.Lobby.LeaveRoom((successful, error) => {
                if (successful)
                {
                    Debug.Log("Left room");
                    State = LobbyState.Default;
                }
                else
                {
                    Debug.Log("Failed to leave room " + error);
                }
            });
        }

        void StartRoom()
        {
            NetworkClient.Lobby.StartRoom((successful, error) => {
                if (successful)
                {
                    Debug.Log("Started room.");
                }
                else
                {
                    Debug.Log("Failed to start room " + error);
                }
            });
        }

        void ConnectToRoom()
        {
            // connect to the game server of the room.
            NetworkClient.Instance.ConnectToRoom((connected) =>
            {
                if (connected)
                {
                    SceneManager.LoadScene("NuarMultiplayer");
                }
                else
                {
                    Debug.Log("Failed to connect to the game server.");
                }
            });
        }

        //****************** Lobby events *********************//
        void OnLobbyConnected()
		{
            RegisterToTheLobbyServer();
		}

        void OnNewPlayerJoinRoomEvent(SWJoinRoomEventData eventData)
        {
            if (NetworkClient.Lobby.IsOwner)
            {
                ShowReadyToStartUI();
            }
        }

        void OnRoomReadyEvent(SWRoomReadyEventData eventData)
        {
            ConnectToRoom();
        }

        //****************** UI event handlers *********************//
        /// <summary>
        /// Practice button was clicked.
        /// </summary>
        public void OnPracticeClicked()
        {
            Debug.Log("OnPracticeClicked");
            SceneManager.LoadScene("Nuar");
        }

        /// <summary>
        /// Online button was clicked.
        /// </summary>
        public void OnOnlineClicked()
        {
            Debug.Log("OnOnlineClicked");
            ShowEnterNicknamePopover();
        }

        /// <summary>
        /// Cancel button in the popover was clicked.
        /// </summary>
        public void OnCancelClicked()
        {
            Debug.Log("OnCancelClicked");

            if (State == LobbyState.JoinedRoom)
            {
                // TODO: leave room.
                LeaveRoom();
            }

            HideAllPopover();
        }

        /// <summary>
        /// Start button in the WaitForOpponentPopover was clicked.
        /// </summary>
        public void OnStartRoomClicked()
        {
            Debug.Log("OnStartRoomClicked");
            // players are ready to player now.
            if (Debugging)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                // Start room
                StartRoom();
            }
        }

        /// <summary>
        /// Ok button in the EnterNicknamePopover was clicked.
        /// </summary>
        public void OnConfirmNicknameClicked()
        {
            nickname = NicknameInputField.text;
            Debug.Log($"OnConfirmNicknameClicked: {nickname}");

            if (Debugging)
            {
                ShowJoinedRoomPopover();
                ShowReadyToStartUI();
            }
            else
            {
				//Use nickname as player custom id to check into SocketWeaver.
				Checkin();
            }
        }

        public void OnHostGameClicked()
        {
            nickname = NicknameInputField.text;
            Debug.Log($"OnHostGameClicked, nickname: {nickname}");

            hostGame = true;
            Checkin();
        }
        
        public void OnFindGameClicked()
        {
            nickname = NicknameInputField.text;
            Debug.Log($"OnFindGameClicked, nickname: {nickname}");

            hostGame = false;
            Checkin();
        }
    }
}
