using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	public GameObject spawnpointParent;
	public static GameController instance;

	private void Awake() {
		instance = this;
	}
}
