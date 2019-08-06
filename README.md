# Installation
[![NuGet version](https://badge.fury.io/nu/Buns.ReportPortal.svg)](https://badge.fury.io/nu/Buns.ReportPortal)

Install **Buns.ReportPortal** NuGet package into your project for cleaning or merging ReportPortal launches.

# Usage example

```C#
namespace ReportPortal.Buns.Test
{
    using ReportPortal.Client;
    using ReportPortal.Client.Filtering;
    using ReportPortal.Buns.Clean;
    using ReportPortal.Buns.Merge;
    using ReportPortal.Buns.Merge.Smart;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new Service(new Uri("path_to_report_portal"), "project_name", "uuid");

            var cleanOptions = new CleanOptions(removeSkipped: true, removeInterrupted: false);

            //simple cleaning
            var cleaner = new LaunchCleaner(service, cleanOptions);

            var filter = new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Equals, "name", "launch_name_for_cleaning")
                },
                Sorting = new Sorting(new List<string> { "start_time" }, SortDirection.Descending),
                Paging = new Paging(1, short.MaxValue)
            };

            var container = await service.GetLaunchesAsync(filter, debug: false);
            var cleanedLaunch = await cleaner.CleanAsync(launch: container.Launches[0]);

            //if launch not finished finishing launch and than clean.
            var forciblyTerminatingLaunchCleaner = new ForciblyTerminatingLaunchCleaner(cleaner, service);
            cleanedLaunch = await forciblyTerminatingLaunchCleaner.CleanAsync(container.Launches[0]);

            //simple merging two launches
            var merger = new LaunchMerger(service, MergeOptions.Default);
            var mergedLaunch = await merger.MergeAsync(container.Launches[0], container.Launches[1]);

            //merging with cleaning launch passed to mathod as first parameter
            var cleanableLaunchMerger = new CleanableLaunchMerger(merger, cleaner);
            mergedLaunch = await cleanableLaunchMerger.MergeAsync(container.Launches[1], container.Launches[0]);

            //merging two latest launches with filter ordering by descending start time with cleaning second launch
            var smartMerger = new SmartLaunchMerger(cleanableLaunchMerger, service, debug: false);
            mergedLaunch = await smartMerger.MergeAsync(filter);
            
        }
    }
}

```
