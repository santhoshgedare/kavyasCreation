# ? QUICK FIX: Google Sign-In Not Working

## ?? **THE #1 FIX (Works 90% of the time):**

### **Add Redirect URI to Google Cloud Console:**

1. **Go to:**
   ```
   https://console.cloud.google.com/apis/credentials
   ```

2. **Click on your OAuth Client ID:**
   - Look for: `1088083900269-...`

3. **Add these URIs to "Authorized redirect URIs":**
   ```
   https://localhost:5001/signin-google
   https://localhost:7001/signin-google
   http://localhost:5000/signin-google
   ```

4. **Click "SAVE"**

5. **Wait 5 minutes** for changes to take effect

6. **Restart your app:**
   ```powershell
   # Stop app (Ctrl+C)
   dotnet run
   ```

7. **Test:**
   - Go to: `https://localhost:5001/Identity/Account/Login`
   - Click "Continue with Google"
   - Should work! ?

---

## ?? **See What's Wrong:**

### **Open Browser Console:**
1. Press `F12`
2. Click "Console" tab
3. Click "Continue with Google"
4. Look for error message

### **Common Errors:**

**"redirect_uri_mismatch"**
? Fix: Add redirect URI (see above)

**"Access blocked"**  
? Fix: Publish app OR add your email to test users in Google Cloud Console

---

## ? **I Already Fixed:**

1. ? Disabled `RequireConfirmedAccount` (was blocking external logins)
2. ? Added detailed logging to ExternalLogin
3. ? Added better error messages

**Just need to add redirect URI to Google Cloud Console!**

---

## ?? **Your Config:**

```json
Google Client ID: 1088083900269-jc9o97nffg83uun7ur75shkkhkrus2lt
Redirect URI needed: https://localhost:5001/signin-google
```

**Add this ? to Google Cloud Console ? Credentials ? Your OAuth Client!**

---

**See `GOOGLE_SSO_TROUBLESHOOTING.md` for detailed troubleshooting!**
