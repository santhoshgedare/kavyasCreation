# ?? Quick OAuth Credentials Setup

## Google OAuth Setup (5 minutes)

### Step-by-Step:

1. **Visit:** https://console.cloud.google.com/

2. **Create Project:**
   - Click "NEW PROJECT"
   - Name: `Kavyas Creations`
   - CREATE

3. **Enable API:**
   - APIs & Services ? Library
   - Search: `Google+ API`
   - ENABLE

4. **OAuth Consent:**
   - APIs & Services ? OAuth consent screen
   - External ? CREATE
   - App name: `Kavya's Creations`
   - Email: `your@email.com`
   - SAVE AND CONTINUE (3 times)

5. **Create Credentials:**
   - APIs & Services ? Credentials
   - CREATE CREDENTIALS ? OAuth client ID
   - Web application
   - Name: `Kavyas Creations Web`
   - Authorized redirect URIs:
     ```
     https://localhost:5001/signin-google
     ```
   - CREATE

6. **Copy Credentials:**
   ```
   Client ID: XXXXXXXX.apps.googleusercontent.com
   Client Secret: GOCSPX-XXXXXXXXXXXXXXXX
   ```

---

## Facebook OAuth Setup (5 minutes)

### Step-by-Step:

1. **Visit:** https://developers.facebook.com/

2. **Create App:**
   - My Apps ? Create App
   - Other ? Next
   - Consumer ? Next
   - Name: `Kavya's Creations`
   - Email: `your@email.com`
   - Create app

3. **Add Facebook Login:**
   - Dashboard ? Add Product
   - Facebook Login ? Set Up
   - Web
   - Site URL: `https://localhost:5001`
   - Save ? Continue

4. **Configure Settings:**
   - Facebook Login ? Settings
   - Valid OAuth Redirect URIs:
     ```
     https://localhost:5001/signin-facebook
     ```
   - Save Changes

5. **Get Credentials:**
   - Settings ? Basic
   - Copy:
     ```
     App ID: XXXXXXXXXXXXXXXX
     App Secret: XXXXXXXXXXXXXXXX (Show)
     ```

6. **Go Live:**
   - Top right: Switch to "Live" mode

---

## Add Credentials to Your App

### Option 1: appsettings.json (Quick)

Edit `kavyasCreation/appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-YOUR_GOOGLE_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_SECRET"
    }
  }
}
```

### Option 2: User Secrets (Secure - Recommended)

```powershell
cd C:\Users\SG\source\repos\kavyasCreation\kavyasCreation

dotnet user-secrets set "Authentication:Google:ClientId" "PASTE_YOUR_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "PASTE_YOUR_CLIENT_SECRET"
dotnet user-secrets set "Authentication:Facebook:AppId" "PASTE_YOUR_APP_ID"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "PASTE_YOUR_APP_SECRET"
```

---

## Test It!

```powershell
dotnet run
```

Navigate to: `https://localhost:5001/Identity/Account/Login`

You should see:
- [?? Continue with Google]
- [?? Continue with Facebook]

Click either button and sign in!

---

## Production URLs

When deploying to production, add these redirect URIs:

**Google:**
```
https://yourdomain.com/signin-google
```

**Facebook:**
```
https://yourdomain.com/signin-facebook
```

And update your appsettings or environment variables with production credentials.
