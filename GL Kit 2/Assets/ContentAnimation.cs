﻿using System;
using GameLab;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class ContentAnimation : BetterMonoBehaviour
{
	public event Action<bool> OnAnimationIsPlaying;

	private Animator contentAnimator = null;
	private Menu contentOwner = null;
	private bool didFlyOut = true; // Start of as true because the menu is closed by default

	[Header("Debug")]
	[SerializeField] UnityEngine.UI.Text debugText;

	private void Awake()
	{
		contentAnimator = GetComponent<Animator>();
		contentOwner = GetComponentInParent<Menu>();

		contentOwner.Opened += OnContentOwnerOpened;
		contentOwner.Closing += OnContentOwnerClosing;
	}

	private void OnDestroy()
	{
		contentOwner.Opened -= OnContentOwnerOpened;
		contentOwner.Closing -= OnContentOwnerClosing;
	}

	private void OnContentOwnerOpened(Menu menu)
	{
		didFlyOut = false;
		contentAnimator.SetTrigger("FlyIn");
	}

	private bool OnContentOwnerClosing(Menu menu)
	{
		if (!didFlyOut)
		{
			debugText.text = Environment.StackTrace;
			contentAnimator.SetTrigger("FlyOut");
			return false;
		}

		return true;
	}

	private void OnFlyOutCompleted()
	{
		didFlyOut = true;
		contentOwner.Close();
	}

	private void OnAnimationStart()
	{
		OnAnimationIsPlaying?.Invoke(true);
		TEST_PlayingAnimation();
	}

	private void OnAnimationEnd()
	{
		OnAnimationIsPlaying?.Invoke(false);
	}

	private void TEST_PlayingAnimation()
	{
		AnimatorClipInfo[] clipInfos = contentAnimator.GetCurrentAnimatorClipInfo(0);

		foreach (AnimatorClipInfo clipInfo in clipInfos)
		{
			Debug.Log($"Playing clip: {clipInfo.clip}...");
		}
	}
}