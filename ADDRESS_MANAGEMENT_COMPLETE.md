# ?? COMPLETE! Address Management CRUD System

## ? **What You Got:**

### **Complete Address Management System** with:
- ? **Full CRUD** (Create, Read, Update, Delete)
- ? **Primary Address** flag
- ? **Default Shipping** flag
- ? **Multiple addresses** per user
- ? **Beautiful Bootstrap Modal** UI
- ? **Soft delete** support
- ? **Toast notifications**
- ? **Form validation**

---

## ?? **Pages & URLs:**

### **1. Address List Page**
**URL:** `/Account/Profile/Addresses`

**Features:**
- Grid layout showing all addresses
- Primary & Shipping badges
- Edit/Delete buttons
- Set Primary/Shipping buttons
- Empty state with call-to-action
- Back to Profile link

### **2. Add/Edit Modal**
**Trigger:** Click "Add Address" or "Edit" button

**Form Fields:**
- Label (dropdown: Home, Work, Billing, Shipping, Other)
- Address Line 1 (required)
- Address Line 2 (optional)
- City, State, Postal Code (required)
- Country (default: USA)
- Phone Number (optional)
- Checkboxes: Set as Primary, Set as Default Shipping

---

## ?? **UI Design:**

### **Address Cards:**
```
???????????????????????????????????????
?  ?? Home          [Primary][Shipping]?
?  ???????                    [??][???]?
?  ?                                  ??
?  ? 123 Main Street, Apt 4B          ??
?  ? New York, NY 10001               ??
?  ? USA                              ??
?  ? ?? +1 555-1234                   ??
?  ?                                  ??
?  ? [Set as Primary] [Set as Shipping]?
?  ?????????????????????????????????????
???????????????????????????????????????
```

### **Add/Edit Modal:**
```
???????????????????????????????????????
?  Add Address                    [×] ?
???????????????????????????????????????
?  Label:        [Home ?]             ?
?  Address Line 1: [_____________]    ?
?  Address Line 2: [_____________]    ?
?  City:         [_____________]      ?
?  State:        [_____________]      ?
?  Postal Code:  [_______]            ?
?  Country:      [USA____________]    ?
?  Phone:        [_____________]      ?
?                                     ?
?  ? Set as Primary Address           ?
?  ? Set as Default Shipping          ?
?                                     ?
?  [Cancel]  [Save Address]           ?
???????????????????????????????????????
```

---

## ?? **Backend Architecture:**

### **Repository Methods:**

```csharp
IUserAddressRepository
?? GetByIdAsync(Guid id)
?? GetByUserProfileIdAsync(Guid userProfileId)
?? GetPrimaryAddressAsync(Guid userProfileId)
?? GetShippingDefaultAsync(Guid userProfileId)
?? AddAsync(UserAddress address)
?? UpdateAsync(UserAddress address)
?? DeleteAsync(Guid id)
?? SetPrimaryAsync(Guid addressId, Guid userProfileId)
?? SetShippingDefaultAsync(Guid addressId, Guid userProfileId)
```

### **Page Handlers:**

```csharp
AddressesModel
?? OnGetAsync()              - Load all addresses
?? OnPostAddAsync()          - Create new address
?? OnPostEditAsync()         - Update existing address
?? OnPostDeleteAsync(id)     - Delete address
?? OnPostSetPrimaryAsync(id) - Mark as primary
?? OnPostSetShippingAsync(id)- Mark as default shipping
```

---

## ?? **Key Features:**

### **1. Primary Address Logic:**
- Only **one** primary address per user
- Setting new primary automatically **unsets** others
- Highlighted with **border-primary** on card
- Shows **blue "Primary" badge**

### **2. Default Shipping Logic:**
- Only **one** default shipping per user
- Used automatically at checkout
- Shows **green "Shipping" badge**
- Can be different from primary

### **3. Smart Flags:**
When setting an address as Primary/Shipping:
```csharp
await _unitOfWork.UserAddresses.SetPrimaryAsync(addressId, profileId);
// This AUTOMATICALLY:
// 1. Sets IsPrimary = true for selected address
// 2. Sets IsPrimary = false for all other addresses
// 3. Updates LastUpdatedAt timestamp
```

### **4. Modal Form:**
- **Single modal** for both Add and Edit
- JavaScript switches form action dynamically
- Pre-fills data when editing
- Clears form when adding new

---

## ?? **User Workflows:**

### **Add New Address:**
```
1. Click "Add Address" button
2. Modal opens with empty form
3. Fill in fields
4. Optionally check "Set as Primary/Shipping"
5. Click "Save Address"
6. Toast: "Address added successfully!"
7. Modal closes, page refreshes
8. New address card appears
```

### **Edit Address:**
```
1. Click pencil icon on address card
2. Modal opens with pre-filled data
3. Modify fields
4. Click "Save Address"
5. Toast: "Address updated successfully!"
6. Changes reflect immediately
```

### **Delete Address:**
```
1. Click trash icon
2. Confirm dialog: "Delete this address?"
3. Click OK
4. Address removed
5. Toast: "Address deleted successfully!"
```

### **Set as Primary:**
```
1. Click "Set as Primary" button
2. Page refreshes
3. Toast: "Primary address updated!"
4. Blue "Primary" badge appears
5. Other cards lose primary badge
```

---

## ?? **Navigation Flow:**

```
Profile Page (/Account/Profile)
    ? "Manage Addresses" button
Address List (/Account/Profile/Addresses)
    ? "Back to Profile" link
Profile Page
```

---

## ?? **Database Integration:**

### **Cascade Delete:**
```
UserProfile (deleted)
    ? CASCADE
All UserAddresses (auto-deleted)
```

### **Ordering:**
Addresses displayed in order:
1. Primary first
2. Then Shipping Default
3. Then alphabetically by Label

### **Soft Delete:**
- Addresses marked as `IsDeleted = true`
- Still in database but hidden from queries
- Can be restored if needed

---

## ?? **UI States:**

### **Empty State:**
```
???????????????????????????????????
?                                 ?
?         ?? (large icon)         ?
?                                 ?
?      No Addresses Yet           ?
?   Add your first address to     ?
?   get started with orders       ?
?                                 ?
?   [+ Add Your First Address]    ?
?                                 ?
???????????????????????????????????
```

### **With Addresses:**
```
???????????????????????????????????
?  My Addresses    [Back] [+ Add] ?
???????????????????????????????????
?  ??????????  ??????????         ?
?  ? Home   ?  ? Work   ?         ?
?  ?[Primary]  ?        ?         ?
?  ?[Shipping] ?        ?         ?
?  ??????????  ??????????         ?
???????????????????????????????????
```

---

## ? **Toast Notifications:**

- **Success (Green):**
  - "Address added successfully!"
  - "Address updated successfully!"
  - "Address deleted successfully!"
  - "Primary address updated!"
  - "Default shipping address updated!"

- **Error (Red):**
  - "Address not found."
  - "Please complete your profile first."

---

## ?? **Usage in Checkout:**

```csharp
// Get shipping address for order
var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
var shippingAddress = await _unitOfWork.UserAddresses
    .GetShippingDefaultAsync(profile.Id);

if (shippingAddress == null)
{
    // Fallback to primary
    shippingAddress = await _unitOfWork.UserAddresses
        .GetPrimaryAddressAsync(profile.Id);
}

// Use shippingAddress for order
order.ShippingAddress = shippingAddress.FullAddress;
```

---

## ?? **Code Examples:**

### **Get All User Addresses:**
```csharp
var addresses = await _unitOfWork.UserAddresses
    .GetByUserProfileIdAsync(profileId);
```

### **Create New Address:**
```csharp
var address = new UserAddress
{
    Id = Guid.NewGuid(),
    UserProfileId = profileId,
    Label = "Home",
    AddressLine1 = "123 Main St",
    City = "New York",
    State = "NY",
    PostalCode = "10001",
    Country = "USA",
    IsPrimary = true,
    IsShippingDefault = true
};
await _unitOfWork.UserAddresses.AddAsync(address);
```

### **Update Address:**
```csharp
var address = await _unitOfWork.UserAddresses.GetByIdAsync(id);
address.AddressLine1 = "456 New St";
await _unitOfWork.UserAddresses.UpdateAsync(address);
```

---

## ?? **Best Practices:**

1. **Always have at least one address** before checkout
2. **Primary address** should be user's main address
3. **Shipping default** can be different (e.g., work for deliveries)
4. **Validate phone numbers** for delivery contact
5. **Country defaults to USA** but can be changed

---

## ? **Testing Checklist:**

- [ ] Add first address
- [ ] Add second address
- [ ] Edit an address
- [ ] Delete an address
- [ ] Set as primary
- [ ] Set as shipping default
- [ ] Verify only one primary exists
- [ ] Verify only one shipping default exists
- [ ] Check empty state display
- [ ] Check modal opens/closes
- [ ] Verify form validation
- [ ] Check toast notifications
- [ ] Test back button to profile
- [ ] Verify addresses show on profile page

---

## ?? **Summary:**

You now have a **complete, production-ready address management system** with:
- ? Beautiful Bootstrap UI
- ? Full CRUD operations
- ? Primary & Shipping flags
- ? Modal form for add/edit
- ? Toast notifications
- ? Form validation
- ? Soft delete
- ? Repository pattern
- ? Clean architecture

**Your users can now manage multiple addresses for shipping and billing!** ????
