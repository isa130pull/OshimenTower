using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GodTouches;

public class OshimenObject : MonoBehaviour {

	private void SetGravity()
	{
		GetComponent<Rigidbody2D>().isKinematic = false;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    // タッチを検出して動かす
		var phase = GodTouch.GetPhase ();
		if (phase == GodPhase.Began) 
		{
		}
		else if (phase == GodPhase.Moved)
		{
			var touchPos = Camera.main.ScreenToWorldPoint(GodTouch.GetPosition());
			transform.position = new Vector3(touchPos.x,transform.position.y);
		}
		else if (phase == GodPhase.Ended) 
		{
			SetGravity();
		}
	}
}
