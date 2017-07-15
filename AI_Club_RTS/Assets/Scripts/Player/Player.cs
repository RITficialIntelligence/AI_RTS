﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * @author Paul Galatic
 * 
 * Class designed to handle all state encapsulated in a Player, such as name,
 * current number of units, current amount of gold, et cetera.
 */
public class Player : MonoBehaviour {

    // Public constants
    public static string TEAM_1 = "Dylante";
    public static string TEAM_2 = "AI_Team";

    // Private Constants
    private const float PASS_INFO_RATE = 1f;
    private const int MAX_UNITS = 20;

    // Private fields

    // The AI that controls this Player, if any
    private PlayerAI brain;
    // Misc state
    private PlayerInfo info;
    private Team team;
    private City toSpawnAt;
    private string toSpawn;
    private int toSpawnCost;
    private int goldAmount;

    /// <summary>
    /// Creates and returns a Player instance.
    /// </summary>
    /// <param name="hasBrain">Whether or not this Player has a brain.</param>
    /// <returns>A Player instance.</returns>
    public static Player MakePlayer(bool hasBrain, Team team)
    {
        GameObject obj = new GameObject("Player Instance");
        Player player = obj.AddComponent<Player>();
        if (hasBrain)
        {
            player.Brain = obj.AddComponent<PlayerAI_Basic>();
            player.Brain.Body = player;
        }
        player.Team = team;
        player.Activate();
        return player;
    }

    /// <summary>
    /// Initializing the Team first because other functionality relies on it.
    /// This is bad code practice and should be fixed. FIXME.
    /// </summary>
    public virtual void Awake()
    {
        info = new PlayerInfo();
    }

    // Use this for initialization
    public virtual void Activate () {
        // Handle private fields
        goldAmount = 0;

        // Handle Coroutines
        if (brain != null)
        {
            StartCoroutine(PassInfo());
        }
    }

    /// <summary>
    /// Stops coroutines associated with this Player.
    /// </summary>
    public virtual void Deactivate()
    {
        if (brain != null)
        {
            StopCoroutine(PassInfo());
        }
    }

    /// <summary>
    /// Sets the unit to spawn. Throws an exception on an invalid name being 
    /// passed.
    /// </summary>
    /// <param name="unitIdentity">The name of the unit to spawn, based on 
    /// Unit.NAME.</param>
    public void SetUnitToSpawn(string unitIdentity)
    {
        toSpawn = unitIdentity;

        switch (unitIdentity)
        {
            case Infantry.IDENTITY:
                toSpawnCost = Infantry.COST;
                break;
            case Tank.IDENTITY:
                toSpawnCost = Tank.COST;
                break;
            default:
                throw new KeyNotFoundException("SetUnitToSpawn given invalid string");
        }
    }

    /// <summary>
    /// Sets the city at which the next unit will be spawned.
    /// </summary>
    /// <param name="city"></param>
    public void SetCityToSpawnAt(City city)
    {
        toSpawnAt = city;
    }

    /// <summary>
    /// Spawns a unit based on toSpawn, if the Player has enough gold.
    /// </summary>
    public void SpawnUnit()
    {
        if (team.mobiles.Count >= MAX_UNITS) { return; }

        if (goldAmount > toSpawnCost)
        {
            Debug.Assert(toSpawnCost > 0);
            goldAmount -= toSpawnCost;

            MobileUnit newUnit = Utils.IdentityToGameObject(toSpawn);
            newUnit.Team = team;
            newUnit.transform.position = toSpawnAt.SpawnPoint.transform.position;
            newUnit.SetName(newUnit.UnitName + team.mobiles.Count.ToString());
            team.mobiles.Add(newUnit);
            newUnit.gameObject.SetActive(true);
            newUnit.Activate();
        }
    }

    /// <summary>
    /// Returns this player's team.
    /// </summary>
    public Team Team
    {
        get { return team; }
        set { team = value; }
    }

    /// <summary>
    /// Returns the AI of this player, or null if it has no brain.
    /// </summary>
    public PlayerAI Brain
    {
        get { return brain; }
        set { brain = value; }
    }

    /// <summary>
    /// Returns the player's current amount of gold.
    /// </summary>
    public int Gold
    {
        get { return goldAmount; }
        set { goldAmount = value; }
    }

    /// <summary>
    /// Passes PlayerInfo to this Player's brain.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PassInfo()
    {
        info.team = team;
        info.goldAmount = goldAmount;
        brain.UpdateInfo(info);
        yield return new WaitForSeconds(PASS_INFO_RATE);
    }

}
