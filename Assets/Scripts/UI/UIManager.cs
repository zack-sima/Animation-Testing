using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
	[SerializeField]
	private Image healthDisplay;
	[SerializeField]
	private TMP_Text ammoText;
	public static UIManager instance;

	public void UpdateHealthDisplay(float healthPercent) {
		healthDisplay.color = new Color(1, 1, 1, 1 - healthPercent);
	}
	public void UpdateAmmoDisplay(int mag, int total) {
		ammoText.text = mag + "/" + total;
	}

	void Start() {
		instance = this;
	}

	void Update() { }
}
