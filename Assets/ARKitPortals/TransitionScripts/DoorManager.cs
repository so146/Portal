﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

// This class shows and hides doors (aka portals) when you walk into them. It listens for all OnPortalTransition events
// and manages the active portal.
public class DoorManager : MonoBehaviour {

	public delegate void DoorAction(Transform door);
	public static event DoorAction OnDoorOpen;

	public GameObject doorToVirtual;
	public GameObject doorToReality;

	public Camera mainCamera;

	private GameObject currDoor;

	private bool isCurrDoorOpen = false;
	private bool isNextDoorVirtual = true;

	void Start(){
		PortalTransition.OnPortalTransition += OnDoorEntrance;
	}

	// This method is called from the Spawn Portal button in the UI. It spawns a portal in front of you.
	public void OpenDoorInFront(){
		if (!isCurrDoorOpen) {
			if (isNextDoorVirtual)
				currDoor = doorToVirtual;
			else
				currDoor = doorToReality;
			

			currDoor.SetActive (true);


			if (isNextDoorVirtual) {

				//Get the door position by ARHitTest
				ARPoint point = new ARPoint { 
					x = 0.5f, //do a hit test at the center of the screen
					y = 0.5f
				};

				// prioritize result types
				ARHitTestResultType[] resultTypes = {
					ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, // if you want to use bounded planes
					//ARHitTestResultType.ARHitTestResultTypeExistingPlane, // if you want to use infinite planes 
					//ARHitTestResultType.ARHitTestResultTypeFeaturePoint // if you want to hit test on feature points
				}; 

				foreach (ARHitTestResultType resultType in resultTypes) {
					if (HitTestWithResultType (point, resultType)) {
						//return;
					}
				}

			} else {
				//Set the door postition. Don't use ARHitTest.
				currDoor.transform.position = (Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up)).normalized
					+ mainCamera.transform.position;

				currDoor.transform.rotation = Quaternion.LookRotation (
					Vector3.ProjectOnPlane(currDoor.transform.position - mainCamera.transform.position, Vector3.up));

				currDoor.GetComponentInParent<Portal>().Source.transform.localPosition = currDoor.transform.position;

			}


			isCurrDoorOpen = true;

			if (OnDoorOpen != null) {
				OnDoorOpen (currDoor.transform);
			}
		}
	}

	bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes) {
		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
		if (hitResults.Count > 0) {
			foreach (var hitResult in hitResults) {
				Vector3 pos = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
				Quaternion rot = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
				Vector3 normal = rot * Vector3.up;
				currDoor.transform.position = pos + normal * 1.451f;
				currDoor.transform.rotation = Quaternion.LookRotation (Vector3.ProjectOnPlane(pos - mainCamera.transform.position, rot * Vector3.up));

				currDoor.GetComponentInParent<Portal>().Source.transform.localPosition = currDoor.transform.position;
				return true;
			}
		}
		return false;
	}


	// Respond to the player walking into the doorway. Since there are only two portals, we don't need to pass which
	// portal was entered.
	private void OnDoorEntrance() {
		currDoor.SetActive(false);
		isCurrDoorOpen = false;
		isNextDoorVirtual = !isNextDoorVirtual;
	}
}
