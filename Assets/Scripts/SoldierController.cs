using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for local player control of soldier--keyboard/mouse input, interactions with UI, etc
[RequireComponent(typeof(SoldierAnimator)), RequireComponent(typeof(Rigidbody))]
public class SoldierController : MonoBehaviour {
	private readonly float cameraXBound = 30; //gun rotation depth
	private SoldierAnimator animator;
	private new Rigidbody rigidbody;

	//parameters
	[SerializeField]
	private float walkSpeed, runSpeed, rotateSpeed, jumpForce;

	//internal variables
	private bool isGrounded;
	private float cameraX = 0;

	void Start() {
		//default setup
		Application.targetFrameRate = 60;
		rigidbody = GetComponent<Rigidbody>();
		animator = GetComponent<SoldierAnimator>();
	}

	void Update() {
		UIOperations();
		Movements();
		Actions();
	}
	private void UIOperations() {
		//screen lock
		if (Input.GetKeyDown(KeyCode.Escape))
			Cursor.lockState = CursorLockMode.None;
		if (Input.GetMouseButtonDown(0))
			Cursor.lockState = CursorLockMode.Locked;
	}
	//checks whether player's feet are on the ground and can jump
	private bool Grounded() {
		return Physics.Raycast(transform.position + Vector3.up * 0.05f, -Vector3.up, 0.1f);
	}
	//translations & rotations + physics
	private void Movements() {
		//check grounding state
		isGrounded = Grounded();

		//jump
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}

		//wasd controls
		float horizontalMove = Input.GetAxisRaw("Horizontal");
		float verticalMove = Input.GetAxisRaw("Vertical");

		//running (pc)
		float speed = walkSpeed;
		if (Input.GetKey(KeyCode.LeftShift) && verticalMove > 0 && !Input.GetMouseButton(1)) {
			verticalMove = 1.7f;
			speed = runSpeed;
			animator.running = true;
		} else {
			animator.running = false;
		}

		//rigidbody movement
		Vector3 translation = transform.forward * (verticalMove * Time.deltaTime);
		translation += transform.right * (horizontalMove * Time.deltaTime);
		translation *= speed;
		translation = rigidbody.position + translation;

		animator.walkAnimationSpeed = Mathf.Abs(verticalMove + horizontalMove / 2f);
		if (!animator.running && animator.walkAnimationSpeed > 1f) {
			animator.walkAnimationSpeed = 1f; //prevents running animation from walking sideways
		}

		//camera vertical rotation (x)
		float vertical = -Input.GetAxis("Mouse Y") * Time.deltaTime * 1.5f;
		float horizontal = Input.GetAxis("Mouse X") * Time.deltaTime * 1.5f;
		cameraX += vertical * rotateSpeed;
		cameraX = Mathf.Clamp(cameraX, -cameraXBound, cameraXBound);
		animator.stomachRotation = cameraX;

		//body horizontal rotation (y)
		Quaternion rotation = transform.rotation * Quaternion.Euler(0, horizontal * rotateSpeed, 0);
		rigidbody.MovePosition(translation);
		rigidbody.MoveRotation(rotation);
	}
	private void Actions() {
		//aiming
		if (!animator.GetReloading() && Input.GetMouseButton(1)) {
			animator.aiming = true;
		} else {
			animator.aiming = false;
		}

		//reloading
		if (Input.GetKeyDown(KeyCode.R)) {
			animator.Reload();
		}

		//shooting
		if (!animator.running && (Input.GetMouseButton(0) && animator.fullAuto ||
			Input.GetMouseButtonDown(0) && !animator.fullAuto)) {
			animator.AttemptShootBullet();
		}
	}
}