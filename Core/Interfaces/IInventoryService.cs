using Core.Entities;

namespace Core.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> CheckAvailabilityAsync(Guid productId, int quantity);
        Task<StockReservation?> ReserveStockAsync(Guid productId, string userId, int quantity, int expirationMinutes = 15);
        Task<bool> CommitReservationAsync(Guid reservationId, Guid orderId);
        Task<bool> CancelReservationAsync(Guid reservationId);
        Task ReleaseExpiredReservationsAsync();
        Task<bool> AdjustStockAsync(Guid productId, int quantity, string movementType, string performedBy, string? referenceId = null, string? notes = null);
        Task<IReadOnlyList<StockMovement>> GetStockHistoryAsync(Guid productId, int count = 50);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync();
    }
}
