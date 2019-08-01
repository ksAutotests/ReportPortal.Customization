namespace ReportPortal.Buns.Merge
{
    using ReportPortal.Client.Models;
    using ReportPortal.Buns.Merge.Strategies;
    using System.Collections.Generic;

    public class MergeOptions
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; }

        public string Type { get; set; }

        public LaunchMode? Mode { get; set; }

        public ICalculationTotalTimeStrategy TimeStrategy { get; set; }

        public static MergeOptions Default
        {
            get
            {
                return new MergeOptions
                {
                    Type = MergeTypes.Deep,
                    Tags = new List<string>(),
                    TimeStrategy = new DefaultCalculationTotalTimeStrategy(),
                };
            }
        }
    }
}
