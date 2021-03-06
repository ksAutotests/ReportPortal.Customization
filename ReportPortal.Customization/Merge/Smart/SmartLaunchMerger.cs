﻿namespace ReportPortal.Buns.Merge.Smart
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using ReportPortal.Client;
    using ReportPortal.Client.Filtering;
    using ReportPortal.Client.Models;
    using ReportPortal.Buns.Exceptions;

    public class SmartLaunchMerger : ISmartLaunchMerger
    {
        private readonly ILogger _logger;

        private readonly ILaunchMerger _merger;

        private readonly Service _service;

        private readonly bool _debug;

        public SmartLaunchMerger(ILaunchMerger merger, Service service, bool debug, ILogger logger = null)
        {
            _debug = debug;
            _logger = logger;

            _merger = merger ?? throw new ArgumentNullException(nameof(merger));
            _service = service ?? throw new ArgumentNullException(nameof(merger));
        }

        public async Task<Launch> MergeAsync(FilterOption filter)
        {
            filter.Sorting = new Sorting(new List<string> { "start_time" }, SortDirection.Descending);

            var container = await _service.GetLaunchesAsync(filter, _debug).ConfigureAwait(false);
            _logger?.LogDebug($"Successfully found ${container.Launches.Count} launch(es)");

            switch (container.Launches.Count)
            {
                case 0:
                    throw new LaunchNotFoundException("Launches for merging not found.");
                case 1:
                    return await Task.FromResult(container.Launches[0]).ConfigureAwait(false);
                default:
                    {
                        var current = container.Launches[0];
                        var previous = container.Launches[1];

                        return await _merger.MergeAsync(previous, current).ConfigureAwait(false);
                    }
            }
        }
    }
}
