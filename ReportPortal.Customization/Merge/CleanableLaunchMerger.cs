namespace ReportPortal.Buns.Merge
{
    using ReportPortal.Client.Models;
    using ReportPortal.Buns.Clean;
    using System;
    using System.Threading.Tasks;

    public class CleanableLaunchMerger : ILaunchMerger
    {
        private readonly ILaunchMerger _decorated;

        private readonly ILaunchCleaner _cleaner;

        public CleanableLaunchMerger(ILaunchMerger decorated, ILaunchCleaner cleaner)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _cleaner = cleaner ?? throw new ArgumentNullException(nameof(cleaner));
        }

        public async Task<Launch> MergeAsync(Launch cleanable, Launch nonCleanable)
        {
            var launchAfterClean = await _cleaner.CleanAsync(cleanable).ConfigureAwait(false);

            return await _decorated.MergeAsync(launchAfterClean, nonCleanable).ConfigureAwait(false);
        }
    }
}
