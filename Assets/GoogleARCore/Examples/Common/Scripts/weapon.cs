using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coloreality;
public class weapon : MonoBehaviour {


    // Update is called once per frame	public float laserWidth = 0.1f;
	public float laserMaxLength = 5f;
	public Transform palm;
	private LineRenderer laserLineRenderer;
	FixedJoint fixedJoint;
	bool addedJointToSelected= false;
	private GameObject selectedObject;
    private bool isInitialPositionUpdated = false;
	private Ray ray = new Ray();
	private Quaternion CurrentRotation;
	private ColorealityManager cManager;
	void Start()
    {
		laserLineRenderer =  gameObject.AddComponent<LineRenderer>();
		ray.origin = transform.position;
		Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
		laserLineRenderer.SetWidth(start: 0.01f, end: 0.01f);
		//laserLineRenderer.SetWidth(0.1f, 0.1f);
		laserLineRenderer.SetPositions(initLaserPositions);
		laserLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		laserLineRenderer.SetColors(Color.red, Color.red);
		cManager = ColorealityManager.Instance;

	}

    void Update () {
		//ray.origin = palm.transform.position;
		ray.origin = getSholderPosition();
		ray.direction = -palm.transform.up;
		Physics.Raycast(ray);
	
		Debug.DrawRay(ray.origin, CurrentRotation * Vector3.forward * 5f);
		//Debug.DrawRay(ray.origin, ray.direction * laserMaxLength, Color.red);

		ShootLaserFromTargetPosition(ray);
	}

    void ShootLaserFromTargetPosition(Ray ray)
    {
		RaycastHit raycastHit;
        float ObjectDistance= 0.0f;

		if (Physics.Raycast(ray, out raycastHit, laserMaxLength))
        {
			laserLineRenderer.SetColors(Color.green, Color.green);

			selectedObject = raycastHit.collider.gameObject;
			
			print("HIT : " + selectedObject.name);

			if (Input.GetKey(KeyCode.Space))
			{

				laserLineRenderer.SetColors(Color.blue, Color.blue);

				selectedObject.transform.rotation = Quaternion.LookRotation(selectedObject.transform.position - Camera.main.transform.position);

				if (addedJointToSelected == false)
				{
					gameObject.AddComponent<FixedJoint>();
					addedJointToSelected = true;
					fixedJoint = gameObject.GetComponent<FixedJoint>();
					selectedObject.AddComponent<Rigidbody>();
					fixedJoint.connectedBody = selectedObject.gameObject.GetComponent<Rigidbody>();
				}
				print("space pressed");
			}
			else
			{
				Destroy(selectedObject.GetComponent<Rigidbody>());
				Destroy(fixedJoint);
				addedJointToSelected = false;

			}

		}
        else
        {
            isInitialPositionUpdated = false;
			laserLineRenderer.SetColors(Color.red, Color.red);
		}
		//laserLineRenderer.enabled = true;
		//laserLineRenderer.SetPosition(0, Camera.main.transform.position - new Vector3(0,0.1f,0));
		//laserLineRenderer.SetPosition(0, ray.origin);
		//laserLineRenderer.SetPosition(1, ray.direction* laserMaxLength);

	}

	public Vector3 getSholderPosition()
	{
		Vector3 ProjectionOrigin = Vector3.zero;

		if (cManager.Leap.Data.frame.Hands.Count != 0)
		{
			if (Camera.main != null)
			{
				if (Camera.main != null)
				{
					CurrentRotation = Camera.main.transform.rotation;
				}
				else
				{
					Debug.LogAssertion("Tag your Main Camera with 'MainCamera' for the UI Module");
				}
				switch (cManager.Leap.Data.frame.Hands[0].IsRight)
				{
					case true:
						ProjectionOrigin = Camera.main.transform.position + CurrentRotation * new Vector3(0.15f, -0.2f, 0f);
						break;
					case false:
						ProjectionOrigin = Camera.main.transform.position + CurrentRotation * new Vector3(-0.15f, -0.2f, 0f);
						break;
				}


			}
		}


		return ProjectionOrigin;

	}

	


}
