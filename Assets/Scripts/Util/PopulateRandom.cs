using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/** populates the prefab within the bounds relative to this transform.
 * The instantiated objects will be children.
 * See context menu functions for use in editor.
 * If run in the editor, the instantiated objects will keep their link
 * to the prefab.
 **/
public class PopulateRandom : MonoBehaviour {

	public Transform prefab;

	public int population = 10;
	public Bounds bounds;

	public bool repopulateOnStart = true;

	// Use this for initialization
	void Start () {
		Clear();
		Populate();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/** populate in the bounds and set the parent to this transform */
	[ContextMenu("Populate")]
	public void Populate()
	{
		Vector3 center = transform.position + bounds.center;
		for (int i = 0; i < population; i++)
		{
			Vector3 pos = center;
			pos.x += Random.Range(-bounds.extents.x, bounds.extents.x);
			pos.y += Random.Range(-bounds.extents.y, bounds.extents.y);
			pos.z += Random.Range(-bounds.extents.z, bounds.extents.z);

			Quaternion rot = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
			// for unity editor, keep the link to the prefab
			#if UNITY_EDITOR
				Transform instance = PrefabUtility.InstantiatePrefab(prefab) as Transform;
				instance.position = pos;
				instance.rotation = rot;
			#else
				Transform instance = Instantiate(prefab, pos, rot) as Transform;
			#endif
			instance.SetParent(transform);
		}
	}

	/** removes all the children */
	[ContextMenu("Clear")]
	public void Clear()
	{
		int childCount = transform.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			GameObject child = transform.GetChild(i).gameObject;
			#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					Destroy(transform.GetChild(i).gameObject);
				}
				else
				{
					DestroyImmediate(transform.GetChild(i).gameObject);
				}
			#else
				Destroy(transform.GetChild(i).gameObject);
			#endif
		}
	}


}
