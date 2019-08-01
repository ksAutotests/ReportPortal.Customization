namespace ReportPortal.Buns.Merge
{
    using ReportPortal.Client.Models;
    using System.Threading.Tasks;

    public interface ILaunchMerger
    {
        Task<Launch> MergeAsync(Launch first, Launch second);
    }
}
