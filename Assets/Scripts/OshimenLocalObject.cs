using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;

namespace Tower.Object
{
	public sealed class OshimenLocalObject : MonoBehaviour {


		private void Start () 
		{
			this.UpdateAsObservable()
				.Where(_ => Mathf.Abs(transform.position.x) > 4.0f || Mathf.Abs(transform.position.y) > 8.0f)
				.Subscribe(_ =>
				{
					Debug.Log("DestroyObject");
					Destroy(gameObject);
					return;
				});
		}
		
	}
}
