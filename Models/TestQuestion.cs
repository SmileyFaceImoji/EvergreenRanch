using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class TestQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string QuestionText { get; set; } = default!;
        // stored as JSON array of strings
        public string ChoicesJson { get; set; } = default!;
        public int CorrectAnswerIndex { get; set; }

        [NotMapped]
        public string[] Choices
        {
            get => System.Text.Json.JsonSerializer.Deserialize<string[]>(ChoicesJson) ?? Array.Empty<string>();
            set => ChoicesJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}
