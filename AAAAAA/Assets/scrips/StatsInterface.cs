using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsInfo
{
  public StatsInfo()
  {
    this.numStarsCollected = 0;
    this.distanceTraveled = 0;
  }

  public StatsInfo(int numStarsCollected, float distanceTraveled)
  {
    this.numStarsCollected = numStarsCollected;
    this.distanceTraveled = distanceTraveled;
  }

  public int numStarsCollected;
  public float distanceTraveled;
}

public class StatsInterface : MonoBehaviour
{

  public Text numStarsCollected;
  public Text constelationsCollected;
  public Text totalPoints;
  public Text distanceTraveled;

  public void RecieveUpdate(StatsInfo info)
  {
    numStarsCollected.text = "Number of Stars: " + info.numStarsCollected.ToString();
    constelationsCollected.text = "Constelations: " + "0";
    distanceTraveled.text = "Distance Traveled: " + ((int)info.distanceTraveled).ToString() + " feet!";
    totalPoints.text = "Total Points: " + info.numStarsCollected.ToString();

    // Serialize best somehow
    // Load best somehow

  }
	// Use this for initialization
	void Start ()
  {
		
	}
	
	// Update is called once per frame
	void Update ()
  {
		
	}
}
