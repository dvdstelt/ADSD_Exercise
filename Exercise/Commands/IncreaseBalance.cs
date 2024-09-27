namespace Exercise.Commands;

public class IncreaseBalance : ICommand
{
    public int AddedBalance { get; set; }
    public Guid AccountId { get; set; }
}