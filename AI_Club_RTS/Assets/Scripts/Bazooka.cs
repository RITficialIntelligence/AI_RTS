﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bazooka : Unit {
	// Author: Ben Fairlamb
	// Purpose: Bazooka unit
	// Limitations: Meh

	// Constructor
	public Bazooka(string team)
		: base(team, 100, "infantry", "ap", 100, 100)
	{
	}

	// Methods
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Attack the specified target.
	/// </summary>
	/// <param name="target">Target to attack.</param>
	public override void Attack(Unit target)
	{
	}

	/// <summary>
	/// Take specified damage.
	/// </summary>
	/// <param name="dmg">Damage to Take.</param>
	/// <param name="amount">Amount.</param>
	public override void TakeDmg(int amount)
	{
	}

	/// <summary>
	/// Kill this instance.
	/// </summary>
	public override void Kill()
	{
	}
}