using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MultiplayerManager : NetworkManager {
	public static MultiplayerManager instance;
	public GameObject placeholderCamera;

	[HideInInspector]
	public List<MultiplayerSoldier> players;

	new private void Start() {
		base.Start();
		players = new List<MultiplayerSoldier>();
		instance = this;

#if UNITY_STANDALONE_LINUX
		StartServer();
#endif
	}
	new private void Update() {
		base.Update();
	}

	//all clients
	public override void OnStartClient() {
		base.OnStartClient();
		placeholderCamera.SetActive(false);
	}
	public override void OnStopClient() {
		base.OnStopClient();
		placeholderCamera.SetActive(true);
	}

	public override void OnStartServer() {
		base.OnStartServer();
	}

	//spawn
	public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
		// add player at correct spawn position
		GameObject player = Instantiate(playerPrefab);
		MultiplayerSoldier multiplayerClient = player.GetComponent<MultiplayerSoldier>();

		//note: make sure the object exists when accessing
		players.Add(player.GetComponent<MultiplayerSoldier>());
		NetworkServer.AddPlayerForConnection(conn, player);
	}
	public override void OnServerDisconnect(NetworkConnectionToClient conn) {
		//clear out leaderboards & player list
		int i = 0;
		while (i < players.Count) {
			if (players[i] == null || players[i].connectionToClient == conn) {
				//remove player
				players.RemoveAt(i);
			} else {
				i++;
			}
			print("removed player");
		}

		base.OnServerDisconnect(conn); //server handles destruction of player

		MultiplayerSoldier.playerInstance.ServerUpdateStats();
	}

}
