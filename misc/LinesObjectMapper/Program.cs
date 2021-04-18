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
            Console.CursorVisible = false;

            var startTime = DateTime.Now;

            User[] users = await Mapper.CsvToObjects(@"data\train.csv");
            await ShiftPriorQuestionValues(users);

            SaveCsv(@"data\shifted_train.csv", users);

            var stopTime = DateTime.Now;

            Console.WriteLine("Elapsed time: [{0}]", (stopTime - startTime).ToString("hh\\:mm\\:ss"));   
        }

        private static async Task ShiftPriorQuestionValues(IEnumerable<User> users)
        {
            int userCount = users.Count();
            int counter = 0;

            var tasks = new List<Task>();

            foreach (var user in users)
            {
                tasks.Add(Task.Run(() => {

                    QuestionBundle[] bundles = user.Bundles.ToArray();

                    for (int i = 0; i < bundles.Length - 1; i++)
                    {
                        var currentBundle = bundles[i];
                        var currentQuestions = currentBundle.Questions.ToArray();

                        var nextBundle = bundles[i + 1];
                        var nextQuestions = nextBundle.Questions.ToArray();

                        for (int k = 0; k < currentQuestions.Length; k++)
                        {
                            var q = nextQuestions.First();
                            currentQuestions[k].ElapsedTime = q.ElapsedTime;
                            currentQuestions[k].HadExplanation = q.HadExplanation;
                        }

                        bundles[i].Questions = currentQuestions;
                    }

                    int lastIndex = bundles.Length - 1;
                    var lastQuestions = bundles[lastIndex].Questions.ToArray();
                    for (int i = 0; i < lastQuestions.Length; i++)
                    {
                        lastQuestions[i].ElapsedTime = null;
                        lastQuestions[i].HadExplanation = null;
                    }

                    bundles[lastIndex].Questions = lastQuestions;
                    user.Bundles = bundles;

                    counter++;
                }));
            }

            var runningTasks = Task.WhenAll(tasks);

            while (!runningTasks.IsCompleted)
            {
                Console.WriteLine($"Shift {counter}/{userCount} ({(int)((float)counter / userCount * 100)}%)");
                await Task.Delay(250);
            }
            Console.WriteLine($"Shift {counter}/{userCount} ({(int)((float)counter / userCount * 100)}%)");
        }

        private static void SaveJson(string path, IEnumerable<User> users)
        {
            Console.WriteLine("Writing file...");
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

        private static void SaveCsv(string path, IEnumerable<User> users)
        {
            Console.WriteLine("Writing file...");
            using (var file = File.CreateText(path))
            {
                file.WriteLine("user_id,content_id,task_container_id,answered_correctly,elapsed_time,had_explanation");
                foreach (var user in users)
                    foreach (var bundle in user.Bundles)
                        foreach (var question in bundle.Questions)
                            file.WriteLine("{0},{1},{2},{3},{4},{5}",
                                user.Id,
                                question.Id,
                                bundle.Id,
                                question.AnsweredCorrectly ? 1 : 0,
                                question.ElapsedTime?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                                question.HadExplanation.HasValue ? question.HadExplanation.Value ? 1 : 0 : string.Empty
                            );
            }
        }
    }
}