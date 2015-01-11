using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsQuery;

namespace BlogPostMigrater
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = GetBlogPosts();
            task.Wait();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            //string input;
            //do
            //{
            //    input = Console.ReadLine();
            //} while (input !=null && !input.ToLower().Equals("quit"));
        }
        
        static async Task GetBlogPosts()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"http://www.dallas-csharp-sig.com");
                for (int year = 2010; year < 2015; year++)
                {
                    for (int month = 1; month < 13; month++)
                    {
                        Console.WriteLine("Getting {0}/{1}", year, month);
                        var response = await client.GetAsync(string.Format("/{0}/{1}/", year, month));
                        if (response.IsSuccessStatusCode)
                        {
                            var html = await response.Content.ReadAsStringAsync();
                            CQ cq = html;

                            var meetingDate = MeetingDate(year, month);
                            var filepath = FilePath(year, month, meetingDate.ToString("MMMM"));
                            Console.WriteLine("Writing: {0}", filepath);
                            using (var file = File.CreateText(filepath))
                            {
                                file.WriteLine(FrontMatter(cq[".post h2"].Text(), meetingDate));
                                file.WriteLine(cq[".post .content"].Html());
                            }
                        }
                    }
                }
            }
        }

        static string FrontMatter(string title, DateTime date )
        {
            var builder = new StringBuilder();
            
            builder.AppendLine("---");
            builder.AppendLine("layout: legacyMeeting");
            builder.AppendLine(string.Format("date: {0}-{1:D2}-{2}", date.Year, date.Month, date.Day));
            builder.AppendLine(string.Format("title: \"{0}\"", title));
            builder.AppendLine("speaker:");
            builder.AppendLine("twitter:");
            builder.AppendLine("eventbrite:");
            builder.AppendLine("github:");
            builder.AppendLine("abstract:");
            builder.AppendLine("bio:");
            builder.AppendLine("redirect_from:");
            builder.AppendLine(string.Format("  - /{0}/{1:D2}/", date.Year, date.AddMonths(-1).Month));
            builder.AppendLine("---");

            return builder.ToString();
        }

        static DateTime MeetingDate(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            while (date.DayOfWeek != DayOfWeek.Thursday)
            {
                date = date.AddDays(1);
            }
            return date.AddMonths(1);
        }

        static string FilePath(int year, int month, string meetingName)
        {
            return string.Format(@"c:\temp\oldposts\{0}-{1:D2}-{2}-meeting.markdown", year, month, meetingName.ToLower());
        }
    }
}
