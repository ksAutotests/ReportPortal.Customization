namespace ReportPortal.Customization.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
