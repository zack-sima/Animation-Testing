using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MultiplayerManager : NetworkManager {
	public static MultiplayerManager instance;
	public GameObject placeholderCamera;

	new private void Start() {
		base.Start();
	}

	//all clients
	public override void OnStartClient() {
		placeholderCamera.SetActive(false);
	}
	public override void OnStopClient() {
		placeholderCamera.SetActive(true);
	}


}
