using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Data.Models;

namespace Server.Data.Repositories
{
    public interface ISubscriptionRepo
    {
        Task<Subscription> GetSubscription(int id);
        Task<List<Subscription>> GetAllSubScriptions();
        Task AddSubscription(Subscription subscription);
        Task AddSubscriptions(List<Subscription> subscription);
        Task RemoveSubscription(Subscription subscription);
        Task RemoveSubscription(int subscriptionId);
    }
}