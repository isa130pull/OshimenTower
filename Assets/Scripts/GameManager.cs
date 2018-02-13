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

	[SerializeField]
	private SpriteRenderer myTurnText;

	private GameObject oshimen;

	public static bool IsMyTurn = false;

	public static bool IsMaster = false;

	void Awake() {
		Application.targetFrameRate = 60;
        PhotonNetwork.OnEventCall += OnRaiseEvent;
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
				Debug.Log("IsGround");
				IsMyTurn = false;
				this.myTurnText.enabled = IsMyTurn;
				this.oshimen = null;
				PhotonNetwork.RaiseEvent((byte)1, "Hello!", true, RaiseEventOptions.Default );
			});

		// Photonに接続する(引数でゲームのバージョンを指定できる)
        PhotonNetwork.ConnectUsingSettings(null);
		this.myTurnText.enabled = IsMyTurn;
	}

	
	// Update is called once per frame
	void Update () {
		MoveCloud();

	}

	// 雲の移動
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
		
		this.myTurnText.enabled = IsMyTurn;
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
    }
 
    // ルームの入室に失敗すると呼ばれる
    void OnPhotonRandomJoinFailed() {
		debugText.text = "ルームの入室に失敗しました。";
        Debug.Log("ルームの入室に失敗しました。");
 
        // ルームがないと入室に失敗するため、その時は自分で作る
        // 引数でルーム名を指定できる
        PhotonNetwork.CreateRoom("myRoomName");

		IsMyTurn = true;
		this.myTurnText.enabled = IsMyTurn;
    }

	void OnPhotonPlayerConnected( PhotonPlayer newPlayer ) {
		debugText.text = "誰かがルームに入室しました。";
        Debug.Log("誰かがルームに入室しました。");
		IsMaster = true;
		CreateOshimen();
	}

	 private void OnRaiseEvent( byte i_eventcode, object i_content, int i_senderid )
    {
		Debug.Log("OnRaiseEvent");
       //ターンが変更した時に呼ばれる
	   switch (i_eventcode)
	   {
			case 1:
			IsMyTurn = true;
			this.oshimen = null;
			CreateOshimen();
			break;
	   }
    }


}
