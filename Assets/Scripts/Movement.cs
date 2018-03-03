using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

	public Transform CharacterTransform;
	private int MovementChange = 3;
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
		Vector3 newPosition = new Vector3(horizontalPosition, verticalPosition);
		CharacterTransform.position += newPosition * MovementChange;
	}
}
