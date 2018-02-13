using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GodTouches;
using UnityEngine.EventSystems;
using UniRx;
using System;

public class OshimenObject : MonoBehaviour {

	public bool IsGround = false;
	private bool isTouch = false;

//	[PunRPC]
	private void SetGravity(bool isKinematic)
	{
		GetComponent<Rigidbody2D>().isKinematic = isKinematic;
	}

	// Use this for initialization
	void Start () {
		if (!GameManager.IsMaster)
		{
			// gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
		}		
	}
	
	// Update is called once per frame
	void Update () {
		if (Mathf.Abs(transform.position.x) > 4.0f || Mathf.Abs(transform.position.y) > 8.0f)
		{
			Debug.Log("Destroy");
			Destroy(gameObject);
			return;
		}

		var isKinematic = GetComponent<Rigidbody2D>().isKinematic;
		bool isPointer;
		#if UNITY_EDITOR
			isPointer = EventSystem.current.IsPointerOverGameObject();
		#else
			isPointer = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
		#endif

		if(isPointer || !isKinematic || this.IsGround || !GameManager.IsMyTurn){
            return;
        }

	    // タッチを検出して動かす
		var phase = GodTouch.GetPhase ();
		if (phase == GodPhase.Began) 
		{
			this.isTouch = true;
		}
		else if (phase == GodPhase.Moved && this.isTouch)
		{			
			var touchPos = Camera.main.ScreenToWorldPoint(GodTouch.GetPosition());
			transform.position = new Vector3(touchPos.x,transform.position.y);
			Debug.Log(name);
		}
		else if (phase == GodPhase.Ended && this.isTouch) 
		{
			PhotonView photonView = PhotonView.Get(this);
			SetGravity(false);
		}
	}

	void OnCollisionEnter2D(Collision2D coll) {
		Observable.Timer(TimeSpan.FromMilliseconds(1000f))
			.Subscribe(_ =>
			{
				PhotonView photonView = PhotonView.Get(this);
				photonView.RPC("SetIsGround", PhotonTargets.All);
			});
	}
	void OnCollisionExit2D(Collision2D coll) {
	}

	[PunRPC]
	void SetIsGround()
	{
		Debug.Log("SetIsGround");
		this.IsGround = true;
	}

}
