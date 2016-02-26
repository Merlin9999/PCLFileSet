using System;
using System.IO;
using System.Reflection;

namespace PCLFileSetTests
{
    public static class TestHelper
    {
        public static string GetTestExecutionPath()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            return path != null ? new Uri(path).LocalPath : null;
        }
    }
}