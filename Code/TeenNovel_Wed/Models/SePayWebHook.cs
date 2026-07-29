public class SePayWebhook
{
    public string TransactionId { get; set; } = "";

    public decimal Amount { get; set; }

    public string Content { get; set; } = "";

    public string BankAccount { get; set; } = "";

    public DateTime TransactionDate { get; set; }
}