using UnityEngine;

[CreateAssetMenu(
    fileName = "NewQuizQuestion",
    menuName = "Quiz/Question"
)]
public class QuizQuestion : ScriptableObject
{
    [Header("Question data")]
    [TextArea(2, 4)]
    public string questionText;

    [Header("Answer options")]
    public string[] answers = new string[4];

    [Header("Correct answer")]
    [Range(0, 3)]
    public int correctAnswerIndex;

    [Header("Related celestial body")]
    public string celestialBodyName;
}
