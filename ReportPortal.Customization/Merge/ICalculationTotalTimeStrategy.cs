namespace ReportPortal.Buns.Merge
{
    using ReportPortal.Client.Models;
    using System;

    public interface ICalculationTotalTimeStrategy
    {
        (DateTime StartTime, DateTime? EndTime) Calculate(Launch first, Launch second);
    }
}
