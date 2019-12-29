using Quartz;
using Quartz.Impl;
using ReviewRetriever;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace ReviewRetriever
{
    class YelpScrapeTaskScheduler : TaskScheduler
    {

        override public IJobDetail CreateJob()
        {
            return JobBuilder.Create<YelpScrapeJob>().Build();
        }

        override public ITrigger CreateTrigger()
        {
            TriggerBuilder trigger = null;
            try
            {
                trigger = TriggerBuilder.Create()
                                        .StartNow()
                                        .WithIdentity("YelpScrapScript", "1")
                                        .WithCronSchedule(config.CronJobSchedule);

                return trigger.Build();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        class YelpScrapeJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var scraper = new YelpScrapeScript();
                await Task.Run(() => scraper.Execute());
            }
        }
    }
}