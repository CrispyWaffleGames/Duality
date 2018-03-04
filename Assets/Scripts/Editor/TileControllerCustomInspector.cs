using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileController))]
public class TileControllerCustomInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		TileController myTarget = (TileController)target;

		if (myTarget != null)
		{
			if (myTarget.CurrentTileSprite != null &&
				myTarget.TilePrefab != null)
			{
				Texture2D myTexture = AssetPreview.GetAssetPreview(myTarget.CurrentTileSprite);
				GUILayout.Label(myTexture);
			}
			
			if (GUILayout.Button("Generate Tiles") == true)
			{
				DeleteTiles(myTarget);
				GenerateTiles(myTarget);
			}

			if (GUILayout.Button("Delete Tiles") == true)
			{
				DeleteTiles(myTarget);
			}
		}
	}

	private void GenerateTiles(TileController inTileController)
	{
		if (inTileController != null &&
			inTileController.Level != null &&
			inTileController.TilePrefab != null)
		{
			SpriteRenderer tileSprite = inTileController.TilePrefab.GetComponent<SpriteRenderer>();
			float tileWidth = tileSprite.bounds.size.x;
			float tileHeight = tileSprite.bounds.size.y;

			float startX = -inTileController.Level.LevelWidth / 2f;
			float endx = inTileController.Level.LevelWidth / 2f;

			float startY = -inTileController.Level.LevelHeight / 2f;
			float endY = inTileController.Level.LevelHeight / 2f;

			for (float tileX = startX; tileX < endx; tileX += tileWidth)
			{
				for (float tileY = startY + tileHeight; tileY <= endY; tileY += tileHeight)
				{
					GameObject tileInstance = (GameObject)PrefabUtility.InstantiatePrefab(inTileController.TilePrefab);
					tileInstance.transform.parent = inTileController.transform;
					tileInstance.transform.position = new Vector3(tileX, tileY);
				}
			}
		}
	}

	private void DeleteTiles(TileController inTileController)
	{
		Transform[] children = inTileController.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in children)
		{
			if (child != inTileController.transform)
			{
				GameObject.DestroyImmediate(child.gameObject);
			}
		}
	}
}
