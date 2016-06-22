using UnityEngine;
using System.Collections;

public class PathFollower : MonoBehaviour {

	public Transform[] path;
	public float closeEnough = 0.5f;

	/** traverse the provided path */
	public virtual void Traverse(Transform[] newPath)
	{
		path = newPath;
	}

	/** clear the path and stop the navigation */
	public virtual void Clear()
	{
		path = null;
	}
}
