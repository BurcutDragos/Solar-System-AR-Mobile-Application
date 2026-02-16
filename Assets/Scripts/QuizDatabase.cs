using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "QuizDatabase",
    menuName = "Quiz/Database"
)]
public class QuizDatabase : ScriptableObject
{
    public List<QuizQuestion> questions;
}
