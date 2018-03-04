using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	public Rigidbody2D CharacterBody;
	public int MovementChange;
	public SpriteRenderer Sprite;
	public Animator SpriteAnimator;
	private InputManager CurrentInstance;

	void Start ()
	{
		CurrentInstance = InputManager.Instance;
		GameCamera.AddInstanceToFollow(transform);
	}
	
	void Update ()
	{
		float horizontalPosition = CurrentInstance.GetCompDevInputAxis(CompDevInput.DirectionHorizontal);
		float verticalPosition = CurrentInstance.GetCompDevInputAxis(CompDevInput.DirectionVertical);
		Vector3 newVelocity = new Vector3(horizontalPosition, verticalPosition);
		CharacterBody.velocity = newVelocity.normalized * MovementChange;

		Vector2 unitVector = new Vector2(horizontalPosition, verticalPosition).normalized;

		int direction = -1;

		if (SpriteAnimator != null)
		{
			direction = SpriteAnimator.GetInteger("Direction");
		}

		if (unitVector.magnitude != 0)
		{
			float angle = Mathf.Acos(unitVector.x) * Mathf.Rad2Deg;
			if (unitVector.y < 0)
			{
				angle *= -1;
			}

			if (angle < 0)
			{
				angle += 360;
			}

			if (angle >= 0 && angle <= 45)
			{
				direction = 0;
			}
			else if (angle > 45 && angle <= 135)
			{
				direction = 1;
			}
			else if (angle > 135 && angle <= 225)
			{
				direction = 2;
			}
			else if (angle > 225 && angle <= 315)
			{
				direction = 3;
			}
			else if (angle > 225 && angle <= 360)
			{
				direction = 0;
			}
		}

		if (SpriteAnimator != null)
		{
			SpriteAnimator.SetBool("Moving", unitVector.magnitude != 0f);
			SpriteAnimator.SetInteger("Direction", direction);
		}

		if (Sprite != null)
		{
			if (horizontalPosition < 0)
			{
				Sprite.flipX = true;
			}
			else if (horizontalPosition > 0)
			{
				Sprite.flipX = false;
			}
		}
	}
}
