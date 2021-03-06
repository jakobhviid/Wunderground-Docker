using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    // By inherting subscription EF Core will create a TPH (table per hierarchy) relationship, where one table will contain all the information
    public class CurrentConditionSubscription : Subscription
    {
        [Required]
        public string StationId { get; set; }
    }
}