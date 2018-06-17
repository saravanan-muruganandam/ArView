using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using obd2NET;
	public class updateValueTest : MonoBehaviour {



	private List<GameObject> activeSimilarObject=null;

	void Start () {


	}


	void OnEnable()
	{

		StartCoroutine(coroutineA());

	}
	void Update () {

	}
	
	IEnumerator coroutineA()
	{
		while (true)

		{
			activeSimilarObject = new List<GameObject>(GameObject.FindGameObjectsWithTag("rpm"));
			Debug.Log(activeSimilarObject.Count);
			if (activeSimilarObject.Count > 0)
			{
				Vehicle.EngineTemperature().ToString();
				activeSimilarObject.ForEach(c => c.GetComponent<TextMesh>().text = Vehicle.EngineTemperature().ToString());
				yield return new WaitForSeconds(5);
			}
			else 
			{
				Debug.Log("LIST EMPTY");
				yield return new WaitForSeconds(2);

			}


			
		}
	}
	void OnApplicationQuit()
	{

		Vehicle.obdConnection.Close();
	}

}


