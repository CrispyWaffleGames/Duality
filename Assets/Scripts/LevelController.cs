using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
	public float LevelWidth;
	public float LevelHeight;

	public GameObject WallPrefab;

	void Start() 
	{
		if (WallPrefab != null)
		{
			SpriteRenderer wallSprite = WallPrefab.GetComponent<SpriteRenderer>();
			float wallWidth = wallSprite.bounds.size.x;
			float wallHeight = wallSprite.bounds.size.y;

			float startX = -LevelWidth / 2f;
			float endx = LevelWidth / 2f;

			float startY = -LevelHeight / 2f;
			float endY = LevelHeight / 2f;

			for (float wallX = startX; wallX <= endx; wallX += wallWidth)
			{
				GameObject.Instantiate(WallPrefab, new Vector3(wallX, startY), Quaternion.identity);
				GameObject.Instantiate(WallPrefab, new Vector3(wallX, endY), Quaternion.identity);
			}

			for (float wallY = startY; wallY <= endY; wallY += wallHeight)
			{
				GameObject.Instantiate(WallPrefab, new Vector3(startX, wallY), Quaternion.identity);
				GameObject.Instantiate(WallPrefab, new Vector3(endx, wallY), Quaternion.identity);
			}
		}
	}
}
