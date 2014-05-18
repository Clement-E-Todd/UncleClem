using UnityEngine;
using System.Collections;

public abstract class UCGravity : MonoBehaviour {

	public int priority = 0;

	public abstract Vector3 GetGravityForObject(UCObject target);

}
