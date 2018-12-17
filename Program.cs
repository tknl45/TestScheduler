namespace TestScheduler
{
    using System;
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using Quartz;
    using Quartz.Impl;
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("程式開始!");
            RunProgram().GetAwaiter().GetResult();
            Console.WriteLine("程式結束!");
        }

        private static async Task RunProgram()
        {
            try
            {
                // 設定排程參數
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // 啟用排程
                await scheduler.Start();

               

                // 設定job
                IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob", "group1") // job名稱 "myJob", 群組名稱 "group1"
                .UsingJobData("jobSays", "你好!") // 參數名稱 "jobSays", 參數值 "你好!"
                .UsingJobData("myFloatValue", 3.141f) // 參數名稱 "myFloatValue", 參數值 "3.141f!"
                .Build();    

                // 設定觸發原則，從現在開始, 每10秒執行job一次
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow() // 從現在開始
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()) //每10秒執行job一次，執行無限次
                    .Build();

                //寫進排程
                await scheduler.ScheduleJob(job, trigger);

                // 等待60秒 
                await Task.Delay(TimeSpan.FromSeconds(60));

                // 關閉排程
                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }


        public class HelloJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                JobKey key = context.JobDetail.Key;

                //傳入參數
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                string jobSays = dataMap.GetString("jobSays");
                float myFloatValue = dataMap.GetFloat("myFloatValue");
                

                //現在時間
                DateTime dt = DateTime.Now;
                string dateStr = dt.ToString();//2005-11-5 13:21:25

                await Console.Out.WriteLineAsync($"[{dateStr}]{jobSays} {myFloatValue}");
            }
        }
    }


   
}


