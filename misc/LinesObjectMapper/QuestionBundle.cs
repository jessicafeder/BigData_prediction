using System.Collections.Generic;

namespace LinesObjectMapper
{
    public class QuestionBundle
    {
        public int Id { get; set; }
        public ICollection<Question> Questions { get; set; }

        public override string ToString() => Id.ToString();
    }
}