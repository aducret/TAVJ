﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkView : MonoBehaviour
{
	public int Id { get; set; }

	private void Start() 
	{
		
	}

	private void Update() 
	{
		
	}

	public void SetPosition(Vector2 position)
	{
		transform.position = position;
	}
}
