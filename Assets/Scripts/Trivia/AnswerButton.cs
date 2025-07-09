using System.Collections;
using UnityEngine;
using TMPro;


public class AnswerButton : MonoBehaviour
{
    private bool isCorrect;
    [SerializeField]
    private TextMeshProUGUI answerText;
    
    [SerializeField]
    public QuestionSetup questionSetup;

    private bool isInDelay = false;
    public void SetAnswerText(string newText)
    {
        answerText.text = newText;
    }
    
    // Optional: expose the answer text if needed
    public string GetAnswerText()
    {
        return answerText.text;
    }


    public void SetIsCorrect(bool newBool)
    {
        isCorrect = newBool;
    }

    public void OnClick()
    {
        if (isInDelay) return;

        if (isCorrect)
        {
            Debug.Log("Correct Answer");
            GameManager.Instance.getCorrectAnswer();
            // add points to score if answer is correct
            // PhotonNetwork.LocalPlayer.AddScore(100);

            if (questionSetup.questions.Count > 0)
            {
                questionSetup.InitializeNewQuestion();
            }
        }
        else
        {
            isInDelay = true;
            Debug.Log("Wrong Answer");
            GameManager.Instance.getWrongAnswer();
            // add points to score when answer is wrong
            // PhotonNetwork.LocalPlayer.AddScore(-100);
            
            // Retrieve the correct answer from the QuestionSetup and display it
            string correctAnswer = questionSetup.GetCorrectAnswerText();
            if (questionSetup.feedbackText != null)
                questionSetup.feedbackText.text = "Correct Answer: " + correctAnswer;
            isInDelay = true;

            StartCoroutine(WrongAnswerDelay());
        }
    }

    private IEnumerator WrongAnswerDelay()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        yield return new WaitForSeconds(5f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (questionSetup.questions.Count > 0)
        {
            // Clear the feedback text before starting the new question.
            if (questionSetup.feedbackText != null)
                questionSetup.feedbackText.text = "";

            questionSetup.InitializeNewQuestion(); // this is inside questionsetup, it's the process to load question and answers
        }

        isInDelay = false;
    }
}