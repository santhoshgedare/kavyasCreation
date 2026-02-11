# ?? GOOGLE SIGN-IN TROUBLESHOOTING

## ? **Your Credentials (from appsettings.json):**

```
Google Client ID: 1088083900269-jc9o97nffg83uun7ur75shkkhkrus2lt.apps.googleusercontent.com
Google Client Secret: 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
```

---

## ?? **MOST COMMON ISSUE: Redirect URI Mismatch**

### **What's Happening:**
When you click "Continue with Google", Google redirects back to:
```
https://localhost:5001/signin-google
```

But this URI must be EXACTLY registered in Google Cloud Console!

---

## ?? **QUICK FIX (5 steps):**

### **Step 1: Go to Google Cloud Console**
```
https://console.cloud.google.com/apis/credentials
```

### **Step 2: Find Your OAuth Client**
- Look for: `1088083900269-jc9o97nffg83uun7ur75shkkhkrus2lt`
- Click on it to edit

### **Step 3: Add Redirect URIs**
In "Authorized redirect URIs" section, add **ALL** of these:

```
https://localhost:5001/signin-google
https://localhost:7001/signin-google
http://localhost:5000/signin-google
```

### **Step 4: Save**
- Click "SAVE" at bottom
- Wait 5 minutes for changes to propagate

### **Step 5: Test Again**
```powershell
# Stop app (Ctrl+C)
# Restart
dotnet run
```

Navigate to: `https://localhost:5001/Identity/Account/Login`

Click "Continue with Google" ? Should work! ?

---

## ?? **How to See the Actual Error:**

### **Open Browser DevTools:**
1. Press `F12` in your browser
2. Go to "Console" tab
3. Click "Continue with Google"
4. Look for error messages in console

### **Check App Logs:**
Look at your terminal where `dotnet run` is running. You should see:
```
External login initiated for provider: Google
```

If you see errors, they'll tell you what's wrong!

---

## ??? **Other Common Issues:**

### **Issue 1: "redirect_uri_mismatch"**

**Error in Browser:**
```
Error 400: redirect_uri_mismatch
The redirect URI in the request does not match
```

**Fix:**
- Add EXACT URI to Google Cloud Console (see Step 3 above)
- NO trailing slash!
- Match https/http protocol exactly

---

### **Issue 2: "Access Blocked" or "This app is blocked"**

**Error:**
```
This app is blocked
This app tried to access sensitive info in your Google Account
```

**Fix:**
1. Google Cloud Console ? OAuth consent screen
2. Publishing status: Click "PUBLISH APP"
3. OR add your email to "Test users" list

---

### **Issue 3: Redirect Works but Not Logged In**

**Symptoms:**
- Redirects to Google ?
- Logs in with Google ?  
- Redirects back to app ?
- But NOT logged in ?

**Fix:**
I just updated `Program.cs` to set `RequireConfirmedAccount = false`

Restart your app:
```powershell
# Stop (Ctrl+C)
dotnet run
```

---

### **Issue 4: "Email not received from provider"**

**Error:** TempData shows this message

**Fix:**
1. Google Cloud Console ? OAuth consent screen
2. Scopes ? Ensure "email" scope is included
3. Try logging in again
4. Grant email permission when prompted

---

## ?? **EXACT STEPS TO TEST:**

```
1. Stop app (Ctrl+C in terminal)

2. Restart:
   dotnet run

3. Open browser:
   https://localhost:5001/Identity/Account/Login

4. Press F12 ? Go to Console tab

5. Click [?? Continue with Google]

6. If redirect URI error:
   - Go to Google Cloud Console
   - Add redirect URI (see Step 3 above)
   - Wait 5 minutes
   - Try again

7. If works:
   - Login with your Google account
   - Grant permissions
   - Should redirect back and login! ?
```

---

## ?? **Check Database After Successful Login:**

```sql
-- Check if user was created
SELECT * FROM AspNetUsers 
ORDER BY Id DESC;

-- Check if Google login was linked
SELECT * FROM AspNetUserLogins 
WHERE LoginProvider = 'Google'
ORDER BY UserId DESC;
```

---

## ? **Verification Checklist:**

- [ ] Credentials in appsettings.json are correct
- [ ] Redirect URI added to Google Cloud Console
- [ ] Redirect URI matches EXACTLY (no trailing slash)
- [ ] App restarted after changes
- [ ] Browser dev tools open to see errors
- [ ] OAuth consent screen configured
- [ ] App published OR email added to test users

---

## ?? **If Still Not Working:**

### **Enable Detailed Logging:**

Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

Restart app and check logs for detailed error messages!

---

## ?? **Quick Commands:**

```powershell
# Restart app
dotnet run

# Check if app is running
# Look for: "Now listening on: https://localhost:5001"

# Test login
# Navigate to: https://localhost:5001/Identity/Account/Login
```

---

**Most likely issue: Redirect URI not registered in Google Cloud Console**

**Fix: Add all redirect URIs from Step 3 above and wait 5 minutes!** ?
