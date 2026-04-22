namespace GradeApp.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public int ProfessorId { get; set; }
        public virtual Professor Professor { get; set; }
    }
}
