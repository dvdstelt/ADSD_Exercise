namespace Exercise.Events;

public class PurchaseFailed : IEvent
{
    public Guid AccountId { get; set; }
    public Guid ItemId { get; set; }
}