namespace Exercise.Events;

public class ItemBought : IEvent
{
    public Guid AccountId { get; set; }
    public Guid ItemId { get; set; }
    public int OriginalPrice { get; set; }
    public int DiscountedPrice { get; set; }
    public decimal Discount { get; set; }
}
