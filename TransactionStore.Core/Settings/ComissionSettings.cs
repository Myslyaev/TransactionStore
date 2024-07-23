namespace TransactionStore.Core.Settings;

public class ComissionSettings(string deposit, string withdraw, string transfer)
{
    public string Deposit { get; } = deposit;
    public string Withdraw { get; } = withdraw;
    public string Transfer { get; } = transfer;
}