using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SwimmerPlayerController : MonoBehaviour {

	public float acceleration = 10.0f;
	public float maxSpeed = 10.0f;
	public float turnRate = 120.0f;

	private Transform cameraTransform;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
	
		cameraTransform = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate() {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		float rising = Input.GetButton("Jump") ? -1.0f : 1.0f;

		if (rb.position.y >= 0.0f && rising > 0.0f)
		{
			rising = -rising;
		}
		Quaternion cameraRotation = cameraTransform.rotation;

		Vector3 targetDirection = 
			cameraRotation * Vector3.right * horizontal
			+ cameraRotation * Vector3.forward * vertical
			+ cameraRotation * Vector3.up * rising
			;

		rb.AddForce(targetDirection * acceleration, 
			ForceMode.Acceleration);


		// limit speed
		if (rb.velocity.magnitude > maxSpeed)
		{
			rb.velocity = rb.velocity.normalized * maxSpeed;
		}

		// rotation
		if (targetDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
			rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 2.0f * Time.deltaTime));
//			rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.deltaTime));
		}
	}

	public void ResetPosition()
	{
		transform.position = Vector3.zero;
	}
}
