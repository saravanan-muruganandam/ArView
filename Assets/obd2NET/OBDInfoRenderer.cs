using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBDInfoRenderer : MonoBehaviour {

	// Use this for initialization

	public float lineWidth = 0.1f;
	public float lineMaxLength = 5f;
	private LineRenderer laserLineRenderer;
	public Transform parentObjTransform;

	void Start () {
		//parentObjTransform = gameObject.GetComponentInParent<Transform>();

		laserLineRenderer = gameObject.AddComponent<LineRenderer>();

		Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
		laserLineRenderer.SetWidth(start: 0.01f, end: 0.01f);
		//laserLineRenderer.SetWidth(0.1f, 0.1f);
		laserLineRenderer.SetPositions(initLaserPositions);
		laserLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		laserLineRenderer.SetColors(Color.red, Color.red);

	}
	
	// Update is called once per frame
	void Update () {
		//transform.up = parentObjTransform.up *5f;


		transform.position = parentObjTransform.position + new Vector3(0, 0.5f, 0);
		Vector3 direction = (transform.position - parentObjTransform.position).normalized;
		laserLineRenderer.enabled = true;
		laserLineRenderer.SetPosition(0, parentObjTransform.position);

		RaycastHit raycastHit;
		if (Physics.Raycast(parentObjTransform.position, direction, out raycastHit, 10))
		{
			laserLineRenderer.SetPosition(1, raycastHit.point);
		}

	}
}
