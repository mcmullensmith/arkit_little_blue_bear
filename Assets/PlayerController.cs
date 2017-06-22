using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;

using System;

public class PlayerController : MonoBehaviour {
	public GameObject planePrefab;
	private GameObject[] planes;
	public Animator bodyAnimator;
	private UnityARAnchorManager unityARAnchorManager;
	Animator anim;
	private Vector3 offset;
	private bool shouldMove;
	private bool shouldRun;
	private bool planeDetected;
	private bool lookAtMe = true;
	public float speed;
	private bool respawn = true;
	private Vector3 newPosition;
	private bool planeSpawned = false;
	public GameObject instructions;
	public GameObject buttons;
	private Toggle toggle;
	private bool positionOn = true;

	[SerializeField]
     Toggle positionToggle;
	// Use this for initialization
	void Start () {
		gameObject.SetActive(false);
		anim = GetComponent<Animator>(); 
		unityARAnchorManager = new UnityARAnchorManager();
		UnityARUtility.InitializePlanePrefab (planePrefab);
		planeSpawned = true;
		UnityARSessionNativeInterface.ARAnchorAddedEvent += ARAnchorAdded;
	}

	public void ARAnchorAdded(ARPlaneAnchor anchorData) {
       if (!planeDetected && planeSpawned) {
			transform.position = UnityARMatrixOps.GetPosition (anchorData.transform);
			offset = Camera.main.transform.position - transform.position;
			transform.localScale = new Vector3(1.0f,1.0f , 1.0f);
			instructions.SetActive(false);
			gameObject.SetActive(true);
			buttons.SetActive(true);
			Hello();
			planeDetected = true;
	   }
      
    }

	bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
            if (hitResults.Count > 0) {
                foreach (var hitResult in hitResults) {
					
					if (respawn) {
						transform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
						transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
					} else {
						lookAtMe = false;
						// newPosition = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
						// transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
						transform.LookAt(UnityARMatrixOps.GetPosition (hitResult.worldTransform));
					}
                    // Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", transform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
                    return true;
                }
            }
            return false;
        }
	
	// Update is called once per frame
	void Update () {

		if (shouldMove) {
			transform.Translate (Vector3.forward * Time.deltaTime * (transform.localScale.x * speed));
		}

		if (Input.touchCount > 0 )
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(0))
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        ARHitTestResultType.ARHitTestResultTypeHorizontalPlane, 
                        ARHitTestResultType.ARHitTestResultTypeFeaturePoint,
						ARHitTestResultType.ARHitTestResultTypeVerticalPlane
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			} else {
				if (lookAtMe){
					transform.LookAt(Camera.main.transform.position);
					transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
				} 
			}
	}

	void OnCollisionEnter (Collision col){
		print("there was a collision: " + col.gameObject.tag);
        if(col.gameObject.tag == "MainCamera"){
            Idle();
        }
    }

	void OnDestroy() {
		unityARAnchorManager.Destroy ();
	}

	public void Walk() {
		anim.Play("A_walk");
		anim.SetBool("isMoving", true);
		anim.SetBool("isRunning", false);
		speed = .09f;
		shouldMove = true;
	}

	public void Run() {
		anim.Play("A_run");
		anim.SetBool("isRunning", true);
		anim.SetBool("isMoving", false);
		speed = .5f;
		shouldMove = true;
	}

	public void Idle() {
		anim.Play("A_idle");
		anim.SetBool("isMoving", false);
		anim.SetBool("isRunning", false);
		shouldMove = false;
	}

	public void LookAt() {
		lookAtMe = true;
		transform.LookAt(Camera.main.transform.position);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	public void Hello() {
		anim.Play("A_goodbye");
	}

	public void SetPosition() {
		if(positionToggle.isOn) {
         	respawn = true;
			 positionToggle.GetComponentInChildren<Text>().text = "Reset Position";
		} else {
			respawn = false;
			positionToggle.GetComponentInChildren<Text>().text = "Reset Lookat";
		}
	}

	public void ToggleDeleteMode(bool newValue) {
		SetPosition();
	}
}
