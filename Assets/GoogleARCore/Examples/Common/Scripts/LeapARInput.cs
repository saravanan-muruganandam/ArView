using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Coloreality;
using Coloreality.LeapWrapper;
using System;

namespace Leap.Unity.InputModule
{
	/** An InputModule that supports the use of Leap Motion tracking data for manipulating Unity UI controls. */
	public class LeapARInput : BaseInputModule
	{
		//General Interaction Parameters
		[Header (" Interaction Setup")]
		[Tooltip ("The current Leap Data Provider for the scene.")]
		/** The LeapProvider providing tracking data to the scene. */
		public ColorealityManager cManager;
		public GameObject handLeft;
		public GameObject handRight;
		LeapHand[] hands;
		GameObject[] handsGameObject;


		[Tooltip ("An optional alternate detector for pinching on the left hand.")]
		/** An optional component that will be used to detect pinch motions if set.
		 * Primarily used for projective or hybrid interaction modes (under experimental features).
		 */
		//public Leap.Unity.PinchDetector LeftHandDetector;
		// [Tooltip("An optional alternate detector for pinching on the right hand.")]
		/** An optional component that will be used to detect pinch motions if set.
		 * Primarily used for projective or hybrid interaction modes (under experimental features).
		 */
		// public Leap.Unity.PinchDetector RightHandDetector;
		// [Tooltip("How many hands and pointers the Input Module should allocate for.")]
		/** The number of pointers to create. By default, one pointer is created for each hand. */
		int NumberOfPointers = 2;

		//Customizable Pointer Parameters
		[Header (" Pointer Setup")]
		[Tooltip ("The sprite used to represent your pointers during projective interaction.")]
		/** The sprite for the cursor. */
		public Sprite PointerSprite;
		[Tooltip ("The material to be instantiated for your pointers during projective interaction.")]
		/** The cursor material. */
		public Material PointerMaterial;
		public Material LineMaterial;
		[Tooltip ("The color of the pointer when it is hovering over blank canvas.")]
		[ColorUsageAttribute (true, false, 0, 8, 0.125f, 3)]
		/** The color for the cursor when it is not in a special state. */
		public Color StandardColor = Color.white;
		[Tooltip ("The color of the pointer when it is hovering over any other UI element.")]
		[ColorUsageAttribute (true, false, 0, 8, 0.125f, 3)]
		/** The color for the cursor when it is hovering over a control. */
		public Color HoveringColor = Color.green;
		[Tooltip ("The color of the pointer when it is triggering a UI element.")]
		[ColorUsageAttribute (true, false, 0, 8, 0.125f, 3)]
		/** The color for the cursor when it is actively interacting with a control. */
		public Color TriggeringColor = Color.gray;
		[Tooltip ("The color of the pointer when it is triggering blank canvas.")]
		[ColorUsageAttribute (true, false, 0, 8, 0.125f, 3)]
		/** The color for the cursor when it is touching or triggering a non-active part of the UI (such as the canvas). */
		public Color TriggerMissedColor = Color.gray;

		//Advanced Options
		[Header (" Advanced Options")]
		[Tooltip ("Whether or not to show Advanced Options in the Inspector.")]
		public bool ShowAdvancedOptions = false;
		[Tooltip ("The distance from the base of a UI element that tactile interaction is triggered.")]
		/** The distance from the base of a UI element that tactile interaction is triggered.*/
		public float TactilePadding = 0.005f;
		[Tooltip ("The sound that is played when the pointer transitions from canvas to element.")]
		/** The sound that is played when the pointer transitions from canvas to element.*/
		public AudioClip BeginHoverSound;
		[Tooltip ("The sound that is played when the pointer transitions from canvas to element.")]
		/** The sound that is played when the pointer transitions from canvas to element.*/
		public AudioClip EndHoverSound;
		[Tooltip ("The sound that is played when the pointer triggers a UI element.")]
		/** The sound that is played when the pointer triggers a UI element.*/
		public AudioClip BeginTriggerSound;
		[Tooltip ("The sound that is played when the pointer triggers a UI element.")]
		/** The sound that is played when the pointer triggers a UI element.*/
		public AudioClip EndTriggerSound;
		[Tooltip ("The sound that is played when the pointer triggers blank canvas.")]
		/** The sound that is played when the pointer triggers blank canvas.*/
		public AudioClip BeginMissedSound;
		[Tooltip ("The sound that is played when the pointer triggers blank canvas.")]
		/** The sound that is played when the pointer triggers blank canvas.*/
		public AudioClip EndMissedSound;
		[Tooltip ("The sound that is played while the pointer is dragging an object.")]
		/** The sound that is played while the pointer is dragging an object.*/
		public AudioClip DragLoopSound;

		// Event delegates triggered by Input
		[System.Serializable]
		public class PositionEvent : UnityEvent<Vector3>
		{

		}

		[Header (" Event Setup")]
		[Tooltip ("The event that is triggered upon clicking on a non-canvas UI element.")]
		/** The event that is triggered upon clicking on a non-canvas UI element.*/
		public PositionEvent onClickDown;
		[Tooltip ("The event that is triggered upon lifting up from a non-canvas UI element (Not 1:1 with onClickDown!)")]
		/** The event that is triggered upon lifting up from a non-canvas UI element (Not 1:1 with onClickDown!)*/
		public PositionEvent onClickUp;
		[Tooltip ("The event that is triggered upon hovering over a non-canvas UI element.")]
		/** The event that is triggered upon hovering over a non-canvas UI element.*/
		public PositionEvent onHover;
		[Tooltip ("The event that is triggered while holding down a non-canvas UI element.")]
		/** The event that is triggered while holding down a non-canvas UI element.*/
		public PositionEvent whileClickHeld;

		[Tooltip ("Whether or not to show unsupported Experimental Options in the Inspector.")]
		public bool ShowExperimentalOptions = false;

		/** Defines the interaction modes :
		 *
		 *  - Hybrid: Both tactile and projective interaction. The active mode depends on the ProjectiveToTactileTransitionDistance value.
		 *
		 *  - Tactile: The user must physically touch the controls.
		 *
		 *  - Projective: A cursor is projected from the user's knuckle.
		 */
		public enum InteractionCapability : int
		{
			Hybrid,
			Tactile,
			Projective}

		;

		[Tooltip ("The interaction mode that the Input Module will be restricted to.")]
		/** The mode to use for interaction. The default mode is tactile. The projective mode is considered experimental.*/
		public InteractionCapability InteractionMode = InteractionCapability.Tactile;
		[Tooltip ("The distance from the base of a UI element that interaction switches from Projective-Pointer based to Touch based.")]
		/** The distance from the canvas at which to switch to projective mode. */
		public float ProjectiveToTactileTransitionDistance = 0.4f;
		[Tooltip ("The size of the pointer in world coordinates with respect to the distance between the cursor and the camera.")]
		/** The size of the pointer in world coordinates with respect to the distance between the cursor and the camera.*/
		public AnimationCurve PointerDistanceScale = AnimationCurve.Linear (0f, 0.1f, 6f, 1f);
		[Tooltip ("The size of the pointer in world coordinates with respect to the distance between the thumb and forefinger.")]
		/** The size of the pointer in world coordinates with respect to the distance between the thumb and forefinger.*/
		public AnimationCurve PointerPinchScale = AnimationCurve.Linear (30f, 0.6f, 70f, 1.1f);
		[Tooltip ("When not using a PinchDetector, the distance in mm that the tip of the thumb and forefinger should be to activate selection during projective interaction.")]
		/** When not using a PinchDetector, the distance in mm that the tip of the thumb and forefinger should be to activate selection during projective interaction.*/
		public float PinchingThreshold = 30f;
		[Tooltip ("Create a pointer for each finger.")]
		/** Create a pointer for each finger.*/
		public bool perFingerPointer = false;
		[Tooltip ("Render the pointer onto the enviroment.")]
		/** Render the pointer onto the enviroment.*/
		public bool EnvironmentPointer = false;
		[Tooltip ("The event that is triggered while pinching to a point in the environment.")]
		/** The event that is triggered while pinching to a point in the environment.*/
		public PositionEvent environmentPinch;
		[Tooltip ("Render a smaller pointer inside of the main pointer.")]
		/** Render a smaller pointer inside of the main pointer.*/
		public bool InnerPointer = false;
		[Tooltip ("The Opacity of the Inner Pointer relative to the Primary Pointer.")]
		/** The Opacity of the Inner Pointer relative to the Primary Pointer.*/
		public float InnerPointerOpacityScalar = 0.77f;
		[Tooltip ("Trigger a Hover Event when switching between UI elements.")]
		/** Trigger a Hover Event when switching between UI elements.*/
		public bool TriggerHoverOnElementSwitch = false;
		[Tooltip ("If the ScrollView still doesn't work even after disabling RaycastTarget on the intermediate layers.")]
		/** If the ScrollView still doesn't work even after disabling RaycastTarget on the intermediate layers.*/
		public bool OverrideScrollViewClicks = false;
		[Tooltip ("Draw the raycast for projective interaction.")]
		/** Draw the raycast for projective interaction.*/
		public bool DrawDebug = false;
		[Tooltip ("Retract compressible widgets when not using Tactile Interaction.")]
		/** Retract compressible widgets when not using Tactile Interaction.*/
		public bool RetractUI = false;
		[Tooltip ("Retransform the Interaction Pointer to allow the Module to work in a non-stationary reference frame.")]
		/** Retransform the Interaction Pointer to allow the Module to work in a non-stationary reference frame.*/
		public bool MovingReferenceFrame = false;
		//Event related data
		//private Camera EventCamera;
		private PointerEventData[] PointEventData;
		private pointerStates[] pointerState;
		private Transform[] Pointers;
		private Transform[] InnerPointers;
		private LineRenderer[] PointerLine;

		//Object the pointer is hovering over
		private GameObject[] currentOverGo;
		private GameObject[] currentOverUIGo;
		private GameObject[] prevOverUIGo;
		//Values from the previous frame
		private pointerStates[] PrevState;
		private Vector2[] PrevScreenPosition;
		private Vector2[] DragBeginPosition;
		private bool[] PrevTriggeringInteraction;
		private bool PrevTouchingMode;
		private GameObject[] prevOverGo;
		private float[] timeEnteredCanvas;

		//Misc. Objects
		private Canvas[] canvases;
		private Quaternion CurrentRotation;
		private AudioSource SoundPlayer;
		private GameObject[] currentGo;
		private GameObject[] currentGoing;
		private Vector3 OldCameraPos = Vector3.zero;
		private Quaternion OldCameraRot = Quaternion.identity;
		private float OldCameraFoV;
		private bool forceProjective = false;
		private bool forceTactile = false;
		private bool isDraging = false;
		private float objectInitialDistance;
		private bool isScaling = false;
		private bool objLock = false;
		private GameObject selectedForScaling;
		private float scalingObjectInitialDistance;
		private Vector3 objectInitialScale;
		//Queue of Spheres to Debug Draw
		private Queue<Vector3> DebugSphereQueue;


		enum pointerStates : int
		{
			OverCanvas,
			OverPhysElement,
			PinchingToCanvas,
			PinchingToPhysicElement,
			NearCanvas,
			TouchingCanvas,
			TouchingElement,
			OffCanvas,
			PoinchingCanvasElement,
			PinchingToNONClickable,
			OverNONClickable,
		MovingObjects}

		;

		//Initialization
		protected override void Start ()
		{
			cManager = ColorealityManager.Instance;
			handsGameObject = new GameObject[2];
			Pointers = new Transform[2];
			PointerLine = new LineRenderer[2];
			handsGameObject [0] = handLeft;
			handsGameObject [1] = handRight;
			hands = new LeapHand[2];
			canvases = Resources.FindObjectsOfTypeAll<Canvas> ();
			base.Start ();


			//Set Projective/Tactile Modes
			if (InteractionMode == InteractionCapability.Projective) {
				ProjectiveToTactileTransitionDistance = -float.MaxValue;
				forceTactile = false;
				forceProjective = true;
			} else if (InteractionMode == InteractionCapability.Tactile) {
				ProjectiveToTactileTransitionDistance = float.MaxValue;
				forceTactile = true;
				forceProjective = false;
			}

			//Initialize the Pointers for Projective Interaction
			if (perFingerPointer == true) {
				NumberOfPointers = 10;
			}
			
			for (int i = 0; i < handsGameObject.Length; i++) {
				
				PointerLine [i] = new LineRenderer ();
				GameObject pointerGameObj = new GameObject ("Pointer " + i);
				Pointers [i] = pointerGameObj.GetComponent<Transform> ();
				Pointers [i].transform.parent = handsGameObject [i].transform;
				pointerGameObj.transform.parent = Pointers [i];
				SpriteRenderer renderer = pointerGameObj.AddComponent<SpriteRenderer> ();
				renderer.sortingOrder = 1000;

				//Add your sprite to the Sprite Renderer

				renderer.sprite = PointerSprite;

				renderer.material = Instantiate (PointerMaterial);

				if (DrawDebug) {
					PointerLine [i] = pointerGameObj.AddComponent<LineRenderer> ();
					PointerLine [i].material = Instantiate (LineMaterial);
					//PointerLine [i].material.color = new Color (0f, 0f, 0f, 0f);

					PointerLine [i].positionCount = 2;
					PointerLine [i].startWidth = 0.002f;
					PointerLine [i].endWidth = 0.007f;

					pointerGameObj.SetActive (true);
				}

			}
			
			//Initialize our Sound Player
			SoundPlayer = this.gameObject.AddComponent<AudioSource> ();

			//Initialize the arrays that store persistent objects per pointer
			PointEventData = new PointerEventData[NumberOfPointers];
			pointerState = new pointerStates[NumberOfPointers];
			currentOverGo = new GameObject[NumberOfPointers];
			prevOverGo = new GameObject[NumberOfPointers];
			currentOverUIGo = new GameObject[NumberOfPointers];
			prevOverUIGo = new GameObject[NumberOfPointers];
			currentGo = new GameObject[NumberOfPointers];
			currentGoing = new GameObject[NumberOfPointers];
			PrevTriggeringInteraction = new bool[NumberOfPointers];
			PrevScreenPosition = new Vector2[NumberOfPointers];
			DragBeginPosition = new Vector2[NumberOfPointers];
			PrevState = new pointerStates[NumberOfPointers];
			timeEnteredCanvas = new float[NumberOfPointers];
			selectedForScaling = new GameObject ();
			//Used for calculating the origin of the Projective Interactions
			if (Camera.main != null) {
				CurrentRotation = Camera.main.transform.rotation;
			} else {
				Debug.LogAssertion ("Tag your Main Camera with 'MainCamera' for the UI Module");
			}

			//Initializes the Queue of Spheres to draw in OnDrawGizmos
			if (DrawDebug) {
				DebugSphereQueue = new Queue<Vector3> ();
			}
		}

		//Update the Head Yaw for Calculating "Shoulder Positions"
		void Update ()
		{

			
		}

		private void ProcessStateEvents (int whichPointer)
		{
			if (TriggerHoverOnElementSwitch) {
				if (currentOverUIGo[whichPointer] != null && pointerState[whichPointer] != pointerStates.MovingObjects) {
					if (currentOverUIGo [whichPointer] != prevOverUIGo [whichPointer]) {
						//When you begin to hover on an element
						SoundPlayer.PlayOneShot (BeginHoverSound);
						onHover.Invoke (Pointers [whichPointer].transform.position);
						
					}
				}
			}
		}
		//Tree to decide the State of the Pointer
		private void ProcessState (int whichPointer, LeapSingleHandView leapHandView, int whichFinger, bool forceTipRaycast)
		{
			if ((currentOverGo [whichPointer] != null)) {
				
				if (forceTactile || (!forceProjective && distanceOfTipToPointer (whichPointer, leapHandView, whichFinger) < ProjectiveToTactileTransitionDistance)) {
					if (isPinching (leapHandView.hand)) {
						if (ExecuteEvents.GetEventHandler<IPointerClickHandler> (PointEventData [whichPointer].pointerCurrentRaycast.gameObject)) {
							pointerState [whichPointer] = pointerStates.TouchingElement;
						} else {
							pointerState [whichPointer] = pointerStates.TouchingCanvas;
						}
					} else {
						pointerState [whichPointer] = pointerStates.NearCanvas;
					}
				} else if (forceTipRaycast) {
					if (isDraging || isScaling) {
						pointerState[whichPointer] = pointerStates.MovingObjects;
					}
					else if (PointEventData[whichPointer].pointerCurrentRaycast.gameObject==null) {// || PointEventData[whichPointer].dragging) {
						if (isPinching (leapHandView.hand)) {
							
							pointerState [whichPointer] = pointerStates.PinchingToPhysicElement;
						} else {
							
							pointerState [whichPointer] = pointerStates.OverPhysElement;
						}
					} else {
						if (isPinching (leapHandView.hand)) {
							pointerState [whichPointer] = pointerStates.PinchingToCanvas;
						} else {
							pointerState [whichPointer] = pointerStates.OverCanvas;
						}
					}
				} else {
					pointerState [whichPointer] = pointerStates.OffCanvas;
				}
			} else {
				pointerState [whichPointer] = pointerStates.OffCanvas;
			}
		}


		void evaluatePointerSize (int whichPointer)
		{
			//Use the Scale AnimCurve to Evaluate the Size of the Pointer
			float PointDistance = 1f;
			if (Camera.main != null) {
				PointDistance = (Pointers [whichPointer].position - Camera.main.transform.position).magnitude;
			}

			float Pointerscale = PointerDistanceScale.Evaluate (PointDistance);

			//if (InnerPointer) { InnerPointers[whichPointer].localScale = Pointerscale * PointerPinchScale.Evaluate(0f) * Vector3.one; }

			if (!perFingerPointer && !getTouchingMode (whichPointer)) {
				if (whichPointer == 0) {
					Pointerscale *= PointerPinchScale.Evaluate (handsGameObject [0].GetComponent<LeapSingleHandView> ().hand.PinchDistance);
				} else if (whichPointer == 1) {
					Pointerscale *= PointerPinchScale.Evaluate (handsGameObject [0].GetComponent<LeapSingleHandView> ().hand.PinchDistance);
				}
			}

			//Commented out Velocity Stretching because it looks funny when switching between Tactile and Projective
			Pointers [whichPointer].localScale = Pointerscale * new Vector3 (1f, 1f /*+ pointData.delta.magnitude*1f*/, 1f);
		}
		//Process is called by UI system to process events
		public override void Process ()
		{
			if (InteractionMode == InteractionCapability.Projective)
			{
				if (Camera.main != null)
				{

					Quaternion HeadYaw = Quaternion.Euler(0f, OldCameraRot.eulerAngles.y, 0f);
					CurrentRotation = Quaternion.Slerp(CurrentRotation, HeadYaw, 0.1f);
					if (isDraging | isScaling)
					{
						objLock = true;
					}
					else
					{
						objLock = false;
					}

					for (int i = 0; i < handsGameObject.Length; i++)
					{

						//Initialize a blank PointerEvent

						if (handsGameObject[i].activeSelf)
						{
							LeapSingleHandView leapHandView = handsGameObject[i].GetComponent<LeapSingleHandView>();

							hands[i] = leapHandView.hand;
							Vector3 ProjectionOrigin = getSholderPosition(leapHandView.hand);
							Vector3 direction = leapHandView.parts[(int)LeapSingleHandView.HandPart.Palm].transform.position - ProjectionOrigin;
							direction.Normalize();
							//Calculate Shoulder Positions (for Projection)
							bool TipRaycast = false;
							if (InteractionMode != InteractionCapability.Projective)
							{
								TipRaycast = GetLookPointerEventData(i, leapHandView, 1, ProjectionOrigin, CurrentRotation * Vector3.forward, true);
								PrevState[i] = pointerState[i]; //Store old state for sound transitionary purposes
								//UpdatePointer(i, PointEventData[i], PointEventData[i].pointerCurrentRaycast.gameObject);
								ProcessState(i, leapHandView, 1, TipRaycast);
							}
							if (((pointerState[i] == pointerStates.OffCanvas) && (InteractionMode != InteractionCapability.Tactile)) || (InteractionMode == InteractionCapability.Projective))
							{
								TipRaycast = GetLookPointerEventData(i, leapHandView, 1, ProjectionOrigin, direction, true);
								if ((InteractionMode == InteractionCapability.Projective))
								{
									PrevState[i] = pointerState[i]; //Store old state for sound transitionary purposes
								}
								//UpdatePointer(i, PointEventData[i], PointEventData[i].pointerCurrentRaycast.gameObject);
								if (!TipRaycast && (forceTactile || (!forceProjective && distanceOfTipToPointer(i, leapHandView, 1) < ProjectiveToTactileTransitionDistance)))
								{
									PointEventData[i].pointerCurrentRaycast = new RaycastResult();
								}
								ProcessState(i, leapHandView, 1, TipRaycast);

								//Debug.Log (pointerState [i]);
							}


							RaycastHit hit;
							if (Physics.Raycast(ProjectionOrigin, (direction * 100), out hit))
							{


								Debug.DrawRay(ProjectionOrigin, hit.point, Color.green);
								Pointers[i].position = hit.point;
								Pointers[i].rotation = Quaternion.LookRotation(hit.normal);
								PointerLine[i].SetPosition(0, ProjectionOrigin);
								PointerLine[i].SetPosition(1, Pointers[i].position);
								if (!objLock)
								{
									prevOverGo[i] = currentOverGo[i];
									currentOverGo[i] = hit.collider.gameObject;
								}

							}
							else
							{
								Pointers[i].position = (direction * 100);
								Pointers[i].rotation = Quaternion.LookRotation(Pointers[i].position - Camera.main.transform.position);
								PointerLine[i].SetPosition(0, ProjectionOrigin);
								PointerLine[i].SetPosition(1, Pointers[i].position);
								Debug.DrawRay(ProjectionOrigin, Pointers[i].position, Color.green);
								if (!objLock)
								{
									currentOverGo[i] = null;

								}

							}

							evaluatePointerSize(i);
							if (isPinching(leapHandView.hand))
							{
								environmentPinch.Invoke(Pointers[i].position); //TODO
							}

							//Trigger events that come from changing pointer state
							ProcessStateEvents(i);

							if (!isDraging)
							{
								base.HandlePointerExitAndEnter(PointEventData[i], currentOverGo[i]);
							}
							if (PointEventData[i].pointerCurrentRaycast.gameObject != null)
							{
								base.HandlePointerExitAndEnter(PointEventData[i], PointEventData[i].pointerCurrentRaycast.gameObject);
							}

							PrevScreenPosition[i] = PointEventData[i].position;

							Debug.Log(pointerState[i].ToString());
							ProcessStateEvents(i);
							// New code to drag the 3d object
							if (isGrabing(leapHandView.hand) && !isScaling && currentOverGo[i] != null)
							{

								StartDarging(i, leapHandView, ProjectionOrigin,  direction);

							}
							else
							{
								currentGo[i] = currentOverGo[i];
								isDraging = false;
							

							}

						}

					} 
					getPinchingHand();
					if (isBothHandsPintching(hands) && selectedForScaling != null)
					{
						scaleTheObject();
					}
					else
					{
						scalingObjectInitialDistance = 0f;
						isScaling = false;
						selectedForScaling = null;
					}

				}

				OldCameraPos = Camera.main.transform.position;
				OldCameraRot = Camera.main.transform.rotation;
				//Send update events if there is a selected object
				//This is important for InputField to receive keyboard events
				SendUpdateEventToSelectedObject();
				//Begin Processing Each Hand
			}
		}

		private void scaleTheObject()
		{
			if (!isScaling)
			{
				scalingObjectInitialDistance = getDistanceBetweenPinch(hands);
				objectInitialScale = selectedForScaling.transform.localScale;
				Debug.Log("-------" + scalingObjectInitialDistance);

			}
			isScaling = true;

			float scaleChangePercent = ((getDistanceBetweenPinch(hands) - scalingObjectInitialDistance) / scalingObjectInitialDistance);
			Debug.Log(scaleChangePercent + "-------" + scaleChangePercent * new Vector3(1, 1, 1));
			selectedForScaling.transform.localScale = Vector3.Lerp(selectedForScaling.transform.localScale, objectInitialScale + scaleChangePercent * new Vector3(1, 1, 1), Time.deltaTime * 10f);//Vector3.Lerp (selectedForScaling.transform.localScale, 0.99f*objectInitialScale.localScale , Time.deltaTime * 50f);

		}

		private void StartDarging(int whichPointer,LeapSingleHandView leapHandView, Vector3 projectionOrigin, Vector3 direction)
		{
			//isDraging = true;
			IDragHandler Dragger = currentOverGo[whichPointer].gameObject.GetComponent<IDragHandler>();
			if (Dragger != null)
			{

				if (Dragger is EventTrigger && currentOverGo[whichPointer].transform.parent)
				{ //Hack: EventSystems intercepting Drag Events causing funkiness

					currentGo[whichPointer] = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo[whichPointer].transform.parent.gameObject);
					if (currentGo[whichPointer] != null)
					{

						Dragger = currentGo[whichPointer].GetComponent<IDragHandler>();

						if ((Dragger != null) && (Dragger is EventTrigger))
						{
							currentGoing[whichPointer] = currentGo[whichPointer];
							DragBeginPosition[whichPointer] = currentGo[whichPointer].transform.position;
							if (currentGoing[whichPointer] != null && leapHandView.hand.PalmVelocity.ToVector3().magnitude > 50.0f)
							{
								if (!isDraging)
								{
									objectInitialDistance = Vector3.Distance(projectionOrigin, currentGoing[whichPointer].transform.position);
								}
								isDraging = true;
								currentGoing[whichPointer].transform.rotation = Quaternion.LookRotation(currentGoing[whichPointer].transform.position - Camera.main.transform.position);
								currentGoing[whichPointer].transform.position = Vector3.Lerp(currentGoing[whichPointer].transform.position, projectionOrigin + direction * objectInitialDistance, Time.deltaTime * 10.0f);

							}
						}
					}
				}
				else
				{
					currentGoing[whichPointer] = currentGo[whichPointer];
					DragBeginPosition[whichPointer] = currentGo[whichPointer].transform.position;

					if (currentGoing[whichPointer] != null && leapHandView.hand.PalmVelocity.ToVector3().magnitude > 50.0f)
					{

						if (!isDraging)
						{
							objectInitialDistance = Vector3.Distance(projectionOrigin, currentGoing[whichPointer].transform.position);
						}
						isDraging = true;
						Debug.Log(leapHandView.hand.PalmVelocity.ToVector3().magnitude.ToString());
						currentGoing[whichPointer].transform.rotation = Quaternion.LookRotation(currentGoing[whichPointer].transform.position - Camera.main.transform.position);
						currentGoing[whichPointer].transform.position = Vector3.Lerp(currentGoing[whichPointer].transform.position, projectionOrigin + direction * objectInitialDistance, Time.deltaTime * 10.0f);

					}
				}

			}
		}

		private bool GetLookPointerEventData (int whichPointer, LeapSingleHandView handView, int whichFinger, Vector3 Origin, Vector3 hitpoint, bool forceTipRaycast)
		{

			//Whether or not this will be a raycast through the finger tip
			bool TipRaycast = false;

			//Initialize a blank PointerEvent
			if (PointEventData [whichPointer] == null) {
				PointEventData [whichPointer] = new PointerEventData (base.eventSystem);
			} else {
				PointEventData [whichPointer].Reset ();
			}

			//We're always going to assume we're "Left Clicking", for the benefit of uGUI
			PointEventData [whichPointer].button = PointerEventData.InputButton.Left;

			//If we're in "Touching Mode", Raycast through the fingers
			Vector3 IndexFingerPosition;
			if (getTouchingMode (whichPointer) || forceTipRaycast) {
				TipRaycast = true;
				IndexFingerPosition = handView.hand.Fingers [1].TipPosition.ToVector3 ();
				//Focus pointer through the average of the extended fingers
				if (!perFingerPointer) {
					float farthest = 0f;
					
					for (int i = 1; i < 3; i++) {
						float fingerDistance = Vector3.Distance (Camera.main.transform.position, handView.hand.Fingers [i].TipPosition.ToVector3 ());
						float fingerExtension = Mathf.Clamp01 (Vector3.Dot (handView.hand.Fingers [i].Direction.ToVector3 (), handView.hand.Direction.ToVector3 ())) / 1.5f;
						if (fingerDistance > farthest && fingerExtension > 0.5f) {
							farthest = fingerDistance;
							IndexFingerPosition = handView.hand.Fingers [i].TipPosition.ToVector3 ();
						}
					}
				} else {
					IndexFingerPosition = handView.hand.Fingers [whichFinger].TipPosition.ToVector3 ();
				}

				//Else Raycast through the knuckle of the Index Finger
			} else {
				//Camera.main.transform.position = Origin;
				// IndexFingerPosition = leapData.frame.Hands[whichHand].Fingers[whichFinger].bones[0].Center.ToVector3();
				IndexFingerPosition = handView.hand.Fingers [whichFinger].TipPosition.ToVector3 ();
			}

			//Draw Camera Origin
			if (DrawDebug)
				DebugSphereQueue.Enqueue (Camera.main.transform.position);

			//Set EventCamera's FoV
			//Camera.main.fieldOfView = 179f;

			//Set the Raycast Direction and Delta
			PointEventData [whichPointer].position = Vector2.Lerp (PrevScreenPosition [whichPointer], Camera.main.WorldToScreenPoint (Pointers[whichPointer].position), 1.0f);//new Vector2(Screen.width / 2, Screen.height / 2);
			PointEventData [whichPointer].delta = (PointEventData [whichPointer].position - PrevScreenPosition [whichPointer]) * -10f;
			PointEventData [whichPointer].scrollDelta = Vector2.zero;
			
			//Perform the Raycast and sort all the things we hit by distance...
			base.eventSystem.RaycastAll (PointEventData [whichPointer], m_RaycastResultCache);

			//Optional hack that subverts ScrollRect hierarchies; to avoid this, disable "RaycastTarget" on the Viewport and Content panes
			if (OverrideScrollViewClicks) {
				PointEventData [whichPointer].pointerCurrentRaycast = new RaycastResult ();
				for (int i = 0; i < m_RaycastResultCache.Count; i++) {
					if (m_RaycastResultCache [i].gameObject.GetComponent<Scrollbar> () != null) {
						PointEventData [whichPointer].pointerCurrentRaycast = m_RaycastResultCache [i];
						
					} else if (PointEventData [whichPointer].pointerCurrentRaycast.gameObject == null && m_RaycastResultCache [i].gameObject.GetComponent<ScrollRect> () != null) {
						PointEventData [whichPointer].pointerCurrentRaycast = m_RaycastResultCache [i];
					}
				}
				if (PointEventData [whichPointer].pointerCurrentRaycast.gameObject == null) {
					PointEventData [whichPointer].pointerCurrentRaycast = FindFirstRaycast (m_RaycastResultCache);
				}
			} else {
				PointEventData [whichPointer].pointerCurrentRaycast = FindFirstRaycast (m_RaycastResultCache);
			}
			if (PointEventData[whichPointer].pointerCurrentRaycast.gameObject != null) {
				//Debug.Log(PointEventData[whichPointer].pointerCurrentRaycast.gameObject.name);
				prevOverUIGo[whichPointer] = currentOverUIGo[whichPointer];
				currentOverUIGo[whichPointer] = PointEventData[whichPointer].pointerCurrentRaycast.gameObject;
			}
			//Clear the list of things we hit; we don't need it anymore.
			m_RaycastResultCache.Clear ();

			return TipRaycast;
		}

		public bool getTouchingMode (int whichPointer)
		{
			return (pointerState [whichPointer] == pointerStates.NearCanvas || pointerState [whichPointer] == pointerStates.TouchingCanvas || pointerState [whichPointer] == pointerStates.TouchingElement);
		}

		public float distanceOfTipToPointer (int whichPointer, LeapSingleHandView handView, int whichFinger)
		{
			//Get Base of Index Finger Position
			Vector3 TipPosition = handView.hand.Fingers [whichFinger].TipPosition.ToHMDVector3 ();
			return (-Pointers [whichPointer].InverseTransformPoint (TipPosition).z * Pointers [whichPointer].lossyScale.z) - TactilePadding;
		}

		private void SendUpdateEventToSelectedObject ()
		{
		}

		public Vector3 getSholderPosition (LeapHand hand)
		{
			Vector3 ProjectionOrigin = Vector3.zero;

			if (Camera.main != null) {

				switch (hand.IsRight) {
				case true:
					ProjectionOrigin = Camera.main.transform.position + Camera.main.transform.rotation * new Vector3 (0.1f, -0.1f, 0f);
					break;
				case false:
					ProjectionOrigin = Camera.main.transform.position + Camera.main.transform.rotation * new Vector3 (-0.1f, -0.1f, 0f);
					break;
				}

			}

			return ProjectionOrigin;

		}

		public override bool ShouldActivateModule ()
		{

			return cManager != null && base.ShouldActivateModule ();
		}

		public bool isPinching (LeapHand hand)
		{

			if (InteractionMode != InteractionCapability.Tactile) {
				if (hand.PinchDistance < PinchingThreshold) {
					return true;
				
				}
			}

			//Disabling Pinching during touch interactions; maybe still desirable?
			//return LeapDataProvider.CurrentFrame.Hands[whichPointer].PinchDistance < PinchingThreshold;

			return false;
		}

		public bool isGrabing (LeapHand hand)
		{
			if (InteractionMode != InteractionCapability.Tactile) {
				if (hand.GrabStrength > 0.9f) {
					return true;
			
				}
			}

			//Disabling Pinching during touch interactions; maybe still desirable?
			//return LeapDataProvider.CurrentFrame.Hands[whichPointer].PinchDistance < PinchingThreshold;

			return false;
		}

		public bool isBothHandsPintching (LeapHand[] hands)
		{
			if (handsGameObject [0].activeSelf && handsGameObject [1].activeSelf) {

				if (hands.Length >= 2 && isPinching (hands [0]) && isPinching (hands [1])) {
					return true;
				} 
			}
			return false;
		}

		public LeapHand getPinchingHand ()
		{
			
			for (int i = 0; i < handsGameObject.Length; i++) {
				if (handsGameObject [i].activeSelf && hands [i] != null && hands [i].IsRight) {
					if (!isScaling && isPinching (hands [i]) && currentOverGo [i] != null) {
						selectedForScaling = currentOverGo [i].transform.root.gameObject;
						return hands [i];

					}
				}
			}

			return null;
		}

		public float getDistanceBetweenPinch (LeapHand[] hands)
		{

			return Vector3.Distance (hands [0].Fingers [1].TipPosition.ToVector3 (), hands [1].Fingers [1].TipPosition.ToVector3 ());
		}

	}
		
}



	

	
