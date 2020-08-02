using System;

namespace Server.Data.Models
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public string StationId { get; set; }
        public int IntervalSeconds { get; set; }
    }
}