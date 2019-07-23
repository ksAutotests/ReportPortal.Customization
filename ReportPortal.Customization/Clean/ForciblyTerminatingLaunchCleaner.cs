namespace ReportPortal.Customization.Clean
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using ReportPortal.Client;
    using ReportPortal.Client.Models;
    using ReportPortal.Client.Requests;
    using ReportPortal.Customization.Extension;

    public class ForciblyTerminatingLaunchCleaner : ILaunchCleaner
    {
        private readonly ILogger _logger;

        private readonly ILaunchCleaner _decorated;

        private readonly Service _service;

        public ForciblyTerminatingLaunchCleaner(ILaunchCleaner decorated, Service service, ILogger logger = null)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            _logger = logger;
        }

        public async Task<Launch> CleanAsync(Launch launch)
        {
            if (launch.IsNotFinished())
            {
                var finishRequest = new FinishLaunchRequest { EndTime = DateTime.UtcNow };
                var message = await _service.FinishLaunchAsync(launch.Id, finishRequest, true).ConfigureAwait(false);

                _logger.LogDebug(message.Info);
            }

            return await _decorated.CleanAsync(launch).ConfigureAwait(false);
        }
    }
}
