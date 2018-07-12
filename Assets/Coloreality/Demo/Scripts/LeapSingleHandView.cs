using UnityEngine;
using System.Collections.Generic;
using Coloreality.LeapWrapper;

namespace Coloreality
{
	public class LeapSingleHandView : MonoBehaviour {
		[HideInInspector]
		public LeapHand hand;
		public Vector3 lineEnd;
		public Transform[] parts;
		public enum HandPart
		{
			Palm = 0,
			Finger_0,
			Finger_1,
			Finger_2,
			Finger_3,
			Finger_4,
			Wrist,
			Arm,
			Count
		}

		const int FIRST_FINGER_INDEX = (int)HandPart.Finger_0;
		void Start()
		{
			
		}

		public void UpdateHand(LeapHand value = null){
			if (value != null) {
				hand = value;
			}

			if (hand == null)
				return;

			parts[(int)HandPart.Palm].localPosition = hand.PalmPosition.ToHMDVector3();
			//parts[(int)HandPart.Palm].localPosition = hand.PalmPosition.ToScaledVector3();

			parts[(int)HandPart.Palm].localRotation = hand.Rotation.ToHMDQuaternion();
			//parts[(int)HandPart.Palm].localRotation = hand.Rotation.ToQuaternion();
			for (int fingerIndex = 0; fingerIndex < hand.Fingers.Count; fingerIndex++) {
				int index = fingerIndex + FIRST_FINGER_INDEX;
                parts[index].localPosition = hand.Fingers[fingerIndex].TipPosition.ToHMDVector3();
				//parts[index].localPosition = hand.Fingers[fingerIndex].TipPosition.ToScaledVector3();

			}
			Quaternion armRotation = hand.Arm.Rotation.ToHMDQuaternion();

            parts[(int)HandPart.Wrist].localPosition = hand.WristPosition.ToHMDVector3();
			parts[(int)HandPart.Wrist].localRotation = armRotation;

            parts[(int)HandPart.Arm].localPosition = hand.Arm.Center.ToHMDVector3();
			parts[(int)HandPart.Arm].localRotation = armRotation;

		


		}


	}
}