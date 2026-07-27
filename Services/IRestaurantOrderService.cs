using HMS.Models;
using HMS.Models.Common;
using HMS.Models.Restaurant;

namespace HMS.Services;

public interface IRestaurantOrderService
{
    Task<PagedList<RestaurantOrder>> GetPaginatedOrdersAsync(int page, int pageSize, RestaurantOrderStatus? statusFilter = null);
    Task<List<RestaurantOrder>> GetActiveKitchenOrdersAsync();
    Task<List<RestaurantOrder>> GetServedOrdersForCashierAsync();
    Task<RestaurantOrder?> GetOrderByIdAsync(int id);
    Task<(bool Success, string ErrorMessage, RestaurantOrder? Order)> CreateOrderAsync(int tableId, int waiterId, string? customerName, string? notes, int[] menuItemIds, int[] quantities);
    Task<bool> UpdateOrderStatusAsync(int orderId, RestaurantOrderStatus status);
    Task<bool> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod, decimal amountPaid);
}
