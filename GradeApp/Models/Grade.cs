using System.Text.Json.Serialization;

namespace GradeApp.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }

        public int StudentId { get; set; }

        [JsonIgnore]
        public virtual Student? Student { get; set; }

        public int SubjectId { get; set; }

        [JsonIgnore]
        public virtual Subject? Subject { get; set; }

        public int ProfessorId { get; set; }

        [JsonIgnore]
        public virtual Professor? Professor { get; set; }
    }
}
