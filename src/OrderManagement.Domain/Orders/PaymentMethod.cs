namespace OrderManagement.Domain.Orders;

public enum PaymentMethod
{
    CashOnDelivery = 0,
    BankTransfer = 1,
    CreditCard = 2,
    EWallet = 3,
    QRCode = 4
}
