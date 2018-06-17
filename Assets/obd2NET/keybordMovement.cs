using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keybordMovement : MonoBehaviour {

	public float speed = 0.1f;
	public void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.Translate(Vector3.right * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.Translate(Vector3.left * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.Translate(Vector3.back * Time.deltaTime * speed);
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.Translate(Vector3.forward * Time.deltaTime * speed);
		}


	}
}
