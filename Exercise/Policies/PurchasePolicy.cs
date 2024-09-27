using System.Globalization;
using Exercise.Commands;
using Exercise.Events;

namespace Exercise.Policies;

public class PurchasePolicy : Saga<PurchasePolicyData>,
    IAmStartedByMessages<BuyItem>,
    IAmStartedByMessages<IncreaseBalance>
{
    const string Format = "yyyy-MM-dd HH:mm:ss:ffffff Z";
    
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PurchasePolicyData> mapper)
    {
        mapper.MapSaga(s => s.AccountId)
            .ToMessage<BuyItem>(data => data.AccountId)
            .ToMessage<IncreaseBalance>(m => m.AccountId);
    }

    public async Task Handle(BuyItem message, IMessageHandlerContext context)
    {
        // Don't just use `Today`, we need to figure out for which day this message was sent.
        var timeSent = context.MessageHeaders["NServiceBus.TimeSent"];
        var daySent = DateTime.ParseExact(timeSent, Format, CultureInfo.InvariantCulture).ToUniversalTime().Date;
        var discount = GetDiscount(message.Price, daySent);
        int finalPrice = (int)(message.Price - message.Price * discount);
        
        if (Data.Balance >= finalPrice)
        {
            Console.WriteLine($"Item worth {message.Price} bought for {finalPrice}");

            Data.Balance -= finalPrice;
            if (Data.Purchases.ContainsKey(daySent))
                Data.Purchases[daySent] += finalPrice;
            else
                Data.Purchases[daySent] = finalPrice;

            await context.Publish(new ItemBought { AccountId = Data.AccountId, ItemId = message.ItemId, OriginalPrice = message.Price, DiscountedPrice = finalPrice , Discount = discount });
        }
        else
        {
            Console.WriteLine($"Not enough balance to purchase item worth {message.Price}");
            await context.Publish(new PurchaseFailed { AccountId = message.AccountId, ItemId = message.ItemId });
        }
    }

    private int CalculateTotalPurchased(int numberOfDays, DateTime daySent)
    {
        int totalPurchased = 0;
        for (int i = 0; i < numberOfDays - 1; i++)
        {
            if (Data.Purchases.TryGetValue(daySent.AddDays(i * -1), out var purchased))
                totalPurchased += purchased;
        }

        return totalPurchased;
    }

    private decimal GetDiscount(int price, DateTime daySent)
    {
        int numberOfDays = Data.IsPreferredCustomer ? 14 : 7;
        int totalPurchased = CalculateTotalPurchased(numberOfDays, daySent);

        decimal discount = 0;
        // Adjust discount rates based on customer type and previous purchases
        if (Data.IsPreferredCustomer)
            discount = totalPurchased > 100 ? 0.2m : 0.1m;
        else if (totalPurchased > 100)
            discount = 0.1m;

        return discount;
    }

    public Task Handle(IncreaseBalance message, IMessageHandlerContext context)
    {
        Data.Balance += message.AddedBalance;
        Console.WriteLine($"Increase balance with {message.AddedBalance} to [{Data.Balance}]");

        return Task.CompletedTask;
    }
}

public class PurchasePolicyData : ContainSagaData
{
    public Guid AccountId { get; set; }
    public int Balance { get; set; }
    public bool IsPreferredCustomer { get; set; }
    public Dictionary<DateTime, int> Purchases { get; set; } = new();
}