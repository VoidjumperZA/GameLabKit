﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameLab;
using Room3;
using UnityEngine;

namespace Room3
{
	public class TileGrid : Singleton<TileGrid>
	{
		public bool HasInteractedWithTile => lastInteractedWithTile != null;

		// TODO: CLeanup current level management
		public Level CurrentLevel
		{
			get => levels[currentLevelIndex];
			set => currentLevelIndex = value != null ? Array.IndexOf(levels, value) : -1;
		}

		public Level.ColorSettings CurrentLevelSettings => CurrentLevel != null && CurrentLevel.HasCustomColorSettings ? CurrentLevel.CustomColorSettings : defaultLevelSettings;

		[SerializeField] private TileSpriteSettings tileSpriteSettings;

		[SerializeField] private TileController tileControllerPrefab = null;

		[SerializeField] private Level.ColorSettings defaultLevelSettings = Level.ColorSettings.Default;
		[SerializeField] private Level[] levels = new Level[0];

		private int currentLevelIndex = -1;

		private bool canBeInteractedWith = false;

		private TileLayer mainLayer = null;
		private TileLayer bridgeLayer = null;

		private TileController lastInteractedWithTile = null;

		private HashSet<Tile.Group> finishedGroups = new HashSet<Tile.Group>();

		protected override void Awake()
		{
			base.Awake();

			if (levels.Length == 0)
			{
				Debug.LogWarning("There are no levels set up in Room 3!");
				return;
			}

			//SpawnLevel(levels[0]);
			NextLevel();
		}

		public void SetGridInteractable(bool canInteractedWith)
		{
			canBeInteractedWith = canInteractedWith;
		}

		private void NextLevel()
		{
			if (currentLevelIndex == levels.Length - 1)
			{
				print("No more levels");
				NextRoomEvent newInfo = new NextRoomEvent();
				EventManager.Instance.RaiseEvent(newInfo);
				return;
			}

			++currentLevelIndex;
			print(currentLevelIndex);
			SetGridInteractable(false);
			SpawnLevel(levels[currentLevelIndex]);
		}

		private void PreviousLevel()
		{
			if (currentLevelIndex == 0)
			{
				print("No previous levels");
			}

			--currentLevelIndex;

			SpawnLevel(levels[currentLevelIndex]);
		}

		private void SpawnLevel(Level level)
		{
			DestroySpawnedLevel();

			mainLayer = new TileLayer(level.Rows, level.Cols);
			bridgeLayer = new TileLayer(level.Rows, level.Cols);
			Level.ColorSettings levelColorSettings = CurrentLevelSettings;
			Color32[] levelPixelData = level.LevelTexture.GetPixels32();

			float anchorStepPerColumn = 1.0f / level.Cols;
			float anchorStepPerRow = 1.0f / level.Rows;

			for (int i = 0; i < levelPixelData.Length; ++i)
			{
				Color32 pixel = levelPixelData[i];
				int row = i / level.Rows;
				int col = i % level.Cols;

				if (pixel.CompareRGB(levelColorSettings.BridgeTileColor))
				{
					mainLayer.Tiles[row, col].AllowedConnectionDirections = Tile.ConnectionDirection.East | Tile.ConnectionDirection.West;
					bridgeLayer.Tiles[row, col].AllowedConnectionDirections = Tile.ConnectionDirection.North | Tile.ConnectionDirection.South;
				}

				Tile tileData = mainLayer.Tiles[row, col];
				Tile bridgeLayerTileData = bridgeLayer.Tiles[row, col];

				bridgeLayerTileData.TileGroup = levelColorSettings.GetTileGroupFromColor(pixel);
				bridgeLayerTileData.TileType = levelColorSettings.GetTileTypeFromColor(pixel);
				tileData.TileGroup = levelColorSettings.GetTileGroupFromColor(pixel);
				tileData.TileType = levelColorSettings.GetTileTypeFromColor(pixel);
				TileController tileController = SpawnTileController(tileData, anchorStepPerColumn, anchorStepPerRow, tileSpriteSettings);
				tileController.Image.color = pixel;
			}
		}

		private TileController SpawnTileController(Tile tileData, float anchorStepPerColumn, float anchorStepPerRow, TileSpriteSettings tileSprites)
		{
			TileController tileController = Instantiate(tileControllerPrefab, Vector3.zero, Quaternion.identity, CachedTransform);

			tileController.name = $"Tile {tileData.Row}, {tileData.Col}";

			tileController.TileData = tileData;
			tileController.TileData.SpriteSettings = tileSprites;

			tileController.OnInteractedWith += OnTileInteractedWith;
			tileController.OnFinishedInteractingAt += OnFinishedInteractingAtTile;

			tileController.CachedRectTransform.anchorMin = new Vector2(tileData.Col * anchorStepPerColumn, tileData.Row * anchorStepPerRow);
			tileController.CachedRectTransform.anchorMax = new Vector2((tileData.Col + 1) * anchorStepPerColumn, (tileData.Row + 1) * anchorStepPerRow);

			tileController.CachedRectTransform.offsetMin = tileController.CachedRectTransform.offsetMax = Vector3.zero;
			tileController.CachedRectTransform.localPosition = new Vector3(tileController.CachedRectTransform.localPosition.x, tileController.CachedRectTransform.localPosition.y, 0.0f);
			return tileController;
		}

		private void DestroySpawnedLevel()
		{
			foreach (Transform spawnedTile in CachedTransform)
			{
				Destroy(spawnedTile.gameObject);
			}

			//CurrentLevel = null;
			mainLayer = null;
			bridgeLayer = null;

			lastInteractedWithTile = null;
			finishedGroups.Clear();
		}

		private void OnTileInteractedWith(TileController tile)
		{
			if (!canBeInteractedWith)
			{
				return;
			}

			if (tile == lastInteractedWithTile)
			{
				return;
			}

			Tile tileData = tile.TileData;
		
			if (!HasInteractedWithTile)
			{
				TryResumePathFrom(tile);
				return;
			}

			if (TryRemoveTileConnectionsAfter(tile))
			{
				return;
			}

			if (finishedGroups.Contains(lastInteractedWithTile.TileData.TileGroup))
			{
				return;
			}

			if (!tileData.TryConnectTo(lastInteractedWithTile.TileData))
			{
				if (bridgeLayer.Tiles[tileData.Row, tileData.Col].TileType == Tile.Type.Connection)
				{
					InteractWithBridge(bridgeLayer.Tiles[tileData.Row, tileData.Col]);
				}
				return;
			}

			ValidatePath(tile, lastInteractedWithTile);

			if (tile.TileData.Row == lastInteractedWithTile.TileData.Row)
			{
				tile.ChangeSprite(tileSpriteSettings.TubeWestToEast);
			}
			else
			{
				tile.ChangeSprite(tileSpriteSettings.TubeNorthToSouth);
			}
			lastInteractedWithTile = tile;

			UpdateWinStatus();
		}

		private void InteractWithBridge(Tile bridge)
		{
			if (bridge == lastInteractedWithTile)
			{
				return;
			}
		}

		private void ValidatePath(TileController currentTile, TileController lastTile)
		{

			TilePath path = mainLayer.CalculatePathForGroup(lastTile.TileData.TileGroup);
			TilePath bridgeLayerPath = bridgeLayer.CalculatePathForGroup(lastTile.TileData.TileGroup);
			if ((!path.Tiles.Contains(currentTile.TileData) || !path.Tiles.Contains(lastTile.TileData)))
			{
				return;
			}
			if (path.Tiles.IndexOf(lastTile.TileData) > 0)
			{
				Tile previousToLastTile = path.Tiles[path.Tiles.IndexOf(lastTile.TileData) - 1];
				CheckForCorners(currentTile, lastTile, previousToLastTile);
			}
		}

		private void CheckForCorners(TileController currentTile, TileController lastTile, Tile previousToLastTile)
		{
			Tile currentTileTileData = currentTile.TileData;
			Tile lastTileTileData = lastTile.TileData;

			// current ptl have both different x and y values 
			bool currentXSmallerThanPTL = (currentTileTileData.Col < previousToLastTile.Col);
			bool currentYSmallerThanPTL = (currentTileTileData.Row < previousToLastTile.Row);

			bool currentOnSameRowAsL = (currentTileTileData.Row == lastTileTileData.Row);

			if (lastTileTileData.Row == currentTileTileData.Row && lastTileTileData.Row == previousToLastTile.Row && lastTileTileData.Col != currentTileTileData.Row)
			{
				// there is an X difference but no Y difference
				lastTile.ChangeSprite(tileSpriteSettings.TubeWestToEast);
				currentTile.ChangeSprite(tileSpriteSettings.TubeWestToEast);
				return;
			}
			if (lastTileTileData.Col == currentTileTileData.Col && lastTileTileData.Col == previousToLastTile.Col && lastTileTileData.Row != currentTileTileData.Row)
			{
				// there is an X difference but no Y difference
				lastTile.ChangeSprite(tileSpriteSettings.TubeNorthToSouth);
				currentTile.ChangeSprite(tileSpriteSettings.TubeNorthToSouth);
				return;
			}

			if (currentTileTileData.Col != previousToLastTile.Col && currentTileTileData.Row != previousToLastTile.Row)
			{
				if (currentXSmallerThanPTL && currentYSmallerThanPTL)
				{
					if (currentOnSameRowAsL)
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeWestToNorth);
					}
					else
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeEastToSouth);
					}
					return;
				}

				if (currentXSmallerThanPTL && !currentYSmallerThanPTL)
				{
					if (currentOnSameRowAsL)
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeWestToSouth);
					}
					else
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeEastToNorth);
					}
					return;
				}

				if (!currentXSmallerThanPTL && currentYSmallerThanPTL)
				{
					if (currentOnSameRowAsL)
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeEastToNorth);
					}
					else
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeWestToSouth);
					}
					return;
				}

				if (!currentXSmallerThanPTL && !currentYSmallerThanPTL)
				{
					if (currentOnSameRowAsL)
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeEastToSouth);
					}
					else
					{
						lastTile.ChangeSprite(tileSpriteSettings.TubeWestToNorth);
					}
					return;
				}
			}
		}

		private void OnFinishedInteractingAtTile(TileController tile)
		{
			lastInteractedWithTile = null;
		}

		private bool TryResumePathFrom(TileController tile)
		{
			if (HasInteractedWithTile)
			{
				return false;
			}

			if (tile.TileData.TileGroup == Tile.Group.Ungrouped)
			{
				return false;
			}

			if (tile.TileData.TileType == Tile.Type.EndPoint)
			{
				return false;
			}

			RemoveTileConnectionsAfter(tile);
			return true;
		}



		private bool TryRemoveTileConnectionsAfter(TileController tile)
		{
			if (tile.TileData.TileGroup == Tile.Group.Ungrouped)
			{
				return false;
			}

			TilePath interactedTileGroupPath = mainLayer.CalculatePathForGroup(tile.TileData.TileGroup);

			int interactedTilePathIndex = interactedTileGroupPath.Tiles.IndexOf(tile.TileData);
			int lastInteractedWithTilePathIndex = interactedTileGroupPath.Tiles.IndexOf(lastInteractedWithTile.TileData);

			if (interactedTilePathIndex < 0 || interactedTilePathIndex >= lastInteractedWithTilePathIndex)
			{
				return false;
			}

			RemoveTileConnectionsAfter(tile);

			return true;
		}

		private void RemoveTileConnectionsAfter(TileController tile)
		{
			tile.TileData.RemoveTileConnectionsAfterThis();
			finishedGroups.Remove(tile.TileData.TileGroup);

			lastInteractedWithTile = tile;
		}

		private void UpdateWinStatus()
		{
			Tile tileData = lastInteractedWithTile.TileData;

			if (tileData.TileType != Tile.Type.EndPoint)
			{
				return;
			}

			finishedGroups.Add(tileData.TileGroup);

			foreach (Tile.Group tileGroup in CurrentLevelSettings.TileGroups)
			{
				if (!finishedGroups.Contains(tileGroup))
				{
					return;
				}
			}

			NextLevel();
			Debug.Log("Level complete!");
		}
	}
}