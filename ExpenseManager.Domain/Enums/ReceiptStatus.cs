namespace ExpenseManager.Domain.Enums
{
    public enum ReceiptStatus
    {
        Pending = 0,
        Uploaded = 1,
        Failed = 2,
        Processing = 3,
        ReadyForReview = 4,
        Confirmed = 5,
        OcrFailed = 6
    }
}
