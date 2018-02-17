using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using GodTouches;
using DG.Tweening;
using Tower.Object;

namespace Tower
{
	public class GameManager : MonoBehaviour {

		// 自分のターンかどうかのフラグ
		// public static bool IsMyTurn = false;

		public static GameStatus Status;

		// 部屋のマスター権限を持っているかのフラグ
		public static bool IsMaster = false;

		[SerializeField]
		private Button rotateButton;

		[SerializeField]
		private SpriteRenderer cloudImage;

		[SerializeField]
		private Text debugText;

		[SerializeField]
		private SpriteRenderer myTurnText;

		[SerializeField]
		private GameObject localObjectBase;

		private GameObject oshimen;



		void Awake() {
			Application.targetFrameRate = 60;

			// OnRaiseEventをコールバック登録
			PhotonNetwork.OnEventCall += OnRaiseEvent;
			// Photonに接続する(引数でゲームのバージョンを指定できる)
			PhotonNetwork.ConnectUsingSettings(null);
		}

		void Start () {
			this.myTurnText.enabled = false;

			// 回転ボタン押下時
			this.rotateButton.OnClickAsObservable()
				.Where(_ => this.oshimen)
				.Where(_ => Status == GameStatus.PlayerTurn && this.oshimen.GetComponent<Rigidbody2D>().isKinematic)
				.Subscribe(_ =>
				{
					// 45度回転させる
					this.oshimen.GetComponent<OshimenObject>().Rotate45();
				});
			
			// オブジェクトが着地したかどうかをチェックする
			this.UpdateAsObservable()
				.Where(_ => this.oshimen)
				.Where(__ => this.oshimen.GetComponent<OshimenObject>().IsGround)
				.Subscribe(_ =>
				{
					Debug.Log("ターンエンド");
					// 自分のターンを終え、相手のターンに移行
					this.myTurnText.enabled = false;
					Status = GameStatus.EnemyTurn;
					this.oshimen = null;
					//TODO: 現在は１件しか飛ばすものがないので引数は仮値
					PhotonNetwork.RaiseEvent((byte)1, "TurnEnd", true, RaiseEventOptions.Default );
				});

			this.UpdateAsObservable()
				.Where(_ => Status == GameStatus.ConnectionWait && Input.GetMouseButtonDown (0))
				.Subscribe(__ => CreateLocalOshimen() );
		}

		
		// Update is called once per frame
		void Update () {
			MoveCloud();
		}

		// 雲の移動
		private void MoveCloud()
		{
			var cloudPos = this.cloudImage.transform.position;
			// 画面左端まで行ったら右端に移動させる
			if (cloudPos.x <= - 3.8f)
			{
				cloudPos.x = 4.0f;
			}
			this.cloudImage.transform.position = new Vector3(cloudPos.x - 0.005f, cloudPos.y);
		}

		// オブジェクトを生成
		private void CreateOshimen()
		{
			var oshimen = Resources.Load("oshimenObject") as GameObject;
			this.oshimen = PhotonNetwork.Instantiate("oshimenObject",oshimen.transform.position,Quaternion.identity,0);
			
			this.myTurnText.enabled = true;
			Status = GameStatus.PlayerTurn;
		}

		// ロビーに入ると呼ばれる
		void OnJoinedLobby() {
			string message = "ロビーに入りました。";
			Debug.Log(message);
			debugText.text = message;
	
			// ルームに入室する
			PhotonNetwork.JoinRandomRoom();
		}
	
		// ルームに入室すると呼ばれる
		void OnJoinedRoom() {
			string message = "ルームへ入室しました。";
			debugText.text = message;
			Debug.Log(message);

			// 1人 = 接続待ち状態
			if (PhotonNetwork.countOfPlayersOnMaster == 1)
			{
				Status = GameStatus.ConnectionWait;
				Debug.Log("GameStatus.ConnectionWait" + Status);
			}
			// 2人以上 = 既に誰か入っているので敵側からターンスタート
			else
			{
				Status = GameStatus.EnemyTurn;
			}
		}
	
		// ルームの入室に失敗すると呼ばれる
		void OnPhotonRandomJoinFailed() {
			string message = "ルームの入室に失敗しました。";
			debugText.text = message;
			Debug.Log(message);
	
			// ルームがないと入室に失敗するため、その時は自分で作る
			// 引数でルーム名を指定できる
			PhotonNetwork.CreateRoom("myRoomName");
		}

		// 他の誰かが入ってきた = 自身がマスター判定
		void OnPhotonPlayerConnected( PhotonPlayer newPlayer ) {
			string message = "誰かがルームに入室しました。";
			debugText.text = message;
			Debug.Log(message);

			// ミニゲームのオブジェクトを全部削除
			foreach (Transform n in this.localObjectBase.transform )
			{
				GameObject.Destroy(n.gameObject);
			}
			IsMaster = true;	
			Status = GameStatus.PlayerTurn;
			CreateOshimen();
		}

		private void OnRaiseEvent( byte i_eventcode, object i_content, int i_senderid )
		{
			// ターンが変更した時に呼ばれる
			Debug.Log("OnRaiseEvent");
			this.oshimen = null;
			CreateOshimen();
		}

		// 通信待ち状態の時にローカル用のオブジェクト生成
		private void CreateLocalOshimen()
		{
			Debug.Log("CreateLocalOshimen");
			var touchPos = Camera.main.ScreenToWorldPoint(GodTouch.GetPosition());
			var oshimen = Resources.Load("oshimenLocalObject") as GameObject;
			var createPos = new Vector3(touchPos.x, touchPos.y);
			Instantiate(oshimen, createPos, Quaternion.identity, this.localObjectBase.transform);
		}

	}
}
