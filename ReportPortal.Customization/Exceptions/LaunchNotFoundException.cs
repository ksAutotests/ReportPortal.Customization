namespace ReportPortal.Buns.Exceptions
{
    using System;

    public class LaunchNotFoundException : Exception
    {
        public LaunchNotFoundException()
        {
        }

        public LaunchNotFoundException(string message)
            : base(message)
        {
        }

        public LaunchNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
