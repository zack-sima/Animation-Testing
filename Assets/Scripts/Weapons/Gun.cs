using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
	public List<Sprite> muzzleSprites;
	public SpriteRenderer muzzleRenderer;
	public AudioSource gunSound, reloadSound;
	public Transform raycastAnchor;

	//constants
	[SerializeField]
	private string gunId;
	[SerializeField]
	private bool fullAuto;
	[SerializeField]
	private int maxBullets, magSize;
	[SerializeField]
	private float shootSpeed, recoil, maxRecoil;
	[SerializeField]
	private int damage;

	[HideInInspector]
	public int totalBullets, magBullets; //current ammo

	private void Start() {
		totalBullets = maxBullets;
		magBullets = magSize;
	}

	public int GetDamage() {
		return damage;
	}
	public string GetGunId() {
		return gunId;
	}
	public int GetMaxBullets() {
		return maxBullets;
	}
	public int GetMagSize() {
		return magSize;
	}
	public float GetShootSpeed() {
		return shootSpeed;
	}
	public float GetRecoil() {
		return recoil;
	}
	public float GetMaxRecoil() {
		return maxRecoil;
	}
	public bool GetFullAuto() {
		return fullAuto;
	}
}
