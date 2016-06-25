/*
 * 3d version of flocker
 */

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class FlockerFish : MonoBehaviour {

	public float speed = 5.0f;				// movement vars
	public float maxTurnRate = 120.0f;	// maximum rotation rate per second in degrees

	public float cohesionMult   = 0.2f;	// come into the center
	public float separationMult = 1.0f;	// intimate spacing
	public float alignmentMult  = 0.5f;	// conformity
	public float avoidanceMult  = 2.0f;	// run away from

	public float cohesionDist   = 10.0f;
	public float separationDist = 3.0f;
	public float alignmentDist  = 5.0f;
	public float avoidanceDist  = 7.5f;

	public LayerMask cohesionLayerMask;
	public LayerMask separationLayerMask;
	public LayerMask alignmentLayerMask;
	public LayerMask avoidanceLayerMask;


	private LayerMask combinedLayerMask; // combined layer mask of all the layers we care about
	private float maxScanDistance; // maximum of all the distances

//	private float intensity    = 0f;
//	private float coolDownRate = 0.3f;
//	private float tenseUpRate = 0.5f;


	private Rigidbody rb;
	private Vector3 cohesionVector;
	private Vector3 separationVector;
	private Vector3 alignmentVector;
	private Vector3 avoidanceVector;

	private int cohesionCount;
	private int separationCount;
	private int alignmentCount;
	private int avoidanceCount;

	public Vector3 moveDirection;

	// shared temporary scratch buffer for OverlapSphere results
	private static Collider[] scanBuffer = new Collider[256];

	/************************************************************************/

	// move closer to closer objects
	void updateCohesion(Rigidbody other, Vector3 diff, Vector3 dir, float dist) {
		if( dist <= cohesionDist ) {
			cohesionVector += dir*((cohesionDist - dist)/cohesionDist);
			cohesionCount++;
		}
	}

	// move away from closer objects
	void updateSeparation(Rigidbody other, Vector3 diff, Vector3 dir, float dist) {
		if( dist <= separationDist ) {
			separationVector += -dir*((separationDist - dist)/separationDist);
			separationCount++;
		}
	}

	// same as separation (but has a different purpose)
	void updateAvoidance(Rigidbody other, Vector3 diff, Vector3 dir, float dist) {
		if( dist <= avoidanceDist ) {
			avoidanceVector += -dir*((avoidanceDist - dist)/avoidanceDist);
			avoidanceCount++;

//			// cause a scare
//			if(intensity < 1f) {
//				intensity = 1f;
//			}

//			// speed up if need to
//			if(other.velocity.magnitude*0.8f > rb.velocity.magnitude) {
//				intensity += ((avoidanceDist - dist)/avoidanceDist)*tenseUpRate*Time.deltaTime*10f;
//			}

		}
	}

	// align more with closest neighbors
	void updateAlignment(Rigidbody other, Vector3 diff, Vector3 dir, float dist) {
		if( dist <= alignmentDist ) {
			// get forward vector of other game object
			Vector3 otherForwardVector = other.rotation * Vector3.forward;
			alignmentCount++;

			// align more with closer gameobjects
			alignmentVector += otherForwardVector * ((alignmentDist - dist)/alignmentDist);
		}
	}

	//reset all behavior vectors to 0
	void resetBehaviorVectors() {
		cohesionVector = Vector3.zero;
		separationVector = Vector3.zero;
		alignmentVector = Vector3.zero;
		avoidanceVector = Vector3.zero;

		cohesionCount = 0;
		separationCount = 0;
		alignmentCount = 0;
		avoidanceCount = 0;
	}
		
	//normalize the vector results, and use them
	void updateMoveFlag() {

		// vectors for movement decision

		// taking the average of each vector
		Vector3 cohesion   = cohesionCount == 0 ? Vector3.zero : cohesionVector / ((float)cohesionCount) * cohesionMult;
		Vector3 separation = separationCount == 0 ? Vector3.zero : separationVector / ((float)separationCount) * separationMult;
		Vector3 alignment  = alignmentCount == 0 ? Vector3.zero : alignmentVector / ((float) alignmentCount) * alignmentMult;
		Vector3 avoidance  = avoidanceCount == 0 ? Vector3.zero : avoidanceVector / ((float) avoidanceCount) * avoidanceMult;

		// final decision
		moveDirection = (cohesion + separation + alignment + avoidance);

		// need an intensity measure to keep the sheep still

		#if UNITY_EDITOR
//			Debug.DrawRay(transform.position, cohesion, Color.white);
//			Debug.DrawRay(transform.position, separation, Color.black);
//			Debug.DrawRay(transform.position, alignment, Color.blue);
//			Debug.DrawRay(transform.position, avoidance, Color.red);
			Debug.DrawRay(transform.position, moveDirection, Color.red);
		#endif

	}


	/************************************************************************/

	// Use this for initialization
	void Start () {
		
		// inits
		rb = GetComponent<Rigidbody>();

		// bitwise combine the layer masks to create a layer mask of 
		// all the layers we care about
		combinedLayerMask = cohesionLayerMask | separationLayerMask | alignmentLayerMask | avoidanceLayerMask;

		// find the maximum scan distance
		maxScanDistance = Mathf.Max(new float[] { cohesionDist, separationDist, alignmentDist, avoidanceDist });
	}

	void FixedUpdate() {
		// gravity dependent on whether the fish is out of water
		rb.useGravity = rb.position.y > 0.0f;


		// get current orientation
		Quaternion rot = rb.rotation;

		// no flying fish
		if (rb.useGravity)
		{
			moveDirection = Vector3.down;
			rb.rotation = Quaternion.LookRotation(Vector3.down);
		}

		// "steer" in the new direction
		if (moveDirection != Vector3.zero)
		{
			// flatten the movement direction to zero out the y (up/down)
			Vector3 v = moveDirection;
			v.Normalize();

			// here's where we ideally want to steer towards
			Quaternion rotNew = Quaternion.LookRotation(v);

			// limited by how much we can rotate
			rot = Quaternion.RotateTowards(rot, rotNew, maxTurnRate * Time.deltaTime);

			// rotate
			rb.MoveRotation(rot);

			// calculate the new velocity, but use the existing y velocity of the rigidbody
			v = rot * Vector3.forward * speed * Mathf.Clamp01(moveDirection.magnitude);

			rb.velocity = v;
		}
		else
		{
			// TODO: put some code here to make the sheep stop or give some random wandering
			// so that we're not using the drag to slow down
		}
	}
		
	// Update is called once per frame (after fixed update)
	void LateUpdate () {

		// reset behavior vectors
		resetBehaviorVectors();

		// find all the overlapping colliders that we care about
		int resultCount = Physics.OverlapSphereNonAlloc(transform.position, maxScanDistance, scanBuffer, combinedLayerMask);

		for (int i = 0; i < resultCount; i++)
		{
			// assumption: only one collider on other object
			// it is a bit more complex if this is not the case
			Collider other = scanBuffer[i];

			// getting the attached body because the collider may not be on the root of the rigidbody
			// for more complex objects
			Rigidbody otherRigidbody = other.attachedRigidbody;

			// skip myself!
			if (otherRigidbody != null && otherRigidbody.gameObject == gameObject)
				continue;
			
			int otherLayer = other.gameObject.layer;

			// convert to layermask
			int otherLayerMask = 1 << otherLayer;

			// calculate some of these commonly used variables
			Vector3 diff = (otherRigidbody == null ? other.transform.position : otherRigidbody.position) - transform.position;
			Vector3 dir = diff.normalized;
			float dist = diff.magnitude;

			// bitwise comparison
			if ((cohesionLayerMask.value & otherLayerMask) != 0)
			{
				updateCohesion(otherRigidbody, diff, dir, dist);
			}

			if ((separationLayerMask.value & otherLayerMask) != 0)
			{
				updateSeparation(otherRigidbody, diff, dir, dist);
			}

			if ((alignmentLayerMask.value & otherLayerMask) != 0)
			{
				updateAlignment(otherRigidbody, diff, dir, dist);
			}

			if ((avoidanceLayerMask.value & otherLayerMask) != 0)
			{
				updateAvoidance(otherRigidbody, diff, dir, dist);
			}
		}

		// update movement flag
		updateMoveFlag();

		// cool down

//		intensity = Mathf.Lerp(intensity, 0.0f, 0.5f * Time.deltaTime);
//		if(intensity > 0f) { intensity -= coolDownRate * Time.deltaTime; }else{ intensity = 0f; }

	}
}
