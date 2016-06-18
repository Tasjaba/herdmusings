using UnityEngine;
using System.Collections;

public class ConstantRotator : MonoBehaviour {

	// rotation rate in degrees per second
	public Vector3 rate;


	void FixedUpdate () {
		transform.Rotate(rate * Time.deltaTime);		
	}
}
