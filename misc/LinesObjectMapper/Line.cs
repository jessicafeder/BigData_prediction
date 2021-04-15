using System.Linq;

namespace LinesObjectMapper
{
    public class Line
    {
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public int TaskContainerId { get; set; }
        public bool AnsweredCorrectly { get; set; }
        public float PriorQuestionElapsedTime { get; set; }
        public bool? PriorQuestionHadExplanation { get; set; }

        public Line(string[] values)
        {
            UserId = int.Parse(values[0]);
            ContentId = int.Parse(values[1]);
            TaskContainerId = int.Parse(values[2]);
            AnsweredCorrectly = values[3] is "1";
            PriorQuestionElapsedTime = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
            PriorQuestionHadExplanation = values[5] is "1" ? true : values[5] is "0" ? false : null;
        }

        public override string ToString() => 
            string.Join(',', UserId, ContentId, TaskContainerId, AnsweredCorrectly, PriorQuestionElapsedTime, PriorQuestionHadExplanation);
    }
}