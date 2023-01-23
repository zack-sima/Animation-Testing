using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRagdoll : MonoBehaviour {
	public GameObject cameraHold;
	public Transform stomach;
	void Start() {
		Destroy(gameObject, 5);
		transform.Rotate(Random.Range(-15, -10), Random.Range(-15, 15), Random.Range(-15, 15));
	}

	void Update() {

	}
}
