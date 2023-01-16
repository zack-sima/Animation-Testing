using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimator : MonoBehaviour {
	public float walkSpeed, runSpeed, rotateSpeed, jumpForce, walkAnimationSpeed, runAnimatonSpeed;
	public Transform stomach;
	public Animator animator;

	new Rigidbody rigidbody;

	public bool isGrounded;

	private float cameraX = 0;
	private readonly float cameraXBound = 30;

	void Start() {
		Application.targetFrameRate = 60;

		rigidbody = GetComponent<Rigidbody>();
	}

	void Update() {
		isGrounded = Grounded();

		if (Input.GetKeyDown(KeyCode.Escape))
			Cursor.lockState = CursorLockMode.None;

		if (Input.GetMouseButtonDown(0))
			Cursor.lockState = CursorLockMode.Locked;

		//wasd controls
		float horizontalMove = Input.GetAxisRaw("Horizontal");
		float verticalMove = Input.GetAxisRaw("Vertical");

		float speed = walkSpeed;
		float animSpeed = walkAnimationSpeed;

		if (Input.GetKey(KeyCode.LeftShift) && verticalMove > 0) {
			verticalMove *= 2f;
			speed = runSpeed;
			animSpeed = runAnimatonSpeed;
		}

		Vector3 translation = transform.forward * (verticalMove * Time.deltaTime);
		translation += transform.right * (horizontalMove * Time.deltaTime);
		translation *= speed;
		translation = rigidbody.position + translation;

		animator.SetFloat("WalkSpeed", verticalMove);

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		}

		//camera rotation
		float vertical = -Input.GetAxis("Mouse Y") * Time.deltaTime * 1.5f;
		float horizontal = Input.GetAxis("Mouse X") * Time.deltaTime * 1.5f;

		cameraX += vertical * rotateSpeed;
		cameraX = Mathf.Clamp(cameraX, -cameraXBound, cameraXBound);

		stomach.localRotation = Quaternion.Euler(cameraX, stomach.localEulerAngles.y, 0);

		Quaternion rotation = transform.rotation * Quaternion.Euler(0, horizontal * rotateSpeed, 0);

		rigidbody.MovePosition(translation);
		rigidbody.MoveRotation(rotation);
	}

	bool Grounded() {
		return Physics.Raycast(transform.position + Vector3.up * 0.05f, -Vector3.up, 0.1f);
	}
}
