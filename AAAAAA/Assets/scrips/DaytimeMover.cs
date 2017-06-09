using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DaytimeMover : MonoBehaviour
{
  [Header("PrefabPointers")]
  public GameObject star;
  public GameObject obstacle;
  public GameObject endGameStats;
  public GameObject starParticle;

  [Header("ObjectPointers")]
  public GameObject background;
  public GameObject timer;
  public Text liveScorekeeper;
  public GameObject canvas;

  [Header("Environment Items")]
  public List<GameObject> environmentItems = new List<GameObject>();

  [Header("Window Size Settings")]
  public float xSize;
  public float yMin;
  public float yMax;

  [Header("Object Settings")]
  public float starObstacleRatio;

  [Header("Day Length Settings")]
  public float initialDayLength;
  public float timeGainedPerStar;

  [Header("Starting timing settings")]
  public float initialSpeed;
  public float initialEnvironmentSpeed;
  public float initialEnvSpawnTime;
  public float initialSpawnTime;

  [Header("Progression Settings")]
  public float streakBonusMultiplier;
  public float environmentMultiplier;
  public float itemSpawnVariance;
  public float envSpawnVariance;
  public float progressionSpeed;


  float moveSpeed, environmentSpeed, spawnTime, dayTimeLeft;

  int clickStreak;
  int misclickStreak;

  //Stats
  StatsInfo stats;

  bool gameOver = false;

  List<GameObject> currentEnvItems = new List<GameObject>();
  List<GameObject> unclicked = new List<GameObject>();
  List<GameObject> clicked = new List<GameObject>();

  // Use this for initialization
  void Start()
  {
    stats = new StatsInfo();
    
    dayTimeLeft = initialDayLength;
    moveSpeed = initialSpeed;
    environmentSpeed = initialEnvironmentSpeed;
    spawnTime = initialSpawnTime;
    spawnTimer = spawnTime;
    envSpawnTimer = initialEnvSpawnTime;

    if (itemSpawnVariance < 1) itemSpawnVariance = 1;
    if (envSpawnVariance < 1) envSpawnVariance = 1;
  }

  public void ResetLevel()
  {
    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
  }

  // Modify this function to do the things we want on end
  void EndRun()
  {
    dayTimeLeft = 0;

    gameOver = true;

    GameObject endStats = Instantiate(endGameStats);
    endStats.transform.SetParent(canvas.transform, false);
    endStats.transform.localPosition = new Vector3();

    endStats.GetComponentInChildren<Button>().onClick.AddListener(delegate { ResetLevel(); });

    endStats.SendMessage("RecieveUpdate", stats);
  }

  void PerformMove()
  {
    float moveAmount = moveSpeed * Time.deltaTime;

    foreach(GameObject obj in unclicked)
    {
      obj.transform.position += new Vector3(-moveAmount, 0, 0);

      if(obj.transform.position.x <= 0)
      {
        unclicked.Remove(obj);

        if(obj.GetComponent<Star>() != null)
          ResetStreak();

        Destroy(obj);
      }
    }
    

    foreach (GameObject obj in clicked)
    {
      obj.transform.position += new Vector3(-moveAmount, 0, 0);

      if (obj.transform.position.x <= 0)
      {
        clicked.Remove(obj);
        

        Destroy(obj);
      }
    }

    float envMove = environmentSpeed * Time.deltaTime;

    stats.distanceTraveled += envMove;

    foreach (GameObject obj in currentEnvItems)
    {
      obj.transform.position += new Vector3(-envMove, 0, 0);

      if (obj.transform.position.x <= -10)
      {
        currentEnvItems.Remove(obj);
        Destroy(obj);
      }
    }
  }

  void ClickParse(GameObject obj)
  {
    if(obj == null)
    {
      MisClick();
      return;
    }

    bool found = false;
    foreach(var objInClicked in unclicked)
    {
      if(objInClicked == obj)
      {
        found = true;
        break;
      }
    }
    if (!found)
    {
      MisClick();
      return;
    }

    obj.transform.position += new Vector3(0, 0, 10);
    obj.GetComponent<SpriteRenderer>().color -= new Color(0.5f, 0.5f, 0.5f,0);

    if(obj.GetComponent<Star>() != null)
    {
      ClickStar(obj);
    }
    if(obj.GetComponent<Obstacle>() != null)
    {
      ClickObstacle(obj);
    }
  }

  void ClickObject(GameObject obj)
  {
    unclicked.Remove(obj);
    clicked.Add(obj);
  }

  int soundIndex = 0;
  const int MAX_SOUND = 13; // Hardcoded
  void ClickStar(GameObject obj)
  {
    ClickObject(obj);

    obj.transform.localScale *= 0.3f;

    var part = Instantiate(starParticle, obj.transform.position, new Quaternion());
    part.transform.SetParent(obj.transform,true);
    part.transform.localScale *= 3;

    stats.numStarsCollected++;
    clickStreak++;

    dayTimeLeft = Mathf.Min((dayTimeLeft + (1 * timeGainedPerStar)),initialDayLength);
    moveSpeed += (1 * streakBonusMultiplier);
    environmentSpeed += (1 * environmentMultiplier);


    obj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/star_counted");

    // TODO: Reroll repeats for now just to get the ball rolling. Replace with proper audio code later
    //////////////////////////////////////////////////////////////////////////////////////////////////
    soundIndex++;
    if (soundIndex > MAX_SOUND) soundIndex = 0;

    gameObject.AddComponent<AudioSource>();
    var buttonSound = Instantiate(Resources.Load("Sound/StarSound" + (soundIndex.ToString())) as AudioClip);
    gameObject.GetComponent<AudioSource>().PlayOneShot(buttonSound);
    //////////////////////////////////////////////////////////////////////////////////////////////////
  }

  void ClickObstacle(GameObject obj)
  {
    ClickObject(obj);
    ObstaclePunish();
  }
  void ObstaclePunish()
  {
    ResetStreak();
    environmentSpeed = initialEnvironmentSpeed / 2;
    moveSpeed = initialSpeed / 2;
    spawnTime = initialSpawnTime / 2;
  }

  void ResetStreak()
  {
    dayTimeLeft--;
    moveSpeed = Mathf.Min(initialSpeed, moveSpeed);
    clickStreak = 0;
    spawnTime = Mathf.Min(initialSpawnTime, spawnTime);
    environmentSpeed = Mathf.Min(initialEnvironmentSpeed, environmentSpeed);
  }

  void MisClick()
  {
    ResetStreak();
    misclickStreak++;

    if(misclickStreak >= 3)
    {
      misclickStreak = 0;
      ObstaclePunish();
    }
  }

  GameObject SpawnStar()
  {
    float y = Random.Range(yMin, yMax);

    return Instantiate(star, new Vector3(xSize, y, 0), new Quaternion());
  }

  GameObject SpawnEnvItem()
  {
    float y = 1.8f;

    int objNumber = Random.Range(0, environmentItems.Count);

    return Instantiate(environmentItems[objNumber], new Vector3(xSize + 10, y, -6), new Quaternion());
  }

  GameObject SpawnObstacle()
  {
    float y = Random.Range(yMin, yMax);

    return Instantiate(obstacle, new Vector3(xSize, y, 0), new Quaternion());
  }
  
  void InputChecker()
  {
    // If we're in editor use mouse to test
    if (Application.platform == RuntimePlatform.WindowsEditor)
    {
      if (Input.GetMouseButtonDown(0))
      {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject obj;

        if (Physics.Raycast(ray, out hit, 100))
        {
          obj = hit.transform.gameObject;
        }
        else obj = null;

        ClickParse(obj);
      }
    }
    else
    {
      Touch[] myTouches = Input.touches;

      for (int i = 0; i < Input.touchCount; i++)
      {
        Ray ray = Camera.main.ScreenPointToRay(myTouches[i].position);
        RaycastHit hit;
        GameObject obj;

        if (Physics.Raycast(ray, out hit, 100))
        {
          obj = hit.transform.gameObject;
        }
        else obj = null;

        ClickParse(obj);
      }
    }
  }

  // Spawn normal Items
  float spawnTimer = 0;
  float tempSpawnVariance = 1;
  void Spawner()
  {
    spawnTimer += Time.deltaTime * (((clickStreak) * progressionSpeed) + 1) * tempSpawnVariance;

    if (spawnTimer >= spawnTime)
    {
      tempSpawnVariance = Random.Range(1, itemSpawnVariance);
      
      spawnTimer = 0;

      float seed = Random.Range(0, starObstacleRatio + 1);
      if (seed >= (starObstacleRatio - 1))
      {
        //TODO: Replace with obstacle spawning code when those are requested to come back
        unclicked.Add(SpawnStar()); // TEMP CHANGE BACK WHEN OBSTACLES ARE INTRODUCED
      }
      else
      {
        unclicked.Add(SpawnStar());
      }
    }
  }

  // Spawn environment Items
  float envSpawnTimer = 0;
  float tempEnvSpawnVariance = 1;
  void EnvSpawner()
  {
    envSpawnTimer += Time.deltaTime * tempEnvSpawnVariance;

    if (envSpawnTimer >= 4)
    {
      tempEnvSpawnVariance = Random.Range(1, envSpawnVariance);

      envSpawnTimer = 0;
      
      currentEnvItems.Add(SpawnEnvItem());
    }
  }

  void ObstacleRecover()
  {
    // Recover
    if (moveSpeed <= initialSpeed)
    {
      moveSpeed += Time.deltaTime / 10;
    }

    // Recover
    if (environmentSpeed <= initialEnvironmentSpeed)
    {
      environmentSpeed += Time.deltaTime / 10;
    }

    // Recover
    if (spawnTime <= initialSpawnTime)
    {
      spawnTime += Time.deltaTime / 10;
    }
  }

  // Progress the day;
  void DayProgress()
  {
    dayTimeLeft -= Time.deltaTime;

    if(dayTimeLeft <= 0)
    {
      EndRun();
    }

    // TODO: Send Update Message
  }

  float backgroundDist = 0;
  void DisplayUpdate()
  {
    // Temp just to show time left. To be replaced with the proper gui stuff
    // timer.GetComponent<Text>().text = dayTimeLeft.ToString();

    Vector3 vec = timer.transform.localEulerAngles;
    vec.z = ((dayTimeLeft / initialDayLength) * 180) - 180;
    timer.transform.localEulerAngles = vec;

    liveScorekeeper.text = stats.numStarsCollected.ToString();

    backgroundDist = 1 - (dayTimeLeft / initialDayLength);

    Color color = background.GetComponent<Image>().color;
    background.GetComponent<RectTransform>().anchorMin = new Vector2((backgroundDist * 2) - 2, 0);
    background.GetComponent<RectTransform>().anchorMax = new Vector2((backgroundDist * 2) + 1, 1);

    //background.GetComponent<Image>().color = new Color(color.r, color.g, color.b, backgroundDist);

    // Modify background
    // Calculate clock values
    // Rotate clock based on calculations on stored values
  }

  // Update is called once per frame
  void Update()
  {
    if (!gameOver)
    {
      DayProgress();
      PerformMove();
      InputChecker();
      Spawner();
      EnvSpawner();
      ObstacleRecover();
      DisplayUpdate();
    }
  }
}
