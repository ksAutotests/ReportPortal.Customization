namespace ReportPortal.Customization.Test
{
    using ReportPortal.Client;
    using ReportPortal.Client.Filtering;
    using ReportPortal.Client.Models;
    using ReportPortal.Customization.Clean;
    using ReportPortal.Customization.Merge;
    using ReportPortal.Customization.Merge.Smart;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {
        static void  Main(string[] args)
        {
            var service = new Service(new Uri("http://mo-64a78b01b.mo.sap.corp:9999/api/v1/"), "olegyanush_personal", "5944fe3f-ac66-40c1-9f9b-83263b2d2dab");

            var cleaner = new LaunchCleaner(service, new CleanOptions { });

            var merger = new LaunchMerger(service);
            var cleanableMerger = new CleanableLaunchMerger(merger, cleaner);

            var smartCleaner = new SmartLaunchMerger(cleanableMerger, service, false);

            var filter = new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Equals, "name", "Demo Api Tests_regression")
                }
            };


            var launch = smartCleaner.MergeAsync(filter).Result;

            Console.ReadKey();
        }
    }
}
