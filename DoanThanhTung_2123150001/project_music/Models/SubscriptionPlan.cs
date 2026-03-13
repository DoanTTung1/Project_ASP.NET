using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class SubscriptionPlan
{
    public string PlanId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public string? Features { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
