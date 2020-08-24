using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data.Models;
using System.Linq;
using Microsoft.Data.Sqlite;
using Polly.Retry;
using Polly;
using System;

namespace Server.Data.Repositories
{
    public class SubscriptionRepo : ISubscriptionRepo
    {
        private readonly DataContext _context;
        private readonly ILogger<SubscriptionRepo> _logger;
        private const int MaxRetries = 3;
        private readonly AsyncRetryPolicy _retryPolicy;

        public SubscriptionRepo(DataContext context, ILogger<SubscriptionRepo> logger)
        {
            _context = context;
            _logger = logger;

            _retryPolicy = Policy.Handle<SqliteException>()
                .WaitAndRetryAsync(MaxRetries,
                    times => TimeSpan.FromMilliseconds(times * 200), // Waiting longer and longer after each retry.
                    onRetry: (exception, _) =>
                {
                    logger.LogWarning(exception.Message); // logging exception messages.
                });

        }

        public async Task<CurrentConditionSubscription> GetCurrentConditionSubscription(int id)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            await _context.CurrentConditionSubscriptions
                .Where(s => s.SubscriptionId == id)
                .SingleOrDefaultAsync()
            );
        }

        public async Task<ForecastSubscription> GetForecastSubscription(int id)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            await _context.ForecastSubscriptions
                .Where(s => s.SubscriptionId == id)
                .SingleOrDefaultAsync()
            );
        }

        public async Task<List<CurrentConditionSubscription>> GetAllCurrentConditionSubScriptions()
        {
            return await _retryPolicy.ExecuteAsync(async () => await _context.CurrentConditionSubscriptions.ToListAsync());
        }

        public async Task<List<ForecastSubscription>> GetAllForecastSubScriptions()
        {
            return await _retryPolicy.ExecuteAsync(async () => await _context.ForecastSubscriptions.ToListAsync());
        }

        public async Task AddCurrentConditionSubscription(CurrentConditionSubscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.CurrentConditionSubscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task AddForecastSubscription(ForecastSubscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.ForecastSubscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task AddCurrentConditionSubscriptions(List<CurrentConditionSubscription> subscriptions)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.CurrentConditionSubscriptions.AddRangeAsync(subscriptions);
                await _context.SaveChangesAsync();
            });
        }

        public async Task AddForecastSubscriptions(List<ForecastSubscription> subscriptions)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.ForecastSubscriptions.AddRangeAsync(subscriptions);
                await _context.SaveChangesAsync();
            });
        }

        public async Task RemoveCurrentConditionSubscription(CurrentConditionSubscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                _context.CurrentConditionSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task RemoveForecastSubscription(ForecastSubscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                _context.ForecastSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task RemoveSubscription(int subscriptionId)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var subscription = await _context.Subscriptions.Where(s => s.SubscriptionId == subscriptionId).SingleOrDefaultAsync();
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            });
        }
    }
}