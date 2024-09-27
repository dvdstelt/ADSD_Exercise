namespace Exercise.Commands;

public class BuyItem : ICommand
{
    public Guid AccountId { get; set; }
    public Guid ItemId { get; set; }
    public int Price { get; set; }
}