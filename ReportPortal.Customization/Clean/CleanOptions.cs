namespace ReportPortal.Buns.Clean
{
    public class CleanOptions
    {
        public CleanOptions(bool removeSkipped = false, bool removeInterrupted = false)
        {
            RemoveSkipped = removeSkipped;
            RemoveInterrupted = removeInterrupted;
        }

        public bool RemoveSkipped { get; set; }

        public bool RemoveInterrupted { get; set; }
    }
}
