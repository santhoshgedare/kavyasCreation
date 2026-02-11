# ?? Stock Management Flow - Complete Guide

## ?? **HOW STOCK GETS REDUCED WHEN ITEMS ARE ORDERED**

### **Complete Order Flow:**

```
???????????????????????????????????????????????????????????????
?                    CUSTOMER JOURNEY                          ?
???????????????????????????????????????????????????????????????

1?? BROWSE PRODUCTS
   ?
   User views catalog ? No stock changes
   Product.Stock = 100
   Product.ReservedStock = 0
   Product.AvailableStock = 100

2?? ADD TO CART
   ?
   User adds item (qty: 3) ? No stock changes (session only)
   Product.Stock = 100
   Product.ReservedStock = 0
   Product.AvailableStock = 100
   Cart (Session) = 3 items

3?? VIEW CART
   ?
   System checks availability ? Shows warnings if low stock
   ? If Available >= Requested ? Show "X available" badge
   ? If Available < Requested ? Show "Only X in stock" (RED)

4?? CHECKOUT / PAYMENT PAGE
   ?
   User clicks "Complete Payment & Place Order"
   
   ????????????????????????????????????????????????
   ?  TRANSACTION STARTS (Database Transaction)   ?
   ????????????????????????????????????????????????
   
   ? STEP 1: VALIDATE STOCK AVAILABILITY
      foreach item in cart:
         Check: product.AvailableStock >= item.Quantity
         If NOT ? Cancel & show error
   
   ? STEP 2: RESERVE STOCK (15-minute lock)
      foreach item in cart:
         ReserveStockAsync(productId, userId, quantity, 15 min)
         
      Changes:
      Product.Stock = 100 (unchanged)
      Product.ReservedStock = 3 ? (locked)
      Product.AvailableStock = 97 (100 - 3)
      
      StockReservation table:
      - Id: <guid>
      - ProductId: <product-id>
      - UserId: <user-id>
      - Quantity: 3
      - ExpiresAt: Now + 15 minutes
      - IsCommitted: false
      
      StockMovement audit:
      - MovementType: "Reservation"
      - Quantity: -3
      - StockBefore: 100
      - StockAfter: 100
   
   ? STEP 3: CREATE ORDER
      Order table:
      - Id: <order-id>
      - UserId: <user-id>
      - Total: $299.97
      - Items: [{ ProductId, Quantity: 3 }]
   
   ? STEP 4: COMMIT RESERVATIONS (ACTUAL STOCK REDUCTION)
      foreach reservation:
         CommitReservationAsync(reservationId, orderId)
      
      Changes:
      Product.Stock = 97 ? (REDUCED!)
      Product.ReservedStock = 0 (released)
      Product.AvailableStock = 97
      
      StockReservation:
      - IsCommitted: true ?
      - OrderId: <order-id>
      
      StockMovement audit:
      - MovementType: "Sale"
      - Quantity: -3
      - StockBefore: 100
      - StockAfter: 97
      - ReferenceId: <order-id>
   
   ????????????????????????????????????????????????
   ?  TRANSACTION COMMITS                         ?
   ????????????????????????????????????????????????

5?? ORDER COMPLETE
   ?
   Cart cleared
   User sees: "Payment Successful!"
   Stock permanently reduced: 100 ? 97

???????????????????????????????????????????????????????????????
?              WHAT HAPPENS IN DIFFERENT SCENARIOS             ?
???????????????????????????????????????????????????????????????

? SCENARIO A: SUCCESSFUL PAYMENT
   Reserve ? Commit ? Stock reduced ? Order placed

? SCENARIO B: PAYMENT FAILS
   Reserve ? Cancel ? Stock released ? No order

?? SCENARIO C: 15-MINUTE TIMEOUT
   Reserve ? Background Service releases ? Stock released

?? SCENARIO D: CONCURRENT ORDERS
   User 1 reserves ? Lock applied
   User 2 tries to reserve same stock ? Fails (not enough available)
   User 1 completes ? Stock reduced
   User 2 gets "Only X in stock" error
```

---

## ?? **DATABASE CHANGES STEP-BY-STEP**

### **Initial State:**
```sql
Product:
  Id: {product-guid}
  Stock: 100
  ReservedStock: 0
  RowVersion: 0x00000001

StockReservations: (empty)
StockMovements: (empty)
Orders: (empty)
```

### **After Reservation:**
```sql
Product:
  Id: {product-guid}
  Stock: 100          ? Unchanged
  ReservedStock: 3    ? Locked
  RowVersion: 0x00000002  ? Concurrency check

StockReservations:
  Id: {reservation-guid}
  ProductId: {product-guid}
  UserId: {user-guid}
  Quantity: 3
  ReservedAt: 2026-01-27 14:30:00
  ExpiresAt: 2026-01-27 14:45:00
  IsCommitted: false
  IsCancelled: false

StockMovements:
  Id: {movement-guid}
  ProductId: {product-guid}
  Quantity: -3
  StockBefore: 100
  StockAfter: 100
  MovementType: "Reservation"
  PerformedBy: "user@example.com"
  CreatedAt: 2026-01-27 14:30:00
```

### **After Commit (Order Placed):**
```sql
Product:
  Id: {product-guid}
  Stock: 97           ? REDUCED!
  ReservedStock: 0    ? Released
  RowVersion: 0x00000003

StockReservations:
  Id: {reservation-guid}
  IsCommitted: true   ? Finalized
  OrderId: {order-guid}

StockMovements: (2 records now)
  1. Reservation (existing)
  2. Sale:
     Quantity: -3
     StockBefore: 100
     StockAfter: 97
     MovementType: "Sale"
     ReferenceId: {order-guid}

Orders:
  Id: {order-guid}
  UserId: {user-guid}
  Total: $299.97
  Items: [{ ProductId, Quantity: 3, Price: $99.99 }]
```

---

## ??? **CONCURRENCY PROTECTION**

### **Scenario: 2 Users Order Last 5 Items**

```
Initial: Stock = 5, Reserved = 0, Available = 5

User A (wants 3):
  ? Reserve ? Reserved = 3, Available = 2

User B (wants 3):
  ? Reserve fails ? Available (2) < Requested (3)
  ? Error: "Only 2 available"

User A:
  ? Commits ? Stock = 2, Reserved = 0

User B:
  Can now order 2 items maximum
```

**Protected by:**
- Row versioning (`RowVersion`)
- Transaction isolation
- Real-time availability calculation
- Optimistic concurrency control

---

## ?? **AUTOMATIC CLEANUP**

### **Background Service (Every 5 Minutes):**

```csharp
StockReservationCleanupService:
  1. Find: ExpiresAt < Now AND !IsCommitted AND !IsCancelled
  2. For each expired:
     - Product.ReservedStock -= Quantity
     - Reservation.IsCancelled = true
     - Record StockMovement (type: "Release")
  3. Save changes
```

**Result:**
- Abandoned carts don't lock stock forever
- Stock auto-releases after 15 minutes
- No manual intervention needed

---

## ?? **ADMIN VISIBILITY**

### **Inventory Dashboard Shows:**

```
Total Products: 50
Low Stock: 3 (?? warning)
Out of Stock: 1 (? critical)
Inventory Value: $45,230.00

Low Stock Table:
Product          Stock  Reserved  Available  Reorder
Headphones         8       3         5         10
Smart Watch        12      2         10        15
```

### **Stock Movement History:**

```
Date/Time             Type          Qty  Before  After  By
2026-01-27 14:35:00  Sale           -3   100     97    Order {guid}
2026-01-27 14:34:00  Reservation    -3   100     100   user@example.com
2026-01-27 10:00:00  Purchase       +50  50      100   admin@local
2026-01-27 09:00:00  Adjustment     -2   52      50    admin@local
```

---

## ? **VERIFICATION CHECKLIST**

? Stock validates before reservation
? Reservations lock stock (ReservedStock)
? Commit reduces actual stock
? Cancel/expire releases stock
? Concurrent orders prevented
? Complete audit trail
? Background cleanup runs
? Admin can track everything
? Cart shows availability warnings
? Payment page validates stock
? Transaction-safe operations

---

## ?? **KEY POINTS**

1. **Stock is NOT reduced when:**
   - Browsing products
   - Adding to cart
   - Viewing cart

2. **Stock IS reduced when:**
   - Payment completes ? `CommitReservationAsync()`
   - Order is placed successfully

3. **Stock is LOCKED (reserved) during:**
   - Checkout process (15 minutes)
   - Payment flow

4. **Protection mechanisms:**
   - Row versioning (concurrency)
   - Transaction safety
   - Real-time validation
   - Automatic cleanup

5. **Visibility:**
   - Complete audit trail
   - Admin dashboard
   - Stock movement history
   - User warnings in cart

---

## ?? **TESTING THE FLOW**

### **Test 1: Normal Order**
1. Add 3 items to cart
2. Go to payment
3. Click "Complete Payment"
4. ? Check: Stock reduced from 100 ? 97

### **Test 2: Insufficient Stock**
1. Product has 5 available
2. Add 10 to cart
3. Go to payment
4. ? Check: Error "Only 5 available"

### **Test 3: Concurrent Orders**
1. User A reserves 8 (out of 10)
2. User B tries to reserve 5
3. ? Check: User B gets "Only 2 available"

### **Test 4: Expiration**
1. Add to cart, go to payment
2. Wait 16 minutes (don't complete)
3. ? Check: Stock auto-released

---

## ?? **CODE REFERENCES**

- **Reservation:** `InventoryService.ReserveStockAsync()`
- **Commit:** `InventoryService.CommitReservationAsync()`
- **Validation:** `Payment/Index.cshtml.cs` lines 35-68
- **Cleanup:** `StockReservationCleanupService.cs`
- **Audit:** `StockMovement` entity
- **Dashboard:** `Admin/Inventory/Dashboard.cshtml`

---

**? Your stock management is now PRODUCTION-READY!**
