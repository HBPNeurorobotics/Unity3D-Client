﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPickup : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.name.Equals("Pickup"))
		{
			Debug.Log(transform.name);
			Pickup pickup = other.gameObject.GetComponent<Pickup>();
			if (transform.name.Equals("RespawnTrigger"))
			{
				pickup.Reset();
			}
			else if (transform.name.Equals("LeftTrigger") || transform.name.Equals("RightTrigger"))
			{
				Debug.Log("Hand Contact");
				pickup.HandContact(transform.GetChild(0));
			}
		}
		
	}
}
