using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//animation controller
public class SoldierAnimator : MonoBehaviour {
	//object assignment in editor
	[SerializeField]
	private Animator animator;
	[SerializeField]
	private Transform stomach, cameraHold; //change cameraHold position for recoil effect
	public Gun gun; //the gun currently in use

	//edited by controllers
	[HideInInspector]
	public float walkAnimationSpeed, runAnimatonSpeed;
	[HideInInspector]
	public bool running, aiming;
	[HideInInspector]
	public float stomachRotation; //x rotation for aiming degrees
	[HideInInspector]
	public float shootTimer = 0;

	private float currentRecoil = 0;

	private void Start() {
		walkAnimationSpeed = 0;
		runAnimatonSpeed = 0;
	}

	private void Update() {
		//bullet timer
		if (shootTimer > 0) {
			shootTimer -= Time.deltaTime;
		}

		//recoil animator
		if (currentRecoil > 0) {
			if (!addingRecoil) currentRecoil -= Time.deltaTime * 7f;
			if (currentRecoil < 0) currentRecoil = 0;
			cameraHold.localPosition = new Vector3(0, 0, currentRecoil / 30f);
		}

		//running animation
		if (running && !reloading) {
			animator.SetBool("Running", true);
		} else {
			animator.SetBool("Running", false);
		}
		animator.SetFloat("WalkSpeed", walkAnimationSpeed);

		//aiming animation
		if (aiming) {
			animator.SetBool("Aiming", true);
		} else {
			animator.SetBool("Aiming", false);
		}

		//gun/camera vertical rotation (y)
		stomach.localRotation = Quaternion.Euler(stomachRotation - currentRecoil * 1.2f, stomach.localEulerAngles.y, 0);
		//counterbalance the stomach rotation
		cameraHold.localRotation = Quaternion.Euler(currentRecoil * 1.2f, 0, 0);
	}
	public void ShootBullet() {
		if (shootTimer <= 0) {
			//add timer instead of setting it directly to compensate for deltaTime
			shootTimer += gun.GetShootSpeed();
			StartCoroutine(DisplayMuzzle());
			StartCoroutine(AddRecoil(gun.GetRecoil()));
			gun.gunSound.PlayOneShot(gun.gunSound.clip);
		}
	}
	//reload animations
	private bool reloading = false;
	public bool GetReloading() {
		return reloading;
	}
	public void Reload(bool soundOn = true) {
		if (!reloading) {
			StartCoroutine(ReloadTimeout(soundOn: soundOn));
		}
	}
	private IEnumerator ReloadTimeout(bool soundOn = true) {
		reloading = true;

		//if running or aiming reload after wait
		if (running) {
			for (float t = 0; t < 0.18; t += Time.deltaTime) yield return null;
		} else if (aiming) {
			for (float t = 0; t < 0.2; t += Time.deltaTime) yield return null;
		}

		animator.SetBool("Reloading", true);

		//wait a little before sound
		for (float t = 0; t < 0.1; t += Time.deltaTime) yield return null;

		if (soundOn) gun.reloadSound.PlayOneShot(gun.reloadSound.clip);

		for (float t = 0; t < 2.5; t += Time.deltaTime) yield return null;

		reloading = false;
		animator.SetBool("Reloading", false);
	}
	//recoil is cumulative with a certain cap
	private bool addingRecoil = false;
	private IEnumerator AddRecoil(float r) {
		addingRecoil = true; //prevents recoil from being decreased while adding
		for (float t = 0; t < 0.05; t += Time.deltaTime) {
			currentRecoil += r * Time.deltaTime * 20;
			if (currentRecoil > gun.GetMaxRecoil()) //
				currentRecoil = gun.GetMaxRecoil();
			yield return null;
		}
		addingRecoil = false;
	}
	private IEnumerator DisplayMuzzle() {
		//displays the muzzle flash for 0.05 seconds
		for (float t = 0; t < 0.05f; t += Time.deltaTime) {
			gun.muzzleRenderer.enabled = true;
			yield return null;
		}
		gun.muzzleRenderer.enabled = false;
	}
}
