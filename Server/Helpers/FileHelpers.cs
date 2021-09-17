using System.IO;

namespace Server.Helpers
{
    public class FileHelpers
    {
        public static readonly string InitialSubscriptionsFileCheckPath = "INITSUBSCRIPTIONS_RUN_BEFORE";
        public static readonly string SQLiteDBFilePath = "/database/test.db";

        public static void CreateInitialSubscriptionsFile() {
            File.Create(InitialSubscriptionsFileCheckPath);
        }
    }
}