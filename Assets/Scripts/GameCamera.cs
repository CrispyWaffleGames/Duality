using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public Transform CameraTransform;
	public Camera MainCamera;

	private static GameCamera _Instance;

	private List<Transform> _InstancesToFollow;
	private Vector2 CameraBoundsMin;
	private Vector2 CameraBoundsMax;

	void Awake()
	{
		_InstancesToFollow = new List<Transform>();
		_Instance = this;
	}

	void Start()
	{
		// set the desired aspect ratio (the values in this example are
		// hard-coded for 16:9, but you could make them into public
		// variables instead so you can set them at design time)
		float targetaspect = 16.0f / 9.0f;

		// determine the game window's current aspect ratio
		float windowaspect = (float)Screen.width / (float)Screen.height;

		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;

		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{
			Rect rect = MainCamera.rect;

			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;

			MainCamera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;

			Rect rect = MainCamera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;

			MainCamera.rect = rect;
		}
	}

	public static void AddInstanceToFollow(Transform inInstance)
	{
		_Instance._InstancesToFollow.Add(inInstance);
	}

	void Update()
	{
		if (CameraTransform != null)
		{
			Vector3 average = Vector3.zero;

			foreach (Transform instanceToFollow in _InstancesToFollow)
			{
				average += instanceToFollow.position;
			}

			average /= _InstancesToFollow.Count;

			Rect levelRect = LevelController.GetLevelRect();
			Rect cameraRect = GetCameraRect();
			float cameraHalfWidth = cameraRect.width / 2f;
			float cameraHalfHeight = cameraRect.height / 2f;

			if ((average.x - cameraHalfWidth) < levelRect.min.x)
			{
				average.x = levelRect.min.x + cameraHalfWidth;
			}
			else if ((average.x + cameraHalfWidth) > levelRect.max.x)
			{
				average.x = levelRect.max.x - cameraHalfWidth;
			}

			if ((average.y - cameraHalfHeight) < levelRect.min.y)
			{
				average.y = levelRect.min.y + cameraHalfHeight;
			}
			else if ((average.y + cameraHalfHeight) > levelRect.max.y)
			{
				average.y = levelRect.max.y - cameraHalfHeight;
			}

			average.z = CameraTransform.position.z;

			CameraTransform.position = average;
		}
	}

	private Rect GetCameraRect()
	{
		float cameraVertExtent = MainCamera.orthographicSize;
		float cameraHorzExtent = cameraVertExtent * Screen.width / Screen.height;

		float cameraX = CameraTransform.position.x;
		float cameraY = CameraTransform.position.y;

		CameraBoundsMin = new Vector2(cameraX - cameraHorzExtent, cameraY - cameraVertExtent);
		CameraBoundsMax = new Vector2(cameraX + cameraHorzExtent, cameraY + cameraVertExtent);

		Rect cameraRect = new Rect();

		cameraRect.min = CameraBoundsMin;
		cameraRect.max = CameraBoundsMax;

		return cameraRect;
	}
}
