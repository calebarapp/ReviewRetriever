using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReviewRetriever
{

    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        public static bool JobScheduled = false;

        private static Thread SCHED_THREAD = null;

        static void Main(string[] args)
        {
            Console.WriteLine("--           Yelp Review Retriever           --", LogLevel.Console);
            exec(args);
        }

        private static void exec(string[] args) 
        {
            if (args.Length == 0)
            {
                string[] saInput = GetCommand();
                if(saInput != null)
                    execCommands(saInput);
            }
            else 
            {
                execCommands(args);
            }
            exec(new string[0]);
        }

        static string[] GetCommand() 
        {
            string sInput = Console.ReadLine();
            if (string.IsNullOrEmpty(sInput))
                return null;

            return sInput.Split('"').Select((element, i) => i % 2 == 0
                                                ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                : new string[] { element }
                                                ).SelectMany(element => element).ToArray();
        }

        static private void execCommands(string[] saInput) 
        {
            switch (saInput[0].ToUpper())
            {
                case "START":
                    SpawnSchedulerThread(schedulerExec);
                    break;
                case "SHUTDOWN":
                    killScheduler();
                    Environment.Exit(0);
                    break;
                case "STORES":
                    printStores();
                    break;
                case "RET":
                    procRet(saInput);
                    break;
            }
        }

        private static void killScheduler() 
        {
            if (SCHED_THREAD != null)
            {
                Logging.Log("killScheduler() -> Scheduled job canceled.", LogLevel.Both);
                try
                {
                    SCHED_THREAD.Abort();
                }
                catch(Exception e) 
                {
                    Logging.Log("killScheduler()-> " + e.Message, LogLevel.LogFile);
                }
            }
            else 
            {
                Logging.Log("killScheduler() -> No job scheduled.", LogLevel.Both);
            }
        }

        private static void SpawnSchedulerThread(Action task) 
        {
            if (!JobScheduled)
            {
                Logging.Log("execCommand() -> Scheduling Review collection with Cron string: " + config.CronJobSchedule, LogLevel.Both);
                try
                {
                    SCHED_THREAD = new Thread(new ThreadStart(schedulerExec));
                    SCHED_THREAD.Start();
                    JobScheduled = true;
                }
                catch (Exception ex)
                {
                    Logging.Log("execCommand() -> Error spawning job thread: " + ex.Message, LogLevel.Both);
                }
            }
            else
            {
                Logging.Log("execCommand()-> Thread already started. A maximum of one job can be scheduled", LogLevel.Both);
            }
        }

        //============================================================
        //Commands:
        //============================================================
        private static void schedulerExec() 
        {
            var scheduler = new YelpScrapeTaskScheduler();
            scheduler.ScheduleTaskAsync();
            
        }

        private static void printStores() 
        {
            try
            {
                Console.WriteLine("--------------------------------------------------------------------------");
                Console.WriteLine("Stores:");
                Console.WriteLine("--------------------------------------------------------------------------");
                foreach (var store in config.Stores)
                {
                    Console.WriteLine("Store Name:  " + store.StoreName);
                    Console.WriteLine("Store Url:   " + store.Url + Environment.NewLine);
                }
                Console.WriteLine("--------------------------------------------------------------------------");
            }
            catch { }
        }

        private static void procRet(string[] saInput) 
        {
            if (saInput.Count() == 1)
            {
                Console.WriteLine("\"From\" argument required.");
                return;
            }
            else if (saInput.Count() > 1) 
            {
                int iDateOffset = 0;
                if (int.TryParse(saInput[1], out iDateOffset))
                {
                    YelpScrapeScript scraper;

                    DateTime dtFrom = DateTime.Now.AddDays(0 - iDateOffset);
                    
                    if (saInput.Count() > 2) 
                        scraper = new YelpScrapeScript(dtFrom, saInput[2]);
                    else
                        scraper = new YelpScrapeScript(dtFrom);

                    scraper.Execute();
                }
                else 
                {
                    Console.WriteLine("\"ret\" command must have a valid int as date time offset argument. ret [iDateOffset] <optional [storeName]>");
                }
            }
        }
    }
}
