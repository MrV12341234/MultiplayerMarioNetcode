using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class GameManager : NetworkBehaviour
{
  public static GameManager Instance { get; private set; }
  public int world { get; private set; }
  public int stage { get; private set; }
  public int lives { get; private set; }
  public int coins { get; private set; }
  
  private PlayerMovement _localMover;
  [SerializeField] private GameObject mainMenuUI;
  [SerializeField] private GameObject gameOverUI;
  
  public GameObject quiz;
  public GameObject correctAnswer;
  public GameObject wrongAnswer;
  public bool showQuiz;
  private bool _localPlayerDied; 
  private PlayerMovement _lockedMovement;
  
  
  [HideInInspector] public int correctAnswerCounter = 0;
  public void OnLocalPlayerDied()       { _localPlayerDied = true; } // called in Player.cs to help determine if player is answer trivia b/c dead or b/c collided with object that set triviaUI active
  public void OnLocalPlayerRespawned()  { _localPlayerDied = false; }
  public void RegisterLocalMover(PlayerMovement mover)
  {
    _localMover = mover;
  }
  private void LockMovement()   { if (_localMover) _localMover.enabled = false; }
  private void UnlockMovement() { if (_localMover) _localMover.enabled = true;  }
  
  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }
  
  private void Start()
  {
    Application.targetFrameRate = 60;
    UnlockMovement(); 
    NewGame();
  }
  
  public void getCorrectAnswer() // called from AnswerButton.cs after correct answer pressed
  {
    correctAnswerCounter++;
    if (correctAnswerCounter == 3 && _localPlayerDied) // used to check if player is answering questions because they died
    {
      quiz.SetActive(false);
      Debug.Log("inside getCorrectAnswer, correctAnswerCounter==3 && _localPlayerDied, after set quiz active");
      ResetLevel();
    } 
    else if (correctAnswerCounter == 3) // used to check if player is answering questions when not dead (trivia forced by collision/trigger of object)
    {
      quiz.SetActive(false);
      Debug.Log("inside getCorrectAnswer, correctAnswerCounter==3, after set quiz active");
      UnlockMovement();
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
      LockMovement();
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
      // TODO: add check for isDead before resetting level. if showQuiz is not checked, ShowQuiz() is called from TriviaActivator on some objects
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

  private void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }
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
    if (IsServer) // do not want the host disconnecting from the game, so they just respawn
    {
      NewGame();
    }
    else
    {
      LockMovement(); // this function is at the top of this class. Player.cs feeds it
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
