using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace LinesObjectMapper 
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            User[] users = await CsvToObjects(@"data\slim_train.csv");
            SaveJson(@"data\slim_train.json", users);   
        }

        private static async Task<User[]> CsvToObjects(string path)
        {
            Line[] lines = File.ReadAllLines(path)[1..] // Exclude header
                .Select(line => line.Split(','))
                .Select(segments => new Line(segments))
                .ToArray();

            // Group users and lines
            int[] distinctUserIds = lines.Select(line => line.UserId).Distinct().ToArray();
            var userSegments = new Dictionary<int, ICollection<Line>>();
            foreach (var line in lines)
            {
                int userId = line.UserId;

                if (userSegments.ContainsKey(userId))
                {
                    userSegments[userId].Add(line);
                }
                else
                {
                    var list = new LinkedList<Line>();
                    list.AddFirst(line);
                    userSegments[userId] = list;
                }
            }

            var tasks = new List<Task>();

            var users = new List<User>();

            // Keep track of progress
            int userCount = userSegments.Count;
            int i = 0;
            foreach (var keyValuePair in userSegments)
            {
                int userId = keyValuePair.Key;
                ICollection<Line> lns = keyValuePair.Value;

                tasks.Add(Task.Run(() =>
                {
                    var distinctTaskContainerIds = lns.Select(line => line.TaskContainerId).Distinct();
                    var questionBundles = new List<QuestionBundle>();
                    foreach (int taskContainerId in distinctTaskContainerIds)
                    {
                        var questionBundle = new QuestionBundle
                        {
                            Id = taskContainerId,
                            Questions = new LinkedList<Question>()
                        };

                        foreach (var line in lns)
                        {
                            if (line.TaskContainerId == taskContainerId)
                            {
                                questionBundle.Questions.Add(new Question
                                {
                                    Id = line.ContentId,
                                    AnsweredCorrectly = line.AnsweredCorrectly,
                                    ElapsedTime = line.PriorQuestionElapsedTime,
                                    HadExplanation = line.PriorQuestionHadExplanation
                                });
                            }
                        }

                        questionBundles.Add(questionBundle);
                    }

                    users.Add(new User
                    {
                        Id = userId,
                        Bundles = questionBundles
                    });

                    i++;

                    Console.WriteLine($"{i}/{userCount}  -  ({userId})");
                }));
            }
            
            await Task.WhenAll(tasks);

            return users.OrderBy(user => user.Id).ToArray();
        }

        private static void SaveJson(string path, IEnumerable<User> users)
        {
            using (var file = File.CreateText(path))
            {
                foreach (var user in users)
                {
                    var serialized = JsonSerializer.Serialize(user, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });
                    file.WriteLine(serialized);
                }
            }
        }
    }
}