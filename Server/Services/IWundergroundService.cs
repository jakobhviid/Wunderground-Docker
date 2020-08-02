using System.Threading.Tasks;
using Server.ServiceContracts;

namespace Server.Services
{
    public interface IWundergroundService
    {
        Task<CurrentConditions> GetCurrentConditionsAsync(string stationId);
    }
}