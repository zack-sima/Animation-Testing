using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(SoldierController)), RequireComponent(typeof(SoldierAnimator))]
public class MultiplayerSoldier : NetworkBehaviour {
	public static MultiplayerSoldier playerInstance;

	[SerializeField]
	private GameObject mainCamera;
	[SyncVar, HideInInspector]
	public bool running, aiming, dying;
	[SyncVar, HideInInspector]
	public float walkAnimationSpeed, runAnimationSpeed;
	[SyncVar]
	public double health = 100;
	[SyncVar]
	public int kills, deaths;
	[SyncVar]
	public string playerName;

	public readonly double maxHealth = 100;

	private bool initialized;

	private List<KeyValuePair<double, Vector3>> deltaPositions; //for lag compensation, remember the past 30 frames (~500ms)

	private SoldierAnimator animator;
	private SoldierController controller;

	[Command] //initializes health, etc
	public void ClientInitialization() {
		this.health = maxHealth;
		//clients auto-initialize once health is not 0
		this.initialized = true;
	}
	private void Start() {
		Cursor.lockState = CursorLockMode.Locked;

		deltaPositions = new List<KeyValuePair<double, Vector3>>();
		animator = GetComponent<SoldierAnimator>();
		controller = GetComponent<SoldierController>();

		if (isLocalPlayer) {
			SetName("Player");

			ChooseSpawnpoint();

			animator.isPlayer = true;
			playerInstance = this;
			animator.ResetAmmo();
			UIManager.instance.UpdateAmmoDisplay(animator.gun.magBullets, animator.gun.totalBullets);

			mainCamera.SetActive(true);
			controller.StartMultiplayer(this);

			ClientInitialization();
		} else {
			animator.cameraHold.gameObject.SetActive(false);
			animator.isPlayer = false;

			GetComponent<SoldierController>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<Collider>().isTrigger = true;

			mainCamera.SetActive(false);
		}
	}
	private void Update() {
		if (!initialized) {
			if (health > 0 && isClient) {
				initialized = true;
			} else return;
		}

		deltaPositions.Add(new KeyValuePair<double, Vector3>(NetworkTime.time, transform.position));
		if (deltaPositions.Count > 30) deltaPositions.RemoveAt(0);

		if (!isLocalPlayer) {
			//sync animations
			animator.running = running;
			animator.aiming = aiming;
			animator.walkAnimationSpeed = walkAnimationSpeed;
			animator.runAnimatonSpeed = runAnimationSpeed;
		} else {
			//health display
			UIManager.instance.UpdateHealthDisplay((float)(health / maxHealth));

			//check health for death animation
			if (health <= 0 && !dying) {
				print("no health");
				KillPlayer();
			}
			//todo: temporary suicide key for testing
			if (Input.GetKeyDown(KeyCode.K) && !dying && !UIManager.paused) {
				KillPlayer();
			}
		}
		if (isServer) {
			//healing
			if (health < maxHealth) {
				double newHealth = health + Time.deltaTime * 5;
				if (newHealth > maxHealth) newHealth = maxHealth;
				health = newHealth;
			}
		}
	}
	[Server]
	public void ServerUpdateStats() {
		RpcUpdateStats(UIManager.GenerateLeaderboardsText(MultiplayerManager.instance));
	}
	[ClientRpc] //called by server to transmit data
	public void RpcUpdateStats(string statsText) {
		UIManager.instance.UpdateLeaderboards(statsText);
	}

	[Command]
	private void KillPlayer() {
		if (!dying) {
			dying = true;
			health = 0;
			deaths++;
			ServerUpdateStats();
			RpcKillPlayer();
			StartCoroutine(RespawnPlayer(5));
		}
	}
	[ClientRpc]
	private void RpcKillPlayer() {
		animator.Die();
	}
	[Command]
	private void ChooseSpawnpoint() {
		RpcRespawnPlayer(FindSpawnpoint());
	}
	[Server]
	private IEnumerator RespawnPlayer(float time) {
		for (float t = 0; t < time; t += Time.deltaTime) {
			yield return null;
		}
		RpcRespawnPlayer(FindSpawnpoint());
		health = maxHealth;
		dying = false;
	}
	public Vector3 FindSpawnpoint() {
		//find furthest spawnpoint
		List<Transform> spawnpoints = new List<Transform>(GameController.instance.spawnpointParent.
			GetComponentsInChildren<Transform>());

		Transform furthestSpawnpoint = spawnpoints[Random.Range(0, spawnpoints.Count)];

		float furthestDistance = 0;
		if (MultiplayerManager.instance.players.Count > 1) {
			foreach (Transform t in spawnpoints) {
				float d = 10000;
				foreach (MultiplayerSoldier s in MultiplayerManager.instance.players) {
					if (s && s != this && Vector3.Distance(s.transform.position, t.position) < d) {
						d = Vector3.Distance(s.transform.position, t.position);
					}
				}
				if (d > furthestDistance) {
					furthestDistance = d;
					furthestSpawnpoint = t;
				}
			}
		}
		return furthestSpawnpoint.position;
	}
	[ClientRpc]
	public void RpcRespawnPlayer(Vector3 spawnpoint) {
		animator.Respawn(spawnpoint);
		if (isLocalPlayer) {
			UIManager.instance.UpdateAmmoDisplay(animator.gun.magBullets, animator.gun.totalBullets);
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
	public void SetName(string playerName) {
		this.playerName = playerName;
		ServerUpdateStats();
	}
	[Command]
	public void SetHealth(double health) {
		this.health = health;
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
		Quaternion originalRotation = animator.gun.raycastAnchor.rotation;
		if (!aiming) {
			//artificially scatter a little for non-ads shots
			animator.gun.raycastAnchor.Rotate(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
		}
		if (Physics.Raycast(animator.gun.raycastAnchor.position, animator.gun.raycastAnchor.forward, out hit, 200f)) {
			MultiplayerSoldier hitEnemy;
			if (hit.collider.TryGetComponent<MultiplayerSoldier>(out hitEnemy)) {
				//alive and can take damage
				if (!hitEnemy.dying) {
					hitEnemy.health -= damage;
					shotHit = true;
					if (hitEnemy.health <= 0) {
						kills++;
						ServerUpdateStats();
					}
					print(hit.collider.name + " lost " + damage + " health");
				}
			}
		}
		animator.gun.raycastAnchor.rotation = originalRotation;

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