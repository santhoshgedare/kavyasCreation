using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(AppDbContext db, ILogger<InventoryService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> CheckAvailabilityAsync(Guid productId, int quantity)
        {
            var product = await _db.Products.FindAsync(productId);
            return product is not null && product.AvailableStock >= quantity;
        }

        public async Task<StockReservation?> ReserveStockAsync(Guid productId, string userId, int quantity, int expirationMinutes = 15)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var product = await _db.Products.FindAsync(productId);
                if (product is null || product.AvailableStock < quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {Available}", 
                        productId, quantity, product?.AvailableStock ?? 0);
                    return null;
                }

                product.ReservedStock += quantity;

                var reservation = new StockReservation
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    UserId = userId,
                    Quantity = quantity,
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    IsCommitted = false,
                    IsCancelled = false
                };

                _db.StockReservations.Add(reservation);

                await RecordStockMovementAsync(productId, -quantity, "Reservation", userId, reservation.Id.ToString(), 
                    $"Reserved {quantity} units for user {userId}");

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock reserved: {Quantity} units of product {ProductId} for user {UserId}", 
                    quantity, productId, userId);

                return reservation;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Concurrency conflict while reserving stock for product {ProductId}", productId);
                return null;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> CommitReservationAsync(Guid reservationId, Guid orderId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _db.StockReservations
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation is null || reservation.IsCommitted || reservation.IsCancelled)
                {
                    _logger.LogWarning("Invalid reservation {ReservationId} for commit", reservationId);
                    return false;
                }

                if (reservation.Product is null)
                {
                    _logger.LogError("Product not found for reservation {ReservationId}", reservationId);
                    return false;
                }

                reservation.IsCommitted = true;
                reservation.OrderId = orderId;
                
                reservation.Product.ReservedStock -= reservation.Quantity;
                reservation.Product.Stock -= reservation.Quantity;

                await RecordStockMovementAsync(reservation.ProductId, -reservation.Quantity, "Sale", 
                    reservation.UserId, orderId.ToString(), $"Order {orderId} committed");

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reservation {ReservationId} committed for order {OrderId}", reservationId, orderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error committing reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(Guid reservationId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _db.StockReservations
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation is null || reservation.IsCommitted || reservation.IsCancelled)
                {
                    return false;
                }

                if (reservation.Product is not null)
                {
                    reservation.Product.ReservedStock -= reservation.Quantity;
                }

                reservation.IsCancelled = true;

                await RecordStockMovementAsync(reservation.ProductId, reservation.Quantity, "Release", 
                    reservation.UserId, reservationId.ToString(), "Reservation cancelled");

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reservation {ReservationId} cancelled", reservationId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task ReleaseExpiredReservationsAsync()
        {
            var expiredReservations = await _db.StockReservations
                .Include(r => r.Product)
                .Where(r => !r.IsCommitted && !r.IsCancelled && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                if (reservation.Product is not null)
                {
                    reservation.Product.ReservedStock -= reservation.Quantity;
                }
                reservation.IsCancelled = true;

                await RecordStockMovementAsync(reservation.ProductId, reservation.Quantity, "Release", 
                    "System", reservation.Id.ToString(), "Expired reservation released");
            }

            if (expiredReservations.Any())
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Released {Count} expired reservations", expiredReservations.Count);
            }
        }

        public async Task<bool> AdjustStockAsync(Guid productId, int quantity, string movementType, string performedBy, string? referenceId = null, string? notes = null)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var product = await _db.Products.FindAsync(productId);
                if (product is null)
                {
                    return false;
                }

                var stockBefore = product.Stock;
                product.Stock += quantity;

                if (product.Stock < 0)
                {
                    _logger.LogError("Stock adjustment would result in negative stock for product {ProductId}", productId);
                    return false;
                }

                await RecordStockMovementAsync(productId, quantity, movementType, performedBy, referenceId, notes, stockBefore);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock adjusted for product {ProductId}: {Quantity} ({MovementType})", 
                    productId, quantity, movementType);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adjusting stock for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<IReadOnlyList<StockMovement>> GetStockHistoryAsync(Guid productId, int count = 50)
        {
            return await _db.StockMovements
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync()
        {
            return await _db.Products
                .Include(p => p.Category)
                .Where(p => (p.Stock - p.ReservedStock) <= p.ReorderLevel)
                .OrderBy(p => p.Stock - p.ReservedStock)
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task RecordStockMovementAsync(Guid productId, int quantity, string movementType, 
            string performedBy, string? referenceId, string? notes, int? stockBefore = null)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product is null) return;

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity,
                StockBefore = stockBefore ?? product.Stock - quantity,
                StockAfter = product.Stock,
                MovementType = movementType,
                ReferenceId = referenceId,
                Notes = notes,
                PerformedBy = performedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.StockMovements.Add(movement);
        }
    }
}
