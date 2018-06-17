using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObdTile : MonoBehaviour {

	// Use this for initialization
	public string Title;
	public string Unit;

	void Start () {

		transform.Find("Title").GetComponent<TextMesh>().text = Title;
		transform.Find("Unit").GetComponent<TextMesh>().text = Unit;


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
