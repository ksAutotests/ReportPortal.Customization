namespace ReportPortal.Customization.Merge.Strategies
{
    using ReportPortal.Client.Models;
    using System;

    public class DefaultCalculationTotalTimeStrategy : ICalculationTotalTimeStrategy
    {
        public (DateTime StartTime, DateTime? EndTime) Calculate(Launch first, Launch second)
        {
            var startTime = first.StartTime < second.StartTime
                ? first.StartTime
                : second.StartTime;

            var endTime = first.EndTime > second.EndTime
                ? first.EndTime
                : second.EndTime;

            return (startTime, endTime);
        }
    }
}
