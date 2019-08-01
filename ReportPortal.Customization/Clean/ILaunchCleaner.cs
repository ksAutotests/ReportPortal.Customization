namespace ReportPortal.Buns.Clean
{
    using ReportPortal.Client.Models;
    using System.Threading.Tasks;

    public interface ILaunchCleaner
    {
        Task<Launch> CleanAsync(Launch launch);
    }
}
