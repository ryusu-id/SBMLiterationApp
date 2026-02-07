using System.Text.Json.Serialization;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public class QuizQuestion : AuditableEntity
{
    public int Id { get; protected set; }
    public int DailyReadId { get; protected set; }
    public int QuestionSeq { get; protected set; }
    public string Question { get; protected set; }
    public string CorrectAnswer { get; protected set; }
    public ICollection<QuizChoice> Choices { get; protected set; } = [];

    [JsonIgnore]
    public DailyRead DailyRead { get; protected set; } = null!;

#pragma warning disable CS8618
    public QuizQuestion() { }
#pragma warning restore CS8618

    public static QuizQuestion Create(int dailyReadId, int questionSeq, string question, string correctAnswer)
    {
        return new QuizQuestion
        {
            DailyReadId = dailyReadId,
            QuestionSeq = questionSeq,
            Question = question,
            CorrectAnswer = correctAnswer
        };
    }

    public void Update(string question, string correctAnswer)
    {
        Question = question;
        CorrectAnswer = correctAnswer;
    }

    public void ClearChoices()
    {
        Choices.Clear();
    }

    public void AddChoice(string choice, string answer)
    {
        var quizChoice = QuizChoice.Create(choice, answer);
        ((List<QuizChoice>)Choices).Add(quizChoice);
    }
}
