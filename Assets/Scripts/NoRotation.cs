using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoRotation : MonoBehaviour
{
	public Transform MyTransform;
	void Update ()
	{
		MyTransform.rotation = Quaternion.Euler(Vector3.zero);
	}
}
