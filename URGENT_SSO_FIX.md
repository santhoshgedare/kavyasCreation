# ?? URGENT FIX: Google & Facebook SSO Not Working

## ?? **GOOGLE: Not Redirecting**

### **Problem:**
Google button clicks but nothing happens

### **FIX (2 minutes):**

1. **Go to Google Cloud Console:**
   ```
   https://console.cloud.google.com/apis/credentials
   ```

2. **Click your OAuth Client** (the one with Client ID: 1088083900269...)

3. **Scroll to "Authorized redirect URIs"**

4. **REMOVE:**
   ```
   ? https://localhost:44387
   ```

5. **ADD THESE EXACT URIs:**
   ```
   ? https://localhost:5001/signin-google
   ? https://localhost:7001/signin-google
   ? http://localhost:5000/signin-google
   ```

6. **Click "SAVE"**

7. **Wait 2 minutes** and test again

---

## ?? **FACEBOOK: "Can't load URL" Error**

### **Problem:**
```
The domain of this URL isn't included in the app's domains.
To be able to load this URL, add all domains and sub-domains
of your app to the App Domains field in your app settings.
```

### **FIX (3 minutes):**

#### **Step 1: Add App Domain**

1. **Go to Facebook Developers:**
   ```
   https://developers.facebook.com/apps/
   ```

2. **Select your app** (App ID: 1206205188266496)

3. **Left sidebar ? Settings ? Basic**

4. **Scroll to "App Domains"**

5. **Add:**
   ```
   localhost
   ```

6. **Click "Save Changes"**

#### **Step 2: Configure Facebook Login Settings**

1. **Left sidebar ? Products ? Facebook Login ? Settings**

2. **Under "Valid OAuth Redirect URIs", add:**
   ```
   https://localhost:5001/signin-facebook
   https://localhost:7001/signin-facebook
   http://localhost:5000/signin-facebook
   ```

3. **Click "Save Changes"**

#### **Step 3: Make App Live (IMPORTANT)**

1. **Top right corner:** See the toggle switch?

2. **Switch from "Development" to "Live"**
   - Click the toggle
   - Confirm when prompted

3. **If it asks for Privacy Policy:**
   - You can skip for testing
   - OR use a placeholder: `https://localhost:5001/privacy`

---

## ? **COMPLETE CONFIGURATION:**

### **Google Cloud Console:**

```
Client ID: 1088083900269-jc9o97nffg83uun7ur75shkkhkrus2lt

Authorized JavaScript origins:
? https://localhost:5001
? http://localhost:5000

Authorized redirect URIs:
? https://localhost:5001/signin-google
? https://localhost:7001/signin-google
? http://localhost:5000/signin-google
```

### **Facebook Developers:**

```
App ID: 1206205188266496

Settings ? Basic:
? App Domains: localhost

Facebook Login ? Settings:
? Valid OAuth Redirect URIs:
   https://localhost:5001/signin-facebook
   https://localhost:7001/signin-facebook
   http://localhost:5000/signin-facebook

App Status:
? LIVE (not Development)
```

---

## ?? **TESTING:**

After making these changes:

1. **Wait 2-5 minutes** for changes to propagate

2. **Restart your app:**
   ```powershell
   # Stop (Ctrl+C)
   dotnet run
   ```

3. **Open browser with DevTools:**
   ```
   Press F12
   Go to Console tab
   ```

4. **Navigate to:**
   ```
   https://localhost:5001/Identity/Account/Login
   ```

5. **Test Google:**
   - Click "Continue with Google"
   - Should redirect to Google login ?
   - Login with Google account
   - Should redirect back and sign you in ?

6. **Test Facebook:**
   - Click "Continue with Facebook"
   - Should redirect to Facebook login ?
   - Login with Facebook account
   - Should redirect back and sign you in ?

---

## ?? **VERIFY YOUR SETUP:**

### **Check Google Cloud Console:**

1. Go to: https://console.cloud.google.com/apis/credentials
2. Click your OAuth client
3. Verify you see:
   ```
   Authorized redirect URIs:
   1. https://localhost:5001/signin-google
   2. https://localhost:7001/signin-google
   3. http://localhost:5000/signin-google
   ```

### **Check Facebook Developers:**

1. Go to: https://developers.facebook.com/apps/
2. Select your app
3. Settings ? Basic
4. Verify:
   ```
   App Domains: localhost
   ```
5. Facebook Login ? Settings
6. Verify:
   ```
   Valid OAuth Redirect URIs:
   https://localhost:5001/signin-facebook
   https://localhost:7001/signin-facebook
   http://localhost:5000/signin-facebook
   ```
7. Top right: Should say "Live" (not "Development")

---

## ?? **COMMON ERRORS & FIXES:**

### **Google Error: "redirect_uri_mismatch"**

**Cause:** Redirect URI not added or wrong

**Fix:** 
- Add EXACT URI: `https://localhost:5001/signin-google`
- NO trailing slash
- Match https/http exactly

### **Facebook Error: "Can't load URL"**

**Cause:** Domain not in App Domains

**Fix:**
- Settings ? Basic ? App Domains: Add `localhost`
- Make app "Live" (toggle switch)

### **Facebook Error: "URL Blocked"**

**Cause:** Redirect URI not registered

**Fix:**
- Facebook Login ? Settings
- Add: `https://localhost:5001/signin-facebook`

### **Both Not Working:**

**Check:**
1. App is running on correct port (5001)
2. Credentials in appsettings.json are correct
3. Restart app after configuration changes
4. Clear browser cookies
5. Try incognito/private browsing

---

## ? **CHECKLIST:**

### **Google:**
- [ ] Go to Google Cloud Console ? Credentials
- [ ] Remove old redirect URI (44387)
- [ ] Add: `https://localhost:5001/signin-google`
- [ ] Add: `https://localhost:7001/signin-google`
- [ ] Add: `http://localhost:5000/signin-google`
- [ ] Save changes
- [ ] Wait 2 minutes

### **Facebook:**
- [ ] Go to Facebook Developers ? Your App
- [ ] Settings ? Basic
- [ ] App Domains: Add `localhost`
- [ ] Save
- [ ] Facebook Login ? Settings
- [ ] Valid OAuth Redirect URIs: Add all 3 URIs
- [ ] Save
- [ ] Switch app to "Live" mode
- [ ] Wait 2 minutes

### **Test:**
- [ ] Restart app: `dotnet run`
- [ ] Open: https://localhost:5001/Identity/Account/Login
- [ ] Click "Continue with Google" ? Should redirect
- [ ] Click "Continue with Facebook" ? Should redirect
- [ ] Both should create account and sign in

---

## ?? **AFTER FIXING:**

Both buttons should:
1. ? Redirect to Google/Facebook login
2. ? Allow you to sign in
3. ? Redirect back to your app
4. ? Auto-create account (first time)
5. ? Sign you in
6. ? Show success message

---

## ?? **QUICK COMMANDS:**

```powershell
# Stop app
Ctrl+C

# Restart app
dotnet run

# Your app will be at:
# https://localhost:5001
```

---

**Fix both providers using the steps above and test!** ?

The key issues are:
1. **Google:** Wrong redirect URIs (44387 instead of 5001)
2. **Facebook:** Missing domain + app not live

Fix these and both will work! ??
