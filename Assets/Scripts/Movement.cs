using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

	// public Transform CharacterTransform;
	public Rigidbody2D CharacterBody;
	public int MovementChange;
	private InputManager CurrentInstance;

	// Use this for initialization
	void Start ()
	{
		CurrentInstance = InputManager.Instance;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float horizontalPosition = CurrentInstance.GetCompDevInputAxis(CompDevInput.DirectionHorizontal);
		float verticalPosition = CurrentInstance.GetCompDevInputAxis(CompDevInput.DirectionVertical);
		Vector3 newVelocity = new Vector3(horizontalPosition, verticalPosition);
		CharacterBody.velocity = newVelocity.normalized * MovementChange;
	}
}
