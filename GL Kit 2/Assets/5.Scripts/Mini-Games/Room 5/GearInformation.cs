﻿using System.Collections;
using UnityEngine;

public enum GearType
{
	Exploration,
	Fantasy,
	Creativity,

	Communication,
	Cooperation,
	Competition,

	Goals,
	Obstacles,
	Stategy,

	Learing,
	Rhythm,
	Collection
};

public class GearInformation : MonoBehaviour
{
	[SerializeField] private GearType gearType = default;
	public GearType GetGearType => gearType;

	[SerializeField] private bool invertRotation = false;
	[SerializeField] private int rotationSpeed = 50;
	[SerializeField] private float timeForRotationToStop = 2f;
	[HideInInspector] public bool isAbleToRotate = false;

	public void StopGearRotationMethod()
	{
		StartCoroutine(StopGearRotation());
	}

	private IEnumerator StopGearRotation()
	{
		yield return new WaitForSeconds(timeForRotationToStop);
		isAbleToRotate = false;
	}

	private void Update()
	{
		if (isAbleToRotate)
		{
			if (!invertRotation)
			{
				transform.Rotate(Vector3.forward * (Time.deltaTime * rotationSpeed));
			}
			else
			{
				transform.Rotate(Vector3.back * (Time.deltaTime * rotationSpeed));
			}
		}
	}
}