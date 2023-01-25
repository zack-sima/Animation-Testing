using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
	[SerializeField]
	private Image healthDisplay;
	[SerializeField]
	private TMP_Text ammoText, leaderboardsText;
	[SerializeField]
	private Slider sensitivitySlider;
	[SerializeField]
	private GameObject pauseUI, leaderboardsUI;
	[SerializeField]
	private TMP_InputField nameInput;

	public static bool paused;
	public static float sensitivity, fov;

	public static UIManager instance;

	public void UpdateLeaderboards(string text) {
		leaderboardsText.text = text;
	}
	public static string GenerateLeaderboardsText(MultiplayerManager multiplayerControl) {
		string t = "K / D\n\n";
		foreach (MultiplayerSoldier s in multiplayerControl.players) {
			t += s.playerName + ": " + s.kills + " / " + s.deaths + "\n";
		}
		return t;
	}
	public void UpdateHealthDisplay(float healthPercent) {
		healthDisplay.color = new Color(1, 1, 1, 1 - healthPercent);
	}
	public void UpdateAmmoDisplay(int mag, int total) {
		ammoText.text = mag + "/" + total;
	}
	public void TogglePause() {
		paused = !paused;
		pauseUI.SetActive(paused);
		if (paused) {
			Cursor.lockState = CursorLockMode.None;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
	//todo: multiplayer only, change to menu later (?) [implement bots with multiplayer(?)]
	public void OnNameInputChanged() {
		string s = nameInput.text;
		if (s == "") s = "Player";

		MultiplayerSoldier.playerInstance.SetName(s);
	}

	void Start() {
		instance = this;

		sensitivity = 5;
	}

	void Update() {
		sensitivity = sensitivitySlider.value;

		if (Input.GetKeyDown(KeyCode.Escape)) {
			TogglePause();
		}
		if (Input.GetKey(KeyCode.Tab)) {
			leaderboardsUI.SetActive(true);
		} else {
			leaderboardsUI.SetActive(false);
		}
	}
}
