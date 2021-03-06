﻿namespace ReportPortal.Buns.Merge
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using ReportPortal.Buns.Extension;
    using ReportPortal.Client;
    using ReportPortal.Client.Models;
    using ReportPortal.Client.Requests;

    public class ForciblyTerminatingLaunchMerger : ILaunchMerger
    {
        private readonly ILogger _logger;

        private readonly ILaunchMerger _decorated;

        private readonly Service _service;

        public ForciblyTerminatingLaunchMerger(ILaunchMerger merger, Service service, ILogger logger = null)
        {
            _decorated = merger ?? throw new ArgumentNullException(nameof(merger));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            _logger = logger;
        }

        public async Task<Launch> MergeAsync(Launch first, Launch second)
        {
            await FinishLaunch(first);
            await FinishLaunch(second);

            var launch = await _decorated.MergeAsync(first, second);
            _logger?.LogDebug($"Merging launches with id {first.Id} and {second.Id} successfully completed.");

            return launch;
        }

        private async Task FinishLaunch(Launch launch)
        {
            if (launch.IsNotFinished())
            {
                var finishRequest = new FinishLaunchRequest { EndTime = DateTime.UtcNow };
                var message = await _service.FinishLaunchAsync(launch.Id, finishRequest, true).ConfigureAwait(false);

                _logger?.LogDebug(message.Info);
            }
        }
    }
}
