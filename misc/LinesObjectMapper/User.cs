using System.Collections.Generic;

namespace LinesObjectMapper
{
    public class User
    {
        public int Id { get; set; }
        public ICollection<QuestionBundle> Bundles { get; set; }

        public override string ToString() => Id.ToString();
    }
}