using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Data.Models;

namespace Server.Data.Repositories
{
    public interface ISubscriptionRepo
    {
        Task<CurrentConditionSubscription> GetCurrentConditionSubscription(int id);
        Task<ForecastSubscription> GetForecastSubscription(int id);
        Task<List<CurrentConditionSubscription>> GetAllCurrentConditionSubScriptions();
        Task<List<ForecastSubscription>> GetAllForecastSubScriptions();
        Task AddCurrentConditionSubscription(CurrentConditionSubscription subscription);
        Task AddForecastSubscription(ForecastSubscription subscription);
        Task AddCurrentConditionSubscriptions(List<CurrentConditionSubscription> subscriptions);
        Task AddForecastSubscriptions(List<ForecastSubscription> subscriptions);
        Task RemoveCurrentConditionSubscription(CurrentConditionSubscription subscription);
        Task RemoveForecastSubscription(ForecastSubscription subscription);
        Task RemoveSubscription(int subscriptionId);
    }
}