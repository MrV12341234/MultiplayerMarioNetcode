using System.Collections;
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
  [SerializeField] private PlayerMovement playerPrefab; // used in GameOver() to stop playermovement
  [SerializeField] private GameObject mainMenuUI;
  [SerializeField] private GameObject gameOverUI;
  
  public GameObject quiz;
  public GameObject correctAnswer;
  public GameObject wrongAnswer;
  public bool showQuiz;
  
  [HideInInspector] public int correctAnswerCounter = 0;
  
  public void getCorrectAnswer() // called from AnswerButton.cs after correct answer pressed
  {
    correctAnswerCounter++;
    if (correctAnswerCounter == 3)
    {
      quiz.SetActive(false);
      ResetLevel();
    } 
    StartCoroutine(showCorrectAnswer());
  }

  public void getWrongAnswer()
  {
    StartCoroutine(showWrongAnswer());
  }

  public void ShowQuiz()
  {
    if (showQuiz) // check box inside GameManager inspector
    {
      
      quiz.SetActive(true);
      
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
      correctAnswerCounter = 0;
      

      // Initialize the first question
      QuestionSetup setup = quiz.GetComponentInChildren<QuestionSetup>();
      
      if (setup != null)
      {
        setup.InitializeNewQuestion();
      }
    }
    else
    {
      ResetLevel();
    }
  }

  IEnumerator showCorrectAnswer()
  {
    correctAnswer.SetActive(true);
    yield return new WaitForSeconds(0.5f);
    correctAnswer.SetActive(false);
  }

  IEnumerator showWrongAnswer()
  {
    wrongAnswer.SetActive(true);
    yield return new WaitForSeconds(5f);
    wrongAnswer.SetActive(false);
  }
  
  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
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
    playerPrefab.enabled = true; // 
    NewGame();
  }

  private void NewGame()
  {
    lives = 3;
    coins = 0;

    // NetworkSceneManager.LoadScene(0, LoadSceneMode.Single);
    
    NetworkGameManager.Instance.NotifyDeathServerRpc(); 
  }
 
  
  // this fuction takes in a delay so right when mario dies ResetLevel() isnt ran instantly. i stopped using a delay after trivia was introduced
  public void ResetLevel(float delay)
  {
    Invoke(nameof(ResetLevel), delay);
  }

  public void ResetLevel() // for when you die but have lives remaining
  {
    
    lives--;
    
    if (lives > 0)
    {
      
      // TODO: I need to add coding here to have the player go back to checkpoint, not the start of the game. try using Y coordinate location
      
        NetworkGameManager.Instance.NotifyDeathServerRpc(); // tells the server you died – it will take care of the rest
        
    }
    else
    {
      GameOver();
    }
  }

  private void GameOver() // when you lose all 3 lives it calls NewGame function (starts from 1-1). Here you could put scode for a game over scene or UI.
  {
    if (IsServer) // we do not want the host disconnecting from the game, so they just respawn
    {
      NewGame();
    }
    else
    { 
      playerPrefab.enabled = false;
      mainMenuUI.SetActive(false);
      gameOverUI.SetActive(true);
    }
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
    lives++; 
  }

}
