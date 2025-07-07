using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour
{
  public static GameManager Instance { get; private set; }
  public int world { get; private set; }
  public int stage { get; private set; }
  public int lives { get; private set; }
  public int coins { get; private set; }
  private void Awake()
  {
    if (Instance != null)
    {
      DestroyImmediate(gameObject);
    }
    else
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
  }

  private void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }
  }
  private void Start()
  {
    Application.targetFrameRate = 60;
    
    NewGame();
  }

  private void NewGame()
  {
    lives = 3;
    coins = 0;

    // NetworkSceneManager.LoadScene(0, LoadSceneMode.Single);
    if (IsOwner)
    NetworkGameManager.Instance.NotifyDeathServerRpc(OwnerClientId); 
  }

  
  /* no longer used once we went to netcode
   public void LoadLevel(int world, int stage)
  {
    this.world = world;
    this.stage = stage;
    
   SceneManager.LoadScene($"{world}-{stage}"); 
  }
  */

  public void NextLevel()
  {
    // if you build the entire game, add code here to check if it's the last stage for current world"
    // if (world == 1 && stage == 10)
   // {
   //   LoadLevel(world + 1, 1);
   // }
   // LoadLevel(world, stage + 1); 
   
  }
  // this fuction takes in a delay so right when mario dies ResetLevel() isnt ran instantly.
  public void ResetLevel(float delay)
  {
    Invoke(nameof(ResetLevel), delay);
  }

  public void ResetLevel() // for when you die but have lives remaining
  {
    Debug.Log("before lives -1");
    lives--;
    Debug.Log("after lives -1");
    if (lives > 0)
    {
      Debug.Log("before IsOwner inside ResetLevel()");
      // I need to add coding here to have the player go back to the most recent level, not the start of the game.
      if (IsOwner)
      {
        Debug.Log("inside IsOwner before respawn");
        //TODO: client does not respawn
        NetworkGameManager.Instance.NotifyDeathServerRpc(OwnerClientId); // tells the server you died – it will take care of the rest
        Debug.Log("after respawn");
      }
      
    }
    else
    {
      GameOver();
    }
  }

  private void GameOver() // when you lose all 3 lives it calls NewGame function (starts from 1-1). Here you could put scode for a game over scene or UI.
  {
    // SceneManager.LoadScene($"{world}-{stage}");   // this is if you want to start over on the current level
   
   
    NewGame();
    
  }

  public void AddCoin()
  {
    coins++;
    if (coins == 100)
    {
      AddLife();
      coins = 0;
    }
  }

  public void AddLife()
  {
    lives++; // need to add ui for lives
  }

}
