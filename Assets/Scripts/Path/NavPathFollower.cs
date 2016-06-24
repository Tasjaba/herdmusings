using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** subclass of PathFollower that uses Unity's NavMeshAgent */
[RequireComponent(typeof(NavMeshAgent))]
public class NavPathFollower : PathFollower {

	private int pathIndex = 0;
	private NavMeshAgent nav;

	void Awake() {
		nav = GetComponent<NavMeshAgent>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (IsCurrentPathIndexValid())
		{
			// check distance to current path waypoint
			Vector3 waypoint = path[pathIndex].position;
			float dist = Vector3.Distance(transform.position, waypoint);

			// if close enough, advance to next waypoint
			if (dist <= closeEnough)
			{
				pathIndex++;
			}

			if (nav.destination != waypoint)
			{
				nav.SetDestination(waypoint);
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
		if (isActiveAndEnabled)
		{
			nav.SetDestination(path[0].position);
			nav.Resume();
		}
	}
		
	/** clear the path and stop the navigation */
	public override void Clear()
	{
		base.Clear();
		if (isActiveAndEnabled)
		{
			pathIndex = -1;
			nav.Stop();
		}
	}
}
