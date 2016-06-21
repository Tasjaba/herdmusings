using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** 
 * Sets up a path of waypoints based on mouse drag.
 * Optional particles can be emitted as well at a fixed interval along the path,
 * based on the normal of surface that it is being dragged across
 * Particles used for low & optimized draw calls
 * TODO: object pooling of waypoints to prevent garbage collection when clearing a path
 * TODO: make the trail start at the actor regardless of where the drag starts
 * TODO: make wps cast down to ground (or other flat surface) so that they are not floating
 */
public class PathSetter : MonoBehaviour {

	// prefab for waypoint
	public Transform wpPrefab;

	// list of waypoints in the path
	public List<Transform> wps;

	// what camera this is being cast from, leave null for Camera.main
	public Camera castCam = null;

	// what layers we care about casting onto
	public LayerMask castLayerMask;

	// max distance away from camera that we care about
	public float castDistance = 100.0f;

	// min distance between waypoints
	public float minDistanceBetweenWPs = 0.5f;

	// optional particle system for waypoints
	// should be in world coords
	// leave null for none
	public ParticleSystem trailParticleSystem = null;

	// distance between trail particle emissions
	public float trailParticleDistance = 0.5f;

	// particles to emit per distance
	public int emissionsPerDistance = 5;

	// PRIVATE VARIABLES
	//

	// in the process of drawing a path
	private bool drawingPathNow = false; 

	// the last position dragged by mouse
	private Vector3 lastHitPos;
	private Vector3 lastHitNormal;

	// min distance squared for faster calculation
	private float minDistanceBetweenWPSquared = 0.01f;

	// cached reference to the trailParticleSystem's transform
	private Transform trailParticleSystemTransform;
	private Vector3 lastTrailParticlePos;

	void Awake()
	{
		// make sure we have a waypoint prefab
		if (wpPrefab == null)
		{
			Debug.LogError("Set the wpPrefab of the PathSetter to something.");
		}

		// initialize the list
		wps = new List<Transform>();

		minDistanceBetweenWPSquared = minDistanceBetweenWPs * minDistanceBetweenWPs;

		// cache the trail particle system's transform
		if (trailParticleSystem != null)
		{
			trailParticleSystemTransform = trailParticleSystem.transform;
		}
	}

	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {

		// if mouse is pressed, then start or continue the path
		if (Input.GetButton("Fire1"))
		{
			StartOrContinuePath();
		}
		// if we were drawing a path, but not anymore,
		// then complete the path
		else if (drawingPathNow)
		{
			CompletePath();
		}
	}

	/** starts or continues the path at the mouse position */
	private void StartOrContinuePath()
	{
		Vector3 mousePosition = Input.mousePosition;

		// make sure we have a camera
		EnsureCameraExists();

		// see where in the world the mouse is pointing at
		RaycastHit raycastHit;
		Ray screenRay = castCam.ScreenPointToRay(mousePosition);

		// do a raycast to see where it hits
		if (Physics.Raycast(screenRay, out raycastHit, 100.0f, castLayerMask))
		{
			lastHitPos = raycastHit.point + raycastHit.normal * 0.11f;
			lastHitNormal = raycastHit.normal;

			// if we're not already in a path, start a path
			if (!drawingPathNow)
			{
				drawingPathNow = true;
				StartNewPath(lastHitPos);

				lastTrailParticlePos = lastHitPos;
				AddParticlesIfFarEnough(lastHitPos, true);

			}
			else
			{
				ContinueExistingPath(lastHitPos);
				AddParticlesIfFarEnough(lastHitPos);

			}


		}
	}
	
	private void EnsureCameraExists()
	{
		if (castCam == null)
		{
			castCam = Camera.main;
		}
	}

	/** clears out the old path and starts a new path */
	private void StartNewPath(Vector3 position)
	{
		
		RecycleWPs();
		AddWP(position, wpPrefab.rotation);

		// clear the particle system
		trailParticleSystem.Clear();
	}

	private void RecycleWPs()
	{
		// TODO: object pooling - put the waypoints objects back into the pool
		// for now we're just doing destroy
		for (int i = 0; i < wps.Count; i++)
		{
			Destroy(wps[i].gameObject);
		}
		wps.Clear();
	}

	/** continues the path - adding a WP if it is far enough from the previous */
	private void ContinueExistingPath(Vector3 position)
	{
		if (IsFarEnoughFromLastWP(position))
		{
			AddWP(position, wpPrefab.rotation);
		}
	}

	/** ends the path, drawing a final waypoint */
	private void CompletePath()
	{
		AddWP(lastHitPos, wpPrefab.rotation);
		drawingPathNow = false;
		AddParticlesIfFarEnough(lastHitPos, true);

	}

	/** creates and adds a WP, adds it to the list, and sets our transform as parent */
	private Transform AddWP(Vector3 position, Quaternion rotation)
	{
		// TODO: Add object pooling for the waypoint prefabs
		// to reduce garbage collection
		//

		Transform wp = Instantiate(wpPrefab, position, rotation) as Transform;
		wp.name = "WP" + wps.Count.ToString();
		wps.Add(wp);
		wp.parent = transform;

		// make the previous WP face this one
		if (wps.Count > 1)
		{
			wps[wps.Count - 2].LookAt(position);
		}

		return wp;
	}

	/** get the last WP in wps. If list is empty, then return null */
	private Transform lastWP
	{
		get {
			
			if (wps == null || wps.Count == 0)
			{
				return null;
			}

			return wps[wps.Count - 1];
		}
	}

	/** returns whether the position is far enough from the position of the last WP */
	private bool IsFarEnoughFromLastWP(Vector3 position)
	{
		if (lastWP == null)
		{
			return true;
		}

		return((Vector3.SqrMagnitude(position - lastWP.position) > minDistanceBetweenWPSquared));
	}
		
	// add particles along a fixed interval
	void AddParticlesIfFarEnough(Vector3 position, bool forced = false)
	{
		if (trailParticleSystem == null)
		{
			return;
		}

		// Vector from the last particle position to the new position
		Vector3 diff = position - lastTrailParticlePos;

		// direction of the vector
		Vector3 dir = diff.normalized;

		// total distance between the last particle and the position
		float totalDistance = diff.magnitude;

		if (totalDistance >= trailParticleDistance || forced)
		{
			Ray ray = new Ray(lastTrailParticlePos, dir);

			// loop through and fill in the particles between, if any
			for (float dist = (forced ? 0.0f : trailParticleDistance); dist <= totalDistance; dist += trailParticleDistance)
			{
				trailParticleSystemTransform.position = ray.GetPoint(dist);

				// make sure the particle is facing in the direction of the last normal
				trailParticleSystem.startRotation3D = ((Quaternion.LookRotation(lastHitNormal, dir)).eulerAngles) / 180.0f * Mathf.PI;

				trailParticleSystem.Emit(emissionsPerDistance);
				lastTrailParticlePos = position;
			}
		}
	}

	/** draw lines between the waypoints for the scene view */
	void OnDrawGizmos()
	{
		if (wps == null || wps.Count < 2)
			return;

		Gizmos.color = Color.green;

		for (int i = 0; i < wps.Count-1; i++)
		{
			
			Gizmos.DrawLine(wps[i].transform.position, wps[i + 1].transform.position);
		}
	}
}
