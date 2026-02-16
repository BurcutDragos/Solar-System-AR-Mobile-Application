using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Database")]
    public QuizDatabase quizDatabase;

    [Header("Quiz Settings")]
    public int numberOfQuestions = 10;

    [Header("Quiz UI")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI progressText;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI scoreText;

    [Header("Review UI")]
    public GameObject reviewPanel;
    public TextMeshProUGUI reviewQuestionText;
    public Button[] reviewAnswerButtons;

    private List<QuizQuestion> selectedQuestions = new List<QuizQuestion>();
    private int currentQuestionIndex = 0;
    private int score = 0;

    private List<int> userAnswers = new List<int>();

    private int reviewIndex = 0;

    private void Start()
    {
        GenerateRandomQuestions();
        ShowQuestion();
    }

    // -------------------------------------------------
    // RANDOM SELECTION
    // -------------------------------------------------

    void GenerateRandomQuestions()
    {
        List<QuizQuestion> pool = new List<QuizQuestion>(quizDatabase.questions);

        for (int i = 0; i < numberOfQuestions; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            selectedQuestions.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
    }

    // -------------------------------------------------
    // QUESTION DISPLAY
    // -------------------------------------------------

    void ShowQuestion()
    {
        if (currentQuestionIndex >= numberOfQuestions)
        {
            ShowResults();
            return;
        }

        QuizQuestion question = selectedQuestions[currentQuestionIndex];

        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }

        progressText.text = (currentQuestionIndex + 1) + " / " + numberOfQuestions;
    }

    // -------------------------------------------------
    // ANSWER CHECK
    // -------------------------------------------------

    void CheckAnswer(int selectedIndex)
    {
        userAnswers.Add(selectedIndex);

        if (selectedIndex == selectedQuestions[currentQuestionIndex].correctAnswerIndex)
        {
            score++;
        }

        currentQuestionIndex++;
        ShowQuestion();
    }

    // -------------------------------------------------
    // RESULTS
    // -------------------------------------------------

    void ShowResults()
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);

        scoreText.text = "Your score: " + score + " / " + numberOfQuestions;
    }

    // -------------------------------------------------
    // BUTTON ACTIONS
    // -------------------------------------------------

    public void RestartQuiz()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("PlanetsScreen");
    }

    public void OpenReview()
    {
        resultPanel.SetActive(false);
        reviewPanel.SetActive(true);
        reviewIndex = 0;
        ShowReviewQuestion();
    }

    void ShowReviewQuestion()
    {
        QuizQuestion question = selectedQuestions[reviewIndex];
        reviewQuestionText.text = question.questionText;

        int correctIndex = question.correctAnswerIndex;
        int userIndex = userAnswers[reviewIndex];

        for (int i = 0; i < reviewAnswerButtons.Length; i++)
        {
            TextMeshProUGUI btnText =
                reviewAnswerButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            btnText.text = question.answers[i];

            Image btnImage = reviewAnswerButtons[i].GetComponent<Image>();

            // Reset culoare
            btnImage.color = Color.white;

            if (i == correctIndex)
            {
                btnImage.color = Color.green;
            }

            if (i == userIndex && userIndex != correctIndex)
            {
                btnImage.color = Color.red;
            }
        }
    }

    public void NextReview()
    {
        if (reviewIndex < numberOfQuestions - 1)
        {
            reviewIndex++;
            ShowReviewQuestion();
        }
        else
        {
            // Am ajuns la final → revenim la Results
            reviewPanel.SetActive(false);
            resultPanel.SetActive(true);
        }
    }

    public void PreviousReview()
    {
        if (reviewIndex > 0)
        {
            reviewIndex--;
            ShowReviewQuestion();
        }
        else
        {
            reviewPanel.SetActive(false);
            resultPanel.SetActive(true);
        }
    }

}
