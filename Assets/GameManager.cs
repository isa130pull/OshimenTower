using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private Button rotateButton;

	[SerializeField]
	private SpriteRenderer cloudImage;

	// Use this for initialization
	void Start () {
		this.rotateButton.OnClickAsObservable()
			.Subscribe(_ =>
			{
				Debug.Log("Click");
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
}
