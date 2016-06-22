using UnityEngine;
using System.Collections;

/** follows the path at a given speed,
 * modifying the velocity and direction
 * of the rigidbody
 */
[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPathFollower : PathFollower {

	public float speed = 5.0f;
	public float rotationSlerpRate = 5.0f;

	private int pathIndex = 0;
	private Rigidbody rb;

	void Awake() {
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void FixedUpdate () {

		Vector3 velocity = rb.velocity;
		float originalY = velocity.y;

		if (IsCurrentPathIndexValid())
		{
			Vector3 waypoint = path[pathIndex].position;;

			// distance to next waypoint 
			float dist = Vector3.Distance(rb.position, waypoint);

			if (dist > closeEnough)
			{
				Vector3 dir = (waypoint - rb.position).normalized;
				velocity = dir * speed;

				// rotate towards that direction but without the y
				dir.y = 0.0f;
				rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * rotationSlerpRate));
			}
			// too close, skip to the next
			else
			{
				pathIndex++;
			}
		}
		// otherwise stop and clear the path
		else
		{
			velocity = Vector3.zero;
			Clear();
		}

		velocity.y += originalY;

		rb.velocity = velocity;
	}

	private bool IsCurrentPathIndexValid()
	{
		return (path != null && pathIndex >= 0 && pathIndex < path.Length);
	}

	/** traverse the provided path */
	public override void Traverse(Transform[] newPath)
	{
		base.Traverse(newPath);
		pathIndex = 0;
	}

	/** clear the path and stop the navigation */
	public override void Clear()
	{
		base.Clear();
		pathIndex = -1;
	}

}
