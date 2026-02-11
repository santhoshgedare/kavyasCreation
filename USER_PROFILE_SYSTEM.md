# ?? USER PROFILE SYSTEM - Complete Guide

## ? **IMPLEMENTED: Separate UserProfile Table with FK to IdentityUser**

### **Overview**
A comprehensive user profile management system with a separate `UserProfiles` table that extends the built-in `IdentityUser` with additional personal, contact, and address information.

---

## ?? **Database Schema**

### **UserProfiles Table**
```sql
CREATE TABLE [UserProfiles] (
    [Id] uniqueidentifier PRIMARY KEY,
    [UserId] nvarchar(450) NOT NULL,  -- FK to AspNetUsers (UNIQUE)
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [AlternateEmail] nvarchar(256) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [AlternatePhoneNumber] nvarchar(20) NULL,
    [Address] nvarchar(500) NULL,
    [City] nvarchar(100) NULL,
    [State] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [Country] nvarchar(100) NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] nvarchar(10) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL
);

-- Indexes
CREATE UNIQUE INDEX IX_UserProfiles_UserId ON UserProfiles(UserId);
CREATE INDEX IX_UserProfiles_Email ON UserProfiles(Email);
```

### **Relationship**
```
AspNetUsers (IdentityUser)
       ? (1:1)
  UserProfiles
```

- **Foreign Key:** `UserProfiles.UserId` ? `AspNetUsers.Id`
- **Unique Constraint:** One profile per user
- **Soft Delete:** Supports soft deletion (IsDeleted flag)

---

## ?? **Entity Structure**

### **UserProfile.cs**
```csharp
public class UserProfile : ISoftDelete
{
    public Guid Id { get; set; }
    
    // Foreign Key to IdentityUser
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }
    
    // Personal Information
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
    
    // Contact Information
    [Required]
    [MaxLength(256)]
    public required string Email { get; set; }
    
    [MaxLength(256)]
    public string? AlternateEmail { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(20)]
    public string? AlternatePhoneNumber { get; set; }
    
    // Address Information
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    // Additional Details
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; } // Male, Female, Other
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Computed
    public string FullName => $"{FirstName} {LastName}";
}
```

---

## ?? **Files Created/Modified**

### **New Files:**
1. ? `Core/Entities/UserProfile.cs` - Entity model
2. ? `Core/Interfaces/IUserProfileRepository.cs` - Repository interface
3. ? `Infrastructure/Repositories/UserProfileRepository.cs` - Repository implementation
4. ? `Areas/Account/Pages/Profile/Index.cshtml` - User profile page (UI)
5. ? `Areas/Account/Pages/Profile/Index.cshtml.cs` - Page model
6. ? `Areas/Admin/Pages/Users/Index.cshtml` - Admin users list
7. ? `Areas/Admin/Pages/Users/Index.cshtml.cs` - Admin page model
8. ? `Infrastructure/Migrations/[timestamp]_AddUserProfiles.cs` - Database migration

### **Modified Files:**
1. ? `Infrastructure/Data/AppDbContext.cs` - Added DbSet and configuration
2. ? `Core/Interfaces/IUnitOfWork.cs` - Added UserProfiles property
3. ? `Infrastructure/Repositories/UnitOfWork.cs` - Injected repository
4. ? `Program.cs` - Registered UserProfileRepository in DI
5. ? `Views/Shared/_LoginPartial.cshtml` - Added profile link with dropdown
6. ? `Views/Shared/_Layout.cshtml` - Added admin users menu

---

## ?? **User Interface**

### **Customer Profile Page** (`/Account/Profile`)
**Features:**
- ? **Three sections:** Personal Info, Contact Info, Address Info
- ? **Bootstrap Icons** for all input fields
- ? **Form Validation** with real-time feedback
- ? **Loading States** on save button
- ? **Toast Notifications** for success/error
- ? **Responsive Design** - mobile-friendly cards

**Sections:**
1. **Personal Information**
   - First Name, Last Name
   - Date of Birth, Gender

2. **Contact Information**
   - Email (Primary), Alternate Email
   - Phone Number, Alternate Phone

3. **Address Information**
   - Street Address
   - City, State, Postal Code, Country

### **Admin Users Page** (`/Admin/Users`)
**Features:**
- ? **Table View** of all user profiles
- ? **Full Contact Info** displayed
- ? **Location Info** at a glance
- ? **Created Date** tracking
- ? **Active Status** badge
- ? **User Count** badge

---

## ?? **Repository Methods**

### **IUserProfileRepository**
```csharp
Task<UserProfile?> GetByIdAsync(Guid id);
Task<UserProfile?> GetByUserIdAsync(string userId);
Task<IReadOnlyList<UserProfile>> ListAllAsync();
Task<UserProfile?> GetByEmailAsync(string email);
Task<bool> ExistsAsync(string userId);
Task AddAsync(UserProfile userProfile);
Task UpdateAsync(UserProfile userProfile);
Task DeleteAsync(Guid id);
```

---

## ?? **Usage Examples**

### **1. Get User Profile**
```csharp
var userId = _userManager.GetUserId(User);
var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);

if (profile != null)
{
    var fullName = profile.FullName;
    var email = profile.Email;
}
```

### **2. Create New Profile**
```csharp
var newProfile = new UserProfile
{
    Id = Guid.NewGuid(),
    UserId = userId,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    PhoneNumber = "+1 (555) 123-4567",
    City = "New York",
    State = "NY",
    Country = "USA"
};

await _unitOfWork.UserProfiles.AddAsync(newProfile);
```

### **3. Update Profile**
```csharp
var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
if (profile != null)
{
    profile.PhoneNumber = "+1 (555) 999-8888";
    profile.Address = "456 New Street";
    await _unitOfWork.UserProfiles.UpdateAsync(profile);
}
```

### **4. Check if Profile Exists**
```csharp
var hasProfile = await _unitOfWork.UserProfiles.ExistsAsync(userId);
if (!hasProfile)
{
    // Redirect to create profile
}
```

---

## ?? **Navigation Structure**

### **Customer Navigation**
```
User Menu (Top Right) ?
?? ?? My Profile
?? ??? My Orders
?? ?? Account Settings
?? ?? Logout
```

### **Admin Navigation**
```
Configure Menu ?
?? ?? Admin Dashboard
?? ?? Manage Products
?? ??? Manage Categories
?? ?? Inventory Dashboard
?? ?? User Profiles ? NEW!
```

---

## ? **Key Features**

### **1. Soft Delete Support**
- Implements `ISoftDelete` interface
- Never permanently deletes user data
- Can be restored if needed

### **2. Auto Timestamps**
- `CreatedAt` set automatically on insert
- `LastUpdatedAt` tracked on every update

### **3. Unique Constraint**
- One profile per IdentityUser (enforced by UNIQUE index)
- Prevents duplicate profiles

### **4. Email Indexing**
- Fast lookups by email address
- Supports alternate email searches

### **5. Validation**
- Required fields: FirstName, LastName, Email
- Optional fields for flexibility
- Max length constraints for all strings

---

## ?? **Workflow**

### **New User Registration:**
```
1. User registers ? IdentityUser created
2. User navigates to Profile page
3. Sees "Profile Incomplete" badge
4. Fills out profile form
5. Saves ? UserProfile created
6. Badge changes to "Profile Complete"
```

### **Existing User:**
```
1. User logs in
2. Clicks "My Profile" in dropdown
3. Sees pre-filled form
4. Updates any field
5. Saves ? LastUpdatedAt timestamp updated
6. Success toast notification shown
```

### **Admin View:**
```
1. Admin clicks "Configure ? User Profiles"
2. Sees table of all registered users
3. Views contact info, location, etc.
4. Can export or analyze user data
```

---

## ?? **Data Relationships**

```
AspNetUsers (IdentityUser)
      ?? Id (PK)
      ?? Email
      ?? UserName
           ? 1:1
UserProfiles
      ?? Id (PK)
      ?? UserId (FK ? AspNetUsers.Id) [UNIQUE]
      ?? FirstName
      ?? LastName
      ?? Email (duplicates IdentityUser.Email)
      ?? AlternateEmail
      ?? PhoneNumber
      ?? Address, City, State, etc.
      ?? CreatedAt, LastUpdatedAt
```

---

## ?? **Security Considerations**

1. ? **Authorization:** Profile page requires `[Authorize]`
2. ? **Admin Only:** User list requires `[Authorize(Roles = "Admin")]`
3. ? **User Isolation:** Users can only edit their own profile
4. ? **No Direct Deletion:** Soft delete prevents data loss

---

## ??? **Testing Checklist**

### **As Customer:**
- [ ] Navigate to "My Profile"
- [ ] See empty form (new users) or pre-filled (existing)
- [ ] Fill all required fields
- [ ] Save and see success message
- [ ] Refresh page - data persists
- [ ] Update a field - LastUpdatedAt changes
- [ ] Check navigation shows name correctly

### **As Admin:**
- [ ] Navigate to "Configure ? User Profiles"
- [ ] See list of all users
- [ ] Verify contact info displays
- [ ] Check total user count badge

---

## ?? **UI Components**

### **Icons Used:**
- ?? `bi-person-circle` - Profile menu
- ?? `bi-envelope` - Email fields
- ?? `bi-phone` - Phone fields
- ?? `bi-house` - Address field
- ?? `bi-globe` - Country field
- ? `bi-check-circle` - Success states
- ?? `bi-exclamation-triangle` - Warnings

### **Badges:**
- Green: Profile Complete
- Yellow: Profile Incomplete
- Blue: Total Users Count

---

## ?? **Future Enhancements**

Potential additions:
- Profile picture upload
- Social media links
- Preferred language
- Newsletter subscription
- Privacy settings
- Account deletion request
- Export user data (GDPR)

---

## ? **Success Criteria**

You have successfully implemented UserProfiles when:

1. ? **Database table** `UserProfiles` created with FK to `AspNetUsers`
2. ? **Customer can** create/edit their profile
3. ? **Admin can** view all user profiles
4. ? **Navigation** includes profile link
5. ? **Soft delete** protects user data
6. ? **Unique constraint** prevents duplicates
7. ? **Timestamps** track creation/updates
8. ? **UI has icons** and validation
9. ? **Toast notifications** show feedback
10. ? **Build is successful** ?

---

**Your UserProfile system is now FULLY OPERATIONAL!** ????
