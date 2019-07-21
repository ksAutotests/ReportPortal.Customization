namespace ReportPortal.Customization.Merge.Smart
{
    using ReportPortal.Client.Filtering;
    using ReportPortal.Client.Models;
    using System.Threading.Tasks;

    public interface ISmartLaunchMerger
    {
        Task<Launch> MergeAsync(FilterOption filter);
    }
}
