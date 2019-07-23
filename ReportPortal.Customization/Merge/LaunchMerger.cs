namespace ReportPortal.Customization.Merge
{
    using ReportPortal.Client;
    using ReportPortal.Client.Models;
    using ReportPortal.Client.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LaunchMerger : ILaunchMerger
    {
        private readonly MergeOptions _options;

        private readonly Service _service;

        public LaunchMerger(Service service, MergeOptions option = null)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _options = option ?? MergeOptions.Default;
        }

        private MergeLaunchesRequest ConfigureMergeRequest(Launch first, Launch second)
        {
            var name = _options.Name
                ?? first.Name
                ?? second.Name
                ?? "unspecified";

            var description = _options.Description
                ?? first.Description
                ?? second.Description
                ?? "unspecified";

            var tags = first.Tags ?? Array.Empty<string>()
                .Concat(second.Tags)
                .Concat(_options.Tags)
                .Distinct()
                .ToList();

            var (startTime, endTime) = _options.TimeStrategy.Calculate(first, second);

            return new MergeLaunchesRequest
            {
                Name = name,
                Description = description,

                Launches = new List<string> { first.Id, second.Id },
                Tags = tags,

                Mode = _options.Mode ?? first.Mode,
                MergeType = _options.Type,

                StartTime = startTime,
                EndTime = endTime ?? DateTime.Now
            };
        }

        public async Task<Launch> MergeAsync(Launch first, Launch second)
        {
            return await _service.MergeLaunchesAsync(ConfigureMergeRequest(first, second)).ConfigureAwait(false);
        }
    }
}
