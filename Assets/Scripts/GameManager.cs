using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using GodTouches;
using DG.Tweening;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private Button rotateButton;

	[SerializeField]
	private SpriteRenderer cloudImage;

	private GameObject oshimen;

	private bool isMyTurn = true;

	void Awake() {
		Application.targetFrameRate = 60;
	}

	void Start () {
		CreateOshimen();
		this.rotateButton.OnClickAsObservable()
			.Subscribe(_ =>
			{
				Debug.Log("Click");
				if (this.oshimen)
				{
					Debug.Log("oshimen not null");
					var currentRotate = this.oshimen.transform.rotation.eulerAngles.z;
					this.oshimen.transform.DORotate(new Vector3(0,0,currentRotate + 45f),0.2f)
						.SetEase(Ease.Linear);
				}
			});
		
		this.UpdateAsObservable()
			.Where(_ => oshimen)
			.Where(__ => oshimen.GetComponent<OshimenObject>().IsGround)
			.Subscribe(_ =>
			{
				CreateOshimen();
			});
	}
	
	// Update is called once per frame
	void Update () {
		MoveCloud();

	}

	private void MoveCloud()
	{
		var cloudPos = this.cloudImage.transform.position;
		if (cloudPos.x <= - 3.8f)
		{
			cloudPos.x = 4.0f;
		}
		this.cloudImage.transform.position = new Vector3(cloudPos.x - 0.005f, cloudPos.y);
	}

	private void CreateOshimen()
	{
		var oshimen = Resources.Load("oshimenObject") as GameObject;		
		this.oshimen = Instantiate(oshimen);
	}


}
