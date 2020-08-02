using System.IO;

namespace Server.Helpers
{
    public class FileHelpers
    {
        public static readonly string InitialSubscriptionsFileCheckPath = "/Users/oliver/OfflineDocuments/GitProjects/Arbejde/WeatherStationDriver-Docker/INITSUBSCRIPTIONS_RUN_BEFORE";
        public static readonly string SQLiteDBFilePath = "/Users/oliver/OfflineDocuments/GitProjects/Arbejde/WeatherStationDriver-Docker/Server/test.db";

        public static void CreateInitialSubscriptionsFile() {
            File.Create(InitialSubscriptionsFileCheckPath);
        }
    }
}