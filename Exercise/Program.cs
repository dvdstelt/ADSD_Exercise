using Exercise.Commands;
using Exercise.Policies;

namespace Exercise;

class Program
{
    static readonly Random random = new Random();
    static readonly Timer timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
    static IEndpointInstance endpoint;
    static readonly Guid accountId = Guid.Parse("da878ef6-f11c-4182-a8ec-3a50eb4fc4fd");

    static async Task Main(string[] args)
    {
        var endpointConfig = new EndpointConfiguration("GameEngine");
        var transport = endpointConfig.UseTransport<LearningTransport>();
        endpointConfig.UsePersistence<LearningPersistence>();
        endpointConfig.UseSerialization<SystemJsonSerializer>();
        
        endpoint = await Endpoint.Start(endpointConfig);

        Console.WriteLine("\nGame engine started.");
        Console.WriteLine("Press [1] to buy a random item");
        Console.WriteLine("Press [2] to buy an expensive random item");
        
        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();
            
            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    await BuyItem(random.Next(25));
                    continue;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    await BuyItem(random.Next(25, 100));
                    continue;
                case ConsoleKey.Escape:
                    break;
                default:
                    return;
            }
        }
    }

    private static async Task BuyItem(int price)
    {
        var message = new BuyItem()
        {
            AccountId = accountId,
            ItemId = Guid.NewGuid(),
            Price = price
        };

        await endpoint.SendLocal(message);
    }

    private static async void TimerCallback(object state)
    {
        var message = new IncreaseBalance() { AccountId = accountId, AddedBalance = random.Next(1, 10) };
        await endpoint.SendLocal(message);
        
        var nextInterval = random.Next(2000, 5000);
        timer.Change(nextInterval, Timeout.Infinite);
    }
}