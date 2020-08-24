using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        [Required]
        public int IntervalSeconds { get; set; }
    }
}