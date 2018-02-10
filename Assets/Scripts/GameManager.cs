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

	[SerializeField]
	private Text debugText;

	private GameObject oshimen;

	private bool isMyTurn = true;

	void Awake() {
		Application.targetFrameRate = 60;
	}

	void Start () {
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

		// Photonに接続する(引数でゲームのバージョンを指定できる)
        PhotonNetwork.ConnectUsingSettings(null);
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
		this.oshimen = PhotonNetwork.Instantiate("oshimenObject",oshimen.transform.position,Quaternion.identity,0);
	}

	// ロビーに入ると呼ばれる
    void OnJoinedLobby() {
        Debug.Log("ロビーに入りました。");
		debugText.text = "ロビーに入りました。";
 
        // ルームに入室する
        PhotonNetwork.JoinRandomRoom();
    }
 
    // ルームに入室すると呼ばれる
    void OnJoinedRoom() {
		debugText.text = "ルームへ入室しました。";
        Debug.Log("ルームへ入室しました。");
		CreateOshimen();
    }
 
    // ルームの入室に失敗すると呼ばれる
    void OnPhotonRandomJoinFailed() {
		debugText.text = "ルームの入室に失敗しました。";
        Debug.Log("ルームの入室に失敗しました。");
 
        // ルームがないと入室に失敗するため、その時は自分で作る
        // 引数でルーム名を指定できる
        PhotonNetwork.CreateRoom("myRoomName");
    }

	void OnPhotonPlayerConnected( PhotonPlayer newPlayer ) {
		debugText.text = "誰かがルームに入室しました。";
        Debug.Log("誰かがルームに入室しました。");		
	}


}
