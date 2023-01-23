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
	[SyncVar]
	public double health;

	public readonly double maxHealth = 100;

	private List<KeyValuePair<double, Vector3>> deltaPositions; //for lag compensation, remember the past 30 frames (~500ms)

	private SoldierAnimator animator;
	private SoldierController controller;

	private void Start() {
		deltaPositions = new List<KeyValuePair<double, Vector3>>();
		animator = GetComponent<SoldierAnimator>();
		controller = GetComponent<SoldierController>();

		if (isLocalPlayer) {
			//everything else by default on
			mainCamera.SetActive(true);
			controller.StartMultiplayer(this);
		} else {
			GetComponent<SoldierController>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Collider>().isTrigger = true;

			mainCamera.SetActive(false);
		}
	}
	private void Update() {
		deltaPositions.Add(new KeyValuePair<double, Vector3>(NetworkTime.time, transform.position));
		if (deltaPositions.Count > 30) deltaPositions.RemoveAt(0);

		//sync animations
		if (!isLocalPlayer) {
			animator.running = running;
			animator.aiming = aiming;
			animator.walkAnimationSpeed = walkAnimationSpeed;
			animator.runAnimatonSpeed = runAnimationSpeed;
		}
	}
	private Vector3 tempDeltaPosition = Vector3.zero;
	public void RewindPosition(double lagMS) {
		//iterates from oldest to newest position saved
		for (int i = 0; i < deltaPositions.Count; i++) {
			//last element
			if (i == deltaPositions.Count - 1) {
				print("rewinded " + lagMS + " seconds");
				tempDeltaPosition = transform.position;
				transform.position = deltaPositions[i].Value;
				return;
			}

			//if timestamp is greater than current time - lag, use this first frame
			if (deltaPositions[i].Key > NetworkTime.time - lagMS) {
				print("rewinded " + lagMS + " seconds");
				tempDeltaPosition = transform.position;
				transform.position = deltaPositions[i].Value;
				return;
			}
		}
	}
	//must be called after rewind position!
	public void UnRewindPosition() {
		transform.position = tempDeltaPosition;
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

	//lag can only be calculated on client side
	[Client]
	public void ShootBullet(int damage) {
		double lag = NetworkTime.rtt / 2; //lag between caller and server
		CmdShootBullet(damage, lag);
	}
	[Command]
	private void CmdShootBullet(int damage, double lag) {
		//rewind everyone except self for lag compensation and check raycasting here
		foreach (MultiplayerSoldier s in MultiplayerManager.instance.players) {
			if (s) { //check that script exists (player didn't leave)
				s.RewindPosition(lag); //rewind all players
			}
		}

		bool shotHit = false;

		RaycastHit hit;
		if (Physics.Raycast(animator.gun.raycastAnchor.position, animator.gun.raycastAnchor.forward, out hit, 200f)) {
			MultiplayerSoldier hitEnemy;
			if (hit.collider.TryGetComponent<MultiplayerSoldier>(out hitEnemy)) {
				hitEnemy.health -= damage;
				shotHit = true;
				print(hit.collider.name + " lost " + damage + " health");
			}
		}

		foreach (MultiplayerSoldier s in MultiplayerManager.instance.players) {
			if (s) { //check that script exists (player didn't leave)
				s.UnRewindPosition(); //return players to original positions
			}
		}

		//client animation
		ClientShootBullet(shotHit);
	}
	[ClientRpc]
	public void ClientShootBullet(bool hit) {
		animator.ShootBullet();

		//register hit
		if (hit && isLocalPlayer) {
			print("hit enemy!");
			animator.hitSound.PlayOneShot(animator.hitSound.clip);
		}
	}

	[Command]
	public void Reload() {
		ClientReload();
	}
	[ClientRpc] //called on all clients to show animation
	public void ClientReload() {
		if (!isLocalPlayer) animator.Reload(soundOn: false);
	}
}