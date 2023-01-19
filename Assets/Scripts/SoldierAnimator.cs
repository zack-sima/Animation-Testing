using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//animation controller
public class SoldierAnimator : MonoBehaviour {
	//object assignment in editor
	[SerializeField]
	private Animator animator;
	[SerializeField]
	private Transform stomach;

	//edited by controllers
	public float walkAnimationSpeed, runAnimatonSpeed;
	public bool running, aiming;
	public float stomachRotation; //x rotation for aiming degrees

	void Start() { }

	void Update() {
		//running animation
		float animSpeed = walkAnimationSpeed;
		if (running) {
			animSpeed = runAnimatonSpeed;
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
		stomach.localRotation = Quaternion.Euler(stomachRotation, stomach.localEulerAngles.y, 0);
	}
}
