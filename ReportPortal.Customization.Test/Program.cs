namespace ReportPortal.Customization.Test
{
    using ReportPortal.Client;
    using ReportPortal.Client.Filtering;
    using ReportPortal.Client.Models;
    using ReportPortal.Client.Requests;
    using ReportPortal.Customization.Clean;
    using ReportPortal.Customization.Merge;
    using ReportPortal.Customization.Merge.Smart;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new Service(new Uri("https://rp.epam.com/api/v1"), "aleh_yanushkevich2_personal", "7bd1c8bc-1be5-4440-b305-142558d54813");

            var cleaner = new LaunchCleaner(service, new CleanOptions { RemoveSkipped = true });

            var merger = new LaunchMerger(service);
            var cleanableMerger = new CleanableLaunchMerger(merger, cleaner);

            var smartCleaner = new SmartLaunchMerger(cleanableMerger, service, false);

            var filter = new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Equals, "name", "Demo Api Tests_autotest")
                },
                Sorting = new Sorting(new List<string> { "start_time" }, SortDirection.Descending)
            };


            var launches = await service.GetLaunchesAsync(filter);

            var first = launches.Launches[0];

            var merge = new MergeLaunchesRequest
            {
                Name = first.Name,
                Description = first.Description,
                StartTime = first.StartTime,
                EndTime = first.EndTime ?? DateTime.Now,
                MergeType = "DEEP",
                Mode = LaunchMode.Default,
                Tags = first.Tags,
                Launches = new List<string> { first.Id }
            };

            var res = cleaner.CleanAsync(first).Result;

            //var newL = await service.MergeLaunchesAsync(merge);

            //Console.WriteLine(newL.Id);

            //launch = await smartCleaner.MergeAsync(filter);

            Console.ReadKey();
        }
    }
}
