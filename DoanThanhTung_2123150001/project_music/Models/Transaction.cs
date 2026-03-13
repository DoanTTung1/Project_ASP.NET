using System;
using System.Collections.Generic;

namespace project_music.Models;

public partial class Transaction
{
    public string TransactionId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentGateway { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
