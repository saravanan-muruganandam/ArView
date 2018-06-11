using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class graph : MonoBehaviour {


	//Box collider to give shape to the gaph, and position
	private BoxCollider collider;
	// XY scale maximum minimum values
	public float timeLinelengthInSec = 40f;
	public float maxYscaleValue = 1500f;
	public float minYscaleValue = 0.0f;
	private static System.DateTime currentTime ; 
	private static System.DateTime XscaleLastTime;
	//No of dividers in both X and Y scale
	public int noOfYpartition = 10;
	public float noOfXpartition = 10;
	// Game objects, X Y and graph line
	private Transform xAxisGameObject=null;
	private Transform yAxisGameObject=null;
	private Transform graphLineObject=null;
	//Line rendereer for X Y and graph lines
	private LineRenderer xLine=null;
	private LineRenderer yLine=null;
	private LineRenderer graphLine=null;
	// Graph plot value
	private List<GraphPoint> graphPoints;
	public Object xDividerPrefab;
	public Object yDividerPrefab;
	// Four vertor point of a face in square
	private Vector3 vertice1;
	private Vector3 vertice2;
	private Vector3 vertice3;
	private Vector3 vertice4;
	//List of divider in the x and y scale lines 
	ArrayList yDividerGameobjects;
	ArrayList xDividerGameobjects;

	public float getXYslot(float axisValue, float axisMaxValue, float axisMinValue){
		float valuePosition=0.0f;

		if (axisValue < axisMaxValue) {
			valuePosition = (axisMaxValue-axisValue) / (axisMaxValue-axisMinValue);

		}
		return valuePosition;
	}

	public float getTimeDiffinPercentage(System.DateTime valueTime)
	{
		
		return (float)(currentTime - valueTime).TotalSeconds/ timeLinelengthInSec;
	}

	public System.DateTime getCurrentTime()
	{
		return System.DateTime.Now;
	}

	void Start () {

		currentTime = getCurrentTime();
		//xDividerPrefab = AssetDatabase.LoadAssetAtPath("Assets/X_Div.prefab", typeof(GameObject));
		//yDividerPrefab = AssetDatabase.LoadAssetAtPath("Assets/Y_Div.prefab", typeof(GameObject));

		yDividerGameobjects= new ArrayList();
		xDividerGameobjects = new ArrayList();
		collider = gameObject.GetComponent<BoxCollider>();

		graphPoints = new List<GraphPoint>(); 
		//XScaleline and YscaleLIne
		xAxisGameObject = transform.Find("xAxis");
		yAxisGameObject = transform.Find("yAxis");
		graphLineObject = transform.Find("GraphLine");

		xLine = xAxisGameObject.GetComponent<LineRenderer>();
		yLine = yAxisGameObject.GetComponent<LineRenderer>();
		graphLine = graphLineObject.GetComponent<LineRenderer>();
		xLine.gameObject.SetActive(true);
		yLine.gameObject.SetActive(true);
		graphLine.gameObject.SetActive(true);
		graphLine.positionCount = graphPoints.Count;
		graphLine.loop = false;

		graphLine.startWidth = graphLine.endWidth = yLine.startWidth = xLine.startWidth = yLine.endWidth = xLine.endWidth = 0.03f;
		graphLine.material = xLine.material = yLine.material = new Material(Shader.Find("Particles/Additive"));

		for (int i = 0; i < noOfYpartition + 1; i++)
		{
			GameObject yDivider = Instantiate(yDividerPrefab, gameObject.transform.parent) as GameObject;
			yDivider.transform.localScale = new Vector3(0.1510651f, 0.1510651f, 0.1510651f);
			yDividerGameobjects.Add(yDivider);

		}
		for (int i = 0; i < noOfXpartition + 1; i++)
		{
			GameObject xDivider = Instantiate(xDividerPrefab,gameObject.transform.parent) as GameObject;
			xDivider.transform.localScale = new Vector3(0.1510651f, 0.1510651f, 0.1510651f);
			xDividerGameobjects.Add(xDivider);

		}
		
		StartCoroutine ("alignGraphValue");
		StartCoroutine("addCurrentValue");
	}

	// Update is called once per frame
	void Update()
	{
		currentTime = getCurrentTime();
		// four vector position of the rectangle face in a collider
		vertice1 = gameObject.transform.TransformPoint(collider.center + new Vector3(-collider.size.x, -collider.size.y, -collider.size.z)*0.5f);
		vertice2 = gameObject.transform.TransformPoint(collider.center + new Vector3(collider.size.x, -collider.size.y, -collider.size.z)*0.5f);
		vertice3 = gameObject.transform.TransformPoint(collider.center + new Vector3(-collider.size.x, collider.size.y, -collider.size.z)*0.5f);
		vertice4 = gameObject.transform.TransformPoint(collider.center + new Vector3(collider.size.x, collider.size.y, -collider.size.z) * 0.5f);

		//Debug.DrawLine(vertice3, vertice4, Color.blue);
		//line for x axis
		xLine.SetPosition(0, vertice1);
		xLine.SetPosition(1, vertice2);
		//line y scale end points
		yLine.SetPosition(0, vertice1);
		yLine.SetPosition(1, vertice3);

		foreach (GameObject divY in yDividerGameobjects)
		{
			divY.transform.position = Vector3.Lerp (vertice1, vertice3,(float) yDividerGameobjects.IndexOf(divY)/noOfYpartition);
			divY.transform.Find ("valueText").GetComponent<TextMesh>().text =  ((((float)yDividerGameobjects.IndexOf(divY)/noOfYpartition))*maxYscaleValue).ToString();
		}

		foreach (GameObject divX in xDividerGameobjects)
		{
			divX.transform.position = Vector3.Lerp (vertice1, vertice2,(float) xDividerGameobjects.IndexOf(divX)/noOfXpartition);

		}
	}

	IEnumerator alignGraphValue() {
		while (true) {
			graphLine.positionCount = graphPoints.Count;
			XscaleLastTime = getCurrentTime().AddSeconds(-timeLinelengthInSec);
			
			foreach (GameObject divX in xDividerGameobjects)
			{
				divX.transform.Find("valueText").GetComponent<TextMesh>().text = (getCurrentTime().AddSeconds(-(double)(((xDividerGameobjects.IndexOf(divX) / noOfXpartition)) * (timeLinelengthInSec)))).ToString("HH:mm:ss");
				
			}

			foreach (GraphPoint graphPoint in graphPoints)
			{
				float YLearpPercent = 1 - getXYslot(graphPoint.graphValue, maxYscaleValue, minYscaleValue);

				Vector3 xScalevalue = Vector3.Lerp(vertice1, vertice2, getTimeDiffinPercentage(graphPoint.valueRecordedTime)); //modify this to adjust the graphpoint horizontal alignment and movement

				Vector3 xScaleTopValue = Vector3.Lerp(vertice3, vertice4, getTimeDiffinPercentage(graphPoint.valueRecordedTime));

				Vector3 actualPoint = Vector3.Lerp(xScalevalue, xScaleTopValue, YLearpPercent);
				graphPoint.worldPositionPoint = actualPoint;
				graphPoint.mark.transform.position = actualPoint;
				graphPoint.mark.name = graphPoints.IndexOf(graphPoint).ToString();
				graphPoint.mark.SetActive(true);
				graphLine.SetPosition(graphPoints.IndexOf(graphPoint), graphPoint.worldPositionPoint);
				if ((getTimeDiffinPercentage(graphPoint.valueRecordedTime)) >= 1f)
				{
					Destroy(graphPoint.mark);
					graphPoints.Remove (graphPoint);
					break;
				}
			}

			yield return null;
		}

	}

	IEnumerator addCurrentValue()
	{
		while (true)
		{

			System.Random random = new System.Random();

			GraphPoint graphpoint = new GraphPoint();
			float YValue = Random.Range(minYscaleValue, maxYscaleValue);
			graphpoint.graphValue = YValue;
			graphpoint.mark = Instantiate(xDividerPrefab, gameObject.transform.parent) as GameObject;
			graphpoint.mark.SetActive(false);
			graphpoint.mark.transform.localScale = new Vector3(0.1110651f, 0.1110651f, 0.1110651f);
			graphpoint.valueRecordedTime = getCurrentTime();
			//graphpoint.mark.transform.Find("valueText").GetComponent<TextMesh>().text = XY.x.ToString()+" , "+ XY.y.ToString();
			graphpoint.mark.transform.Find("valueText").GetComponent<TextMesh>().text = currentTime.ToString("HH:mm:ss.ffff") + " , " + YValue.ToString();
			//graphpoint.mark.transform.parent = collider.transform;
			graphPoints.Add(graphpoint);
			graphPoints.Sort(new sortGraphPointByTime());
			yield return new WaitForSeconds(0.3f);

		}
	}

	public class sortGraphPointByTime : IComparer<GraphPoint>
	{

		public int Compare(GraphPoint a, GraphPoint b)
		{
			System.DateTime c1 = a.valueRecordedTime;
			System.DateTime c2 = b.valueRecordedTime;
			if (c1 > c2)
				return 1;
			if (c1 < c2)
				return -1;
			else
				return 0;
		}
	}


}

public class GraphPoint{

	public System.DateTime valueRecordedTime { get; set; }
	public float graphValue{ get; set; }
	public Vector3 worldPositionPoint{ get; set; } 
	public GameObject mark { get; set; }
}


