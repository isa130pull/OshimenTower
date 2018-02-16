using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GodTouches;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

namespace Tower.Object
{
	public class OshimenObject : MonoBehaviour {

		public bool IsGround = false;
		protected bool isTouch = false;

		protected int targetRotate = 0;

		// 45度アニメーション付きで回転させる
		public void Rotate45()
		{
			PhotonView.Get(this).RPC("RpcRotate", PhotonTargets.All);
		}

		private void Start()
		{
			// オブジェクトが画面外に出た時
			// TODO: オブジェクト削除ではなく、ゲームオーバー判定に用いる
			this.UpdateAsObservable()
				.Where(_ => Mathf.Abs(transform.position.x) > 4.0f || Mathf.Abs(transform.position.y) > 8.0f && this.IsGround)
				.Subscribe(_ =>
				{
					Debug.Log("DestroyObject");
					Destroy(gameObject);
					return;
				});
		}

		private void Update () 
		{
			bool isButtonTouches;
			// ボタンタップ中か否か
			#if UNITY_EDITOR
				isButtonTouches = EventSystem.current.IsPointerOverGameObject();
			#else
				isButtonTouches = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
			#endif
			var isKinematic = GetComponent<Rigidbody2D>().isKinematic;

			// 自キャラを操作できない条件
			// ①回転ボタンタップ中
			// ②重力設定中(自由落下中)
			// ③落下済
			// ④相手ターン
			if (isButtonTouches || !isKinematic || this.IsGround || !GameManager.IsMyTurn){
				return;
			}

			TouchProcess();
		}

		// タップ中の操作やタップエンド処理
		private void TouchProcess()
		{
			// タッチを検出して動かす
			var phase = GodTouch.GetPhase ();
			var touchPos = Camera.main.ScreenToWorldPoint(GodTouch.GetPosition());
			if (phase == GodPhase.Began) 
			{
				this.isTouch = true;
			}
			else if (phase == GodPhase.Moved && this.isTouch)
			{			
				PhotonView.Get(this).RPC("RpcPosition", PhotonTargets.All,touchPos);
			}
			else if (phase == GodPhase.Ended && this.isTouch) 
			{
				// 物理判定を適用させる(重力による自由落下含)
				PhotonView.Get(this).RPC("RpcSetKinematic", PhotonTargets.All, touchPos, false);
				this.isTouch = false;
			}
		}

		// 他オブジェクトに当たった瞬間呼ばれる
		private void OnCollisionEnter2D(Collision2D coll) {
			Observable.Timer(TimeSpan.FromMilliseconds(1000f))
				.Where(_ => !this.IsGround)
				.Subscribe(_ =>
				{
					// 他オブジェクトに当たって一定時間経過 = 着地した判定
					// TODO: オブジェクトの動きが止まったら着地判定
					PhotonView.Get(this).RPC("RpcIsGround", PhotonTargets.All);
				});
		}

		// 他オブジェクトへの接触処理が落ち着いたら呼ばれる
		void OnCollisionExit2D(Collision2D coll) {
		}


		// RPC経由でIsGroundフラグをTrueに共有させる
		[PunRPC]
		void RpcIsGround()
		{
			this.IsGround = true;
		}

		// RPC経由で操作キャラを45度回転させる
		[PunRPC]
		private void RpcRotate()
		{
			this.targetRotate += 45;
			this.transform.DORotate(new Vector3(0,0,this.targetRotate),0.2f)
				.SetEase(Ease.Linear);
		}

		// RPC経由で操作中キャラのPositionを同期させる
		[PunRPC]
		private void RpcPosition(Vector3 touchPos)
		{
			transform.position = new Vector3(touchPos.x,transform.position.y);
		}

		// RPC経由でKinematicのフラグを同期させる
		[PunRPC]
		private void RpcSetKinematic(Vector3 touchPos, bool isKinematic)
		{
			// ポジションを同期
			transform.position = new Vector3(touchPos.x,transform.position.y);
			// Kinematicフラグを同期
			GetComponent<Rigidbody2D>().isKinematic = isKinematic;
		}

	}

}