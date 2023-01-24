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

	//all clients
	public override void OnStartClient() {
		base.OnStartClient();
		placeholderCamera.SetActive(false);
	}
	public override void OnStopClient() {
		base.OnStopClient();
		placeholderCamera.SetActive(true);
	}

	//spawn
	public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
		// add player at correct spawn position
		GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		MultiplayerSoldier multiplayerClient = player.GetComponent<MultiplayerSoldier>();
		SoldierAnimator animator = player.GetComponent<SoldierAnimator>();

		//note: make sure the object exists when accessing
		players.Add(player.GetComponent<MultiplayerSoldier>());
		NetworkServer.AddPlayerForConnection(conn, player);
	}
	public override void OnServerDisconnect(NetworkConnectionToClient conn) {
		base.OnServerDisconnect(conn); //server handels destruction of player

		//clear out any destroyed player scripts
		int i = 0;
		while (i < players.Count) {
			if (players[i] == null) {
				players.RemoveAt(i);
				print("removed player");
			} else {
				i++;
			}
		}
	}

}
