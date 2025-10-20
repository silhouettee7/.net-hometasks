using Quartz;
using RESTAuth.Application.Configuration.Calendars;
using RESTAuth.Domain.Abstractions.Generators;
using RESTAuth.Domain.Abstractions.Repositories;

namespace RESTAuth.Application.Jobs;

public class CreateUsersJob(
    ISchedulerFactory schedulerFactory,
    ITestDataGenerator dataGenerator,
    IUserRepository userRepository, 
    ILogger<CreateUsersJob> logger): IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var usersCount = context.MergedJobDataMap.GetInt("usersCount");
            var users = dataGenerator.CreateFullDataAboutUsersAsync(usersCount);
            await userRepository.CreateUsers(users);
            logger.LogInformation($"Created {usersCount} users at {DateTime.Now}");
            await ScheduleNextJob();
    
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"Failed to create users");
        }
    }

    private async Task ScheduleNextJob()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var nextJobKey = new JobKey("periodicCreateUsersJob", "createUsersJobs");
        if (await scheduler.CheckExists(nextJobKey))
        {
            return;
        }
        var periodicJob = JobBuilder.Create<CreateUsersJob>()
            .WithIdentity("periodicCreateUsersJob", "createUsersJobs")
            .UsingJobData("usersCount",10)
            .Build();
        
        var startAtTime = DateBuilder.FutureDate(20, IntervalUnit.Second);
        var endAtTime = startAtTime.AddMinutes(3);
        
        var calendarName = "skipSecondMinuteCalendar";
        var scipCalendar = new SkipSecondMinuteCalendar(startAtTime);
        await scheduler.AddCalendar(calendarName, scipCalendar, false, false);
        
        var periodicTrigger = TriggerBuilder.Create()
            .WithIdentity("periodicCreateUsersTrigger", "createUsersTriggers")
            .StartAt(startAtTime)
            .WithSimpleSchedule(b => b
                .WithIntervalInSeconds(10)
                .RepeatForever())
            .EndAt(endAtTime)
            .ModifiedByCalendar(calendarName)
            .Build();
        
        await scheduler.ScheduleJob(periodicJob, periodicTrigger);
        logger.LogInformation("Periodic job scheduled");
    }
}