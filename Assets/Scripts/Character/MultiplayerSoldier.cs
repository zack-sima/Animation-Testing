using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(SoldierController)), RequireComponent(typeof(SoldierAnimator))]
public class MultiplayerSoldier : NetworkBehaviour {
	[SerializeField]
	private GameObject mainCamera;

	[SyncVar]
	public bool running, aiming;
	[SyncVar]
	public float walkAnimationSpeed, runAnimationSpeed;

	private SoldierAnimator animator;

	private void Start() {
		animator = GetComponent<SoldierAnimator>();
		if (isLocalPlayer) {
			//everything else by default on
			mainCamera.SetActive(true);
			GetComponent<SoldierController>().StartMultiplayer(this);
		} else {
			GetComponent<SoldierController>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Collider>().isTrigger = true;

			mainCamera.SetActive(false);
		}
	}

	[Command]
	public void SetRunning(bool running) {
		this.running = running;
	}
	[Command]
	public void SetAiming(bool aiming) {
		this.aiming = aiming;
	}
	[Command]
	public void SetWalkAnimationSpeed(float walkAnimationSpeed) {
		this.walkAnimationSpeed = walkAnimationSpeed;
	}
	[Command]
	public void SetRunAnimationSpeed(float runAnimationSpeed) {
		this.runAnimationSpeed = runAnimationSpeed;
	}

	[Command]
	public void ShootBullet() {
		//rewind for lag compensation and check raycasting here

		//client animation
		ClientShootBullet();
	}
	[ClientRpc]
	public void ClientShootBullet() {
		animator.ShootBullet();
	}

	[Command]
	public void Reload() {
		ClientReload();
	}
	[ClientRpc] //called on all clients to show animation
	public void ClientReload() {
		animator.Reload();
	}
}