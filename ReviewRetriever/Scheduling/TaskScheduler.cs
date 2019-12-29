using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;

namespace ReviewRetriever
{
    enum IntervalType
    {
        Minutes = 0,
        Hours   = 1,
        Day     = 2,
        Week    = 3,
        Month   = 4,
        Year    = 5
    }

    /// <summary>
    /// TaskScheduler is a base class for implementing a Quartz scheduler. This class needs to be extended to work.
    /// Derived classes should implement CreateJob, returning an IJobDetail using an IJob object defined within the class derived from TaskScheduler.
    //  Derived class should implement CreateTrigger to specify when the task should execute.
    /// </summary>

    abstract class TaskScheduler
    {
        //Implement better error handling and compile as
        public async void ScheduleTaskAsync()
        {
            try
            {
                //this should be part of config file
                NameValueCollection props = new NameValueCollection
                {
                    {"quartz.serializer.type","binary"}
                };

                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                var scheduler = await factory.GetScheduler();

                IJobDetail job = CreateJob();

                await scheduler.Start();

                ITrigger trigger = CreateTrigger();
                
                await scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception e ) 
            {
            
            }
        }

        abstract public IJobDetail CreateJob();

        abstract public ITrigger CreateTrigger();
    }
}
//========================================================================================
// implementation example:
//========================================================================================
//
//private class YelpScrapeJob : IJob
//{
//    public async Task Execute(IJobExecutionContext context)
//    {
//        var scraper = new YelpScrapeScript();
//        await Task.Run(() => scraper.Execute());
//    }
//}

//virtual public IJobDetail CreateJob()
//{
//    return JobBuilder.Create<YelpScrapeJob>().Build();
//}

//ITrigger trigger = TriggerBuilder.Create()
//                                .StartNow()
//                                .WithIdentity("YelpScrapScript", "1")
//                                .WithSimpleSchedule(x => x
//                                    .WithIntervalInSeconds(30)
//                                    .RepeatForever())
//                                .Build()