namespace ReportPortal.Buns.Extension
{
    using ReportPortal.Client.Models;
    using System.Linq;

    public static class ReportPortalExtension
    {
        private static readonly TestItemType[] _nonTestTypes =
        {
            TestItemType.BeforeClass,
            TestItemType.BeforeMethod,
            TestItemType.AfterMethod,
            TestItemType.AfterClass
        };

        public static bool IsTest(this TestItem test) => !IsNotTest(test);

        public static bool IsNotTest(this TestItem test) => _nonTestTypes.Contains(test.Type);

        public static bool IsFailed(this TestItem test) => test.Status == Status.Failed;

        public static bool IsSkipped(this TestItem test) => test.Status == Status.Skipped;

        public static bool IsInterrupted(this TestItem test) => test.Status == Status.Interrupted;

        public static bool IsSuite(this TestItem test)
        {
            return test.Type == TestItemType.Suite || test.Type == TestItemType.Test;
        }

        public static bool IsFinished(this Launch launch) => launch.EndTime.HasValue;

        public static bool IsNotFinished(this Launch launch) => !IsFinished(launch);
    }
}
