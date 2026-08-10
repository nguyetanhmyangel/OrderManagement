namespace OrderManagement.Domain.Inventories;

public enum InventoryTransactionType
{
    StockIn = 0,          // Nhập kho
    StockOut = 1,         // Xuất kho (bán hàng)
    Adjustment = 2,       // Điều chỉnh tồn kho
    TransferIn = 3,       // Chuyển kho đến
    TransferOut = 4,      // Chuyển kho đi
    Return = 5,           // Hàng trả về
    Reservation = 6,      // Giữ hàng (reserve)
    ReleaseReservation = 7 // Giải phóng hàng giữ
}
