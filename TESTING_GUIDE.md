# ?? Testing Guide - Vendor & Buyer Registration

## ? Ready to Test!

**All features are implemented and building successfully.**

---

## ?? How to Test the Complete Flow

### Step 1: Register a New Vendor

1. **Run the project:**
   ```bash
   dotnet run --project Web/Web.csproj --launch-profile https
   ```

2. **Navigate to Vendor Registration:**
   - URL: `https://localhost:7086/Identity/Account/RegisterVendor`
   - Or click: Register ? "Register as Vendor"

3. **Fill in the registration form:**
   ```
   Company Information:
   - Company Name: Test Vendor Inc
   - Company Email: contact@testvendor.com
   - Company Phone: +1-555-1234
   - Tax ID: TAX123456
   - Description: We sell amazing products
   - Address: 123 Main St
   - City: New York
   - State: NY
   - Postal Code: 10001
   - Country: USA

   Admin User Information:
   - First Name: John
   - Last Name: Doe
   - Email: john@testvendor.com (this will be login email)
   - Phone: +1-555-5678
   - Job Title: CEO
   - Password: Test123!@#
   - Confirm Password: Test123!@#
   ```

4. **Submit the form**
   - You should see: "Registration successful! Your application is pending admin approval."

5. **Try to login as vendor (it will fail):**
   - Email: `john@testvendor.com`
   - Password: `Test123!@#`
   - Should show: Access denied or profile inactive (because not yet approved)

---

### Step 2: Approve the Vendor (as Admin)

1. **Login as Admin:**
   - Email: `admin@local`
   - Password: `Admin123!` (check User Secrets if different)

2. **Navigate to Pending Vendors:**
   - Click: Admin dropdown ? "Pending Vendors"
   - URL: `https://localhost:7086/Admin/Vendors/Pending`

3. **You should see:**
   - Card showing "Test Vendor Inc"
   - Company details
   - Admin contact (John Doe)
   - Two buttons: "Approve" and "Reject"

4. **Click "Approve"**
   - Success message should appear
   - Vendor disappears from pending list

5. **What happened behind the scenes:**
   ```
   Vendor table:
   - IsApproved = true
   - IsActive = true
   - ApprovedAt = current timestamp
   - ApprovedBy = admin user ID

   VendorUser table:
   - IsActive = true
   ```

---

### Step 3: Login as Approved Vendor

1. **Logout from admin account**

2. **Login as vendor:**
   - Email: `john@testvendor.com`
   - Password: `Test123!@#`

3. **Success! You should now see:**
   - "Vendor" dropdown in navigation
   - Click it to see: Dashboard, My Products, Orders, Buyers

4. **Navigate to Vendor Dashboard:**
   - Click: Vendor ? Dashboard
   - URL: `https://localhost:7086/Vendor/Dashboard`

5. **You should see:**
   - Welcome message: "Welcome, Test Vendor Inc"
   - KPI cards showing:
     - Total Products: 0
     - Total Orders: 0
     - Total Revenue: ?0.00
     - Low Stock Items: 0
   - Quick action buttons

6. **Navigate to My Products:**
   - Click: Vendor ? My Products
   - URL: `https://localhost:7086/Vendor/Products`

7. **You should see:**
   - "No Products Found" message
   - "Add Your First Product" button
   - Filter tabs: All, Active, Inactive, Low Stock

---

### Step 4: Test Buyer Registration (Same Process)

1. **Navigate to Buyer Registration:**
   - URL: `https://localhost:7086/Identity/Account/RegisterBuyer`

2. **Fill similar form:**
   ```
   Company: Test Buyer Company
   Email: contact@testbuyer.com
   Admin: Jane Smith, jane@testbuyer.com
   Password: Test123!@#
   ```

3. **Submit and wait for approval**

4. **Admin approves at:** `/Admin/Buyers/Pending` (TODO - not created yet)

5. **Buyer logs in and sees:** Buyer dropdown menu

---

## ?? Database Verification

You can verify the data was created correctly:

### Check Identity User:
```sql
SELECT Id, UserName, Email, EmailConfirmed 
FROM AspNetUsers 
WHERE Email = 'john@testvendor.com';
```

### Check Vendor:
```sql
SELECT Id, CompanyName, ContactEmail, IsApproved, IsActive, CreatedAt, ApprovedAt 
FROM Vendors 
WHERE CompanyName = 'Test Vendor Inc';
```

### Check VendorUser:
```sql
SELECT vu.Id, vu.FirstName, vu.LastName, vu.Email, vu.IsAdmin, vu.IsActive,
       v.CompanyName, u.UserName
FROM VendorUsers vu
JOIN Vendors v ON vu.VendorId = v.Id
JOIN AspNetUsers u ON vu.UserId = u.Id
WHERE vu.Email = 'john@testvendor.com';
```

### Check Role Assignment:
```sql
SELECT u.Email, r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'john@testvendor.com';
```

Expected: VendorAdmin role

---

## ?? Troubleshooting

### Problem: "Vendor role not found"
**Solution:** Make sure all roles are seeded in Program.cs:
```csharp
// In Program.cs
foreach (var role in Roles.All)
{
    if (!await roleManager.RoleExistsAsync(role))
    {
        await roleManager.CreateAsync(new IdentityRole(role));
    }
}
```

Roles should include:
- Admin
- VendorAdmin
- VendorUser
- BuyerAdmin
- BuyerUser
- Customer

### Problem: "Vendor not found after login"
**Check:**
1. VendorUser.UserId matches AspNetUsers.Id
2. VendorUser.IsActive = true
3. Vendor.IsApproved = true
4. Vendor.IsActive = true

### Problem: "Pending Vendors page shows nothing"
**Check:**
1. Vendor.IsApproved = false (should be false before approval)
2. Vendor.IsDeleted = false
3. Repository method `GetAllAsync()` returns non-deleted vendors

### Problem: "Can't see Vendor dropdown menu"
**Check:**
1. User has "VendorAdmin" or "VendorUser" role in AspNetUserRoles
2. VendorUser.IsActive = true
3. Logged in user's email matches VendorUser.Email

---

## ?? Expected User Journey

```
Day 1: Vendor Signs Up
?? Creates account on /RegisterVendor
?? Receives: "Pending approval" message
?? Cannot login (inactive)
?? Waits for email

Day 2: Admin Reviews
?? Admin logs in
?? Sees vendor in /Vendors/Pending
?? Reviews company info
?? Clicks "Approve"
?? System sends email (TODO)

Day 3: Vendor Activates
?? Vendor receives approval email
?? Logs in with credentials
?? Sees Vendor dropdown
?? Accesses dashboard
?? Adds first product
?? Starts selling!
```

---

## ?? Next Steps After Testing

Once you verify the flow works:

1. ? **Create Buyer Approval Page** (copy from Vendor)
   - `/Areas/Admin/Pages/Buyers/Pending.cshtml`

2. ? **Add Email Notifications**
   - Send approval email to vendor
   - Send welcome email
   - Configure SMTP settings

3. ? **Allow Vendors to Add More Users**
   - `/Areas/Vendor/Pages/Users/Create.cshtml`
   - Invite team members

4. ? **Create "All Vendors" List for Admin**
   - `/Areas/Admin/Pages/Vendors/Index.cshtml`
   - View, edit, deactivate vendors

5. ? **Add Validation**
   - Duplicate email check
   - Tax ID format validation
   - Phone number formatting

---

## ?? Test Checklist

- [ ] Vendor can register
- [ ] Registration creates all 3 records (User, Vendor, VendorUser)
- [ ] Vendor cannot login before approval
- [ ] Admin can see pending vendor
- [ ] Admin can approve vendor
- [ ] Approval activates vendor and vendor user
- [ ] Vendor can login after approval
- [ ] Vendor sees "Vendor" dropdown
- [ ] Vendor can access dashboard
- [ ] Vendor can access products page
- [ ] Same flow works for Buyer registration

---

## ?? Success Criteria

Your system is working correctly when:

? New vendors can self-register  
? System prevents unapproved access  
? Admin has full control over approvals  
? Approved vendors can access their dashboard  
? Role-based menus show correctly  
? All database relationships are correct  

---

**Ready to test! Run the project and follow the steps above.** ??
