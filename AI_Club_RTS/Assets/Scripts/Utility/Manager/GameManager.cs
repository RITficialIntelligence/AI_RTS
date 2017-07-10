﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // Public constants
    // The first created player, which will always be the main player.
    public static Player PLAYER;

    // Public fields
    public City cityPrefab;

    private const string CITY_SPAWN_TAG = "CitySpawn";
    private const float GOLD_INCREMENT_RATE = 0.1f; // higher is slower
    private const int MAX_MONEY = 999; // richness ceiling
    private const int NUM_AI_PLAYERS = 1;

    private static GameObject[] citySpawnPoints;
    private static List<Team> teams;
    private static List<Player> players;

    private Camera m_Camera;
    private RTS_Terrain m_Terrain;
    private bool oneTeamLeft = false;

    /// <summary>
    /// Pauses or unpauses the game, depending on the current timescale.
    /// </summary>
    public void TogglePause()
    {
        // Set to 1 if 0, and 0 if 1.
        Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
    }

    /// <summary>
    /// Sets the new destination for the unit, if the unit is of the player's
    /// team.
    /// </summary>
    /// <param name="terrain">The terrain, which was right clicked such to 
    /// invoke this method.</param>
    public void SetNewDestination(HashSet<Unit> selectedUnits, RTS_Terrain terrain)
    {
        if (selectedUnits == null) { return; }
        RaycastHit hit;
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
        Team playerTeam = PLAYER.Team;
        if (Physics.Raycast(ray, out hit, terrain.ignoreAllButTerrain))
        {
            // Set the destination of all the units
            foreach (Unit u in selectedUnits)
            {
                if (u.Team == playerTeam)
                    u.Destination = hit.point;
            }
        }
    }

    /// <summary>
    /// Transfers a city from the control of one Team to another, and destroys 
    /// a team if they have no remaining cities.
    /// </summary>
    /// <param name="city">The city to be transferred.</param>
    /// <param name="newTeam">The team the city will be transferred to.</param>
    public void TransferCity(City city, Team newTeam)
    {
        city.Team.cities.Remove(city);
        newTeam.cities.Add(city);

        // Don't chance the city's team until the end
        city.Team = newTeam;

        // Have all the team's cities been eliminated?

    }

    private void Awake()
    {
        m_Camera = Camera.main;

        m_Terrain = GameObject.FindGameObjectWithTag(RTS_Terrain.TERRAIN_TAG).GetComponent<RTS_Terrain>();
        citySpawnPoints = GameObject.FindGameObjectsWithTag(CITY_SPAWN_TAG);

        Debug.Assert(m_Terrain != null);
        Debug.Assert(citySpawnPoints != null && citySpawnPoints.Length > 1);

        City curr;

        teams = new List<Team>
        {
            new Team("Dylante", Color.cyan),
            new Team("AI Team", Color.red)
        };

        players = new List<Player>
        {
            Player.MakePlayer(false, teams[0]),
            Player.MakePlayer(true, teams[1])
        };

        // Every player should have at least one city, and we need places to 
        // put them.
        Debug.Assert(citySpawnPoints.Length >= NUM_AI_PLAYERS + 1);
        // The number of cities to instantiate is capped both by the number of
        // places to spawn them as well as the total number of players in the 
        // game. 
        for (int x = 0; ((x < citySpawnPoints.Length) && (x < NUM_AI_PLAYERS + 1)); x++)
        {
            curr = City.MakeCity(teams[x], cityPrefab, citySpawnPoints[x].transform.position);
            teams[x].cities.Add(curr);

        }

        PLAYER = players[0];

        // Set main camera to be behind the player's first city
        CenterCameraBehindPosition(teams[0].cities[0].transform.position);
    }

    private void Start()
    {
        StartCoroutine(GameLoop());

    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        StartCoroutine(GameLoop());
    }

    private IEnumerator RoundStarting()
    {
        yield return new WaitForSeconds(1);
    }

    private IEnumerator RoundPlaying()
    {
        // Handle IEnumerators
        StartCoroutine(IncrementGold());
        yield return new WaitUntil(() => oneTeamLeft == true);
    }

    private IEnumerator RoundEnding()
    {
        StopCoroutine(IncrementGold());
        yield return new WaitForSeconds(1);
    }

    /// <summary>
    /// Updates the current gold amount, of all players reflecting income from
    /// cities.
    /// </summary>
    private IEnumerator IncrementGold()
    {
        while (true)
        {

            foreach (Player p in players)
            {
                foreach (City c in p.Team.cities)
                {
                    p.Gold += c.IncomeLevel;
                    if (p.Gold > MAX_MONEY)
                    {
                        p.Gold = MAX_MONEY;
                    }
                }
            }

            yield return new WaitForSeconds(GOLD_INCREMENT_RATE);

        }
    }

    private void CenterCameraBehindPosition(Vector3 position)
    {
        // Move the camera over the position
        Vector3 dest = position;
        // Rotate the camera toward the center of the map
        Vector3 towardCenter = dest - m_Terrain.transform.position;
        Quaternion rotation = Quaternion.LookRotation(towardCenter);
        rotation.x = 0; rotation.z = 0; // x and z aren't relevant
        // Set values so that the orientation of the camera is unchanged
        dest = rotation * dest;
        dest.y = m_Camera.transform.position.y;
        dest.z += 200; // offset to account for angle of camera
        
        m_Camera.transform.position = dest;
    }
    
}
