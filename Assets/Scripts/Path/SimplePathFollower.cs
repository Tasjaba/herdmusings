using UnityEngine;
using System.Collections;

/** simply follows the path at a given speed,
 * modifying the transform and direction
 */
public class SimplePathFollower : PathFollower {

	public float speed = 5.0f;

	public float rotationSlerpRate = 5.0f;
	private int pathIndex = 0;

	void Awake() {
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void FixedUpdate () {

		if (IsCurrentPathIndexValid())
		{
			// total distance to travel
			float travelDist = speed * Time.deltaTime;

			// check distance to current path waypoint
			Vector3 waypoint;
			int iterations = 0;
			while(travelDist > 0.0f && pathIndex < path.Length && iterations < 16)
			{
				waypoint = path[pathIndex].position;

				// distance to next waypoint 
				float dist = Vector3.Distance(transform.position, waypoint);

				// if can't reach with this travel distance
				// then go as far as you can towards it
				if (travelDist < dist)
				{
					Vector3 dir = (waypoint - transform.position).normalized;
					transform.position = transform.position + (dir * travelDist);
					travelDist = 0.0f;

					// rotate towards that direction but without the y
					dir.y = 0.0f;
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * rotationSlerpRate);
				}
				// if overshoot, then jump to next one
				// and reduce the remaining travel distance
				else
				{
					// start at that waypoint
					transform.position = waypoint;
					pathIndex++;

					// run out of paths, then break
					if (pathIndex >= path.Length)
					{
						Clear();
						break;
					}

					waypoint = path[pathIndex].position;
					travelDist = -dist;
				}
				iterations++;
			}
		}
		// otherwise stop and clear the path
		else
		{
			Clear();
		}
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
