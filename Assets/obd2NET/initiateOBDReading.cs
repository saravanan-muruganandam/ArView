using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class initiateOBDReading : MonoBehaviour {

	// Use this for initialization
	private float lineWidth = 0.1f;
	private float lineMaxLength = 5f;
	private LineRenderer laserLineRenderer;
	private GameObject obdInfo;
	public GameObject obdInfo_prefab;
	void Start () {
		//Object prefab = AssetDatabase.LoadAssetAtPath("Assets/obd2NET/OBDdisplayBoard.prefab", typeof(GameObject));
		//obdInfo = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		obdInfo = Instantiate(obdInfo_prefab) as GameObject;
		obdInfo.transform.SetParent(transform.parent);
		obdInfo.tag = "rpm-reading";

		laserLineRenderer = gameObject.AddComponent<LineRenderer>();
		Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
		laserLineRenderer.SetWidth(start: 0.01f, end: 0.01f);
		laserLineRenderer.SetPositions(initLaserPositions);
		laserLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		laserLineRenderer.SetColors(Color.red, Color.red);

	}
	
	// Update is called once per frame
	void Update () {

		//obdInfo.transform.LookAt(Camera.main.transform);
		obdInfo.transform.rotation = Quaternion.LookRotation(obdInfo.transform.position - Camera.main.transform.position);

		obdInfo.transform.position = transform.position + new Vector3(0, 0.5f, 0);
		Vector3 direction = (obdInfo.transform.position - transform.position).normalized;
		laserLineRenderer.enabled = true;
		laserLineRenderer.SetPosition(0, transform.position);

		RaycastHit raycastHit;
		if (Physics.Raycast(transform.position, direction, out raycastHit, 10))
		{
			laserLineRenderer.SetPosition(1, raycastHit.point);
		}

	}
}
