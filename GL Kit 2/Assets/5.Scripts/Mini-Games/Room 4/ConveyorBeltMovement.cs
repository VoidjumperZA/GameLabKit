﻿using GameLab;
using UnityEngine;
using System.Collections.Generic;

public class ConveyorBeltMovement : Singleton<ConveyorBeltMovement>
{
	private Vector3 beginSwipePosition = Vector3.zero;
	private Vector3 endSwipePosition = Vector3.zero;

	private const float minimumHeight = 169f;

	public int CurrentPlatformIndex { get; private set; }
	private List<Transform> platforms = new List<Transform>();

	private void Start()
	{
		foreach(Transform child in transform)
		{
			platforms.Add(child);
		}

		DisplayPlatformInformation.Instance.LoadInformation(CurrentPlatformIndex);		
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			beginSwipePosition = Input.mousePosition;
		}

		if (Input.GetMouseButtonUp(0))
		{
			endSwipePosition = Input.mousePosition;

			if (endSwipePosition.y > minimumHeight)
			{
				if (endSwipePosition.x > beginSwipePosition.x)
				{
					Next();
				}
				else if (endSwipePosition.x < beginSwipePosition.x)
				{
					Previous();
				}
			}
		}
	}

	private void Next()
	{
		CurrentPlatformIndex = (CurrentPlatformIndex + 1) % platforms.Count;

		DisplayPlatformInformation.Instance.LoadInformation(CurrentPlatformIndex);
	}

	private void Previous()
	{
		if (CurrentPlatformIndex <= 0)
		{
			CurrentPlatformIndex = platforms.Count - 1;
		}
		else
		{
			CurrentPlatformIndex = (CurrentPlatformIndex - 1) % platforms.Count;
		}

		DisplayPlatformInformation.Instance.LoadInformation(CurrentPlatformIndex);
	}
}