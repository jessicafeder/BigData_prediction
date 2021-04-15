namespace LinesObjectMapper
{
    public class Question
    {
        public int Id { get; set; }
        public bool AnsweredCorrectly { get; set; }
        public float ElapsedTime { get; set; }
        public bool? HadExplanation { get; set; }

        public override string ToString() => Id.ToString();
    }
}