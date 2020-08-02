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

        public async Task<List<Subscription>> GetAllSubScriptions()
        {
            return await _retryPolicy.ExecuteAsync(async () => await _context.Subscriptions.ToListAsync());
        }

        public async Task<Subscription> GetSubscription(int id)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _context.Subscriptions
                    .Where(s => s.SubscriptionId == id)
                    .SingleOrDefaultAsync()
            );
        }

        public async Task AddSubscription(Subscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.Subscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task AddSubscriptions(List<Subscription> subscriptions)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _context.Subscriptions.AddRangeAsync(subscriptions);
                await _context.SaveChangesAsync();
            });
        }

        public async Task RemoveSubscription(Subscription subscription)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            });
        }

        public async Task RemoveSubscription(int subscriptionId)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var subscription = await GetSubscription(subscriptionId);
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            });
        }
    }
}