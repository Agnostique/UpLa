# upla.com.tr server add-on: app sign-in

The UpLa desktop app lets members sign in with their upla.com.tr username (or email) and password. Chevereto 4.5.7 has no
API for that, so this folder contains a small Chevereto **route override** that adds one. It does not modify any
Chevereto core file.

Guests are not affected: without signing in, the app keeps uploading with the site's public (guest) API key.

## What it adds

| URL | Used by | What it does |
| --- | --- | --- |
| `POST /upla-app/login` | app | Checks username/email + password (and the two-step verification code when the account has one) and returns a new API key for that computer, named `UpLa - <computer>` |
| `POST /upla-app/me` | app | Returns the account of the `X-API-Key` header (shown as "Signed in as …") |
| `POST /upla-app/logout` | app | Deletes the key of the `X-API-Key` header (the app's "Sign out") |
| `GET /upla-app/devices` | website | Signed-in members see their computers and API keys, remove them, and create API keys for other programs |

The app only keeps the per-computer key (encrypted with Windows DPAPI); the password is sent only to `/upla-app/login`
and is never stored. The keys are normal Chevereto API keys, so uploads still go through the standard
`POST /api/1/upload`, and a key can only upload.

All app endpoints require `POST` and the header `X-Upla-App: 1`, which browsers cannot send from other websites. A
sign-in with two-step verification takes two requests: the first one answers `two_factor_required`, the app then asks
for the code and sends both again.

## Install

1. Copy `chevereto/app/legacy/routes/overrides/upla-app.php` to the same path in the Chevereto installation
   (`<chevereto>/app/legacy/routes/overrides/upla-app.php`). Create the `overrides` folder if it does not exist.
   On Docker installs, mount or bake the file into the image, otherwise a rebuild removes it.
2. Dashboard → Settings → API: **user API keys must be enabled** (`enable_api_user`).
3. Make sure Chevereto sees the visitor's real IP (see [Real visitor IP](#real-visitor-ip)). Otherwise every visitor
   shares the same attempt limits, and one person's typos can block everyone's sign-in.
4. Check it from any computer. Expect `{"status_code":405,...}` for the GET and `invalid_credentials` for the wrong
   password:

   ```sh
   curl -i https://upla.com.tr/upla-app/login
   curl -i -X POST -H "X-Upla-App: 1" --data "login-subject=nobody&password=wrong" https://upla.com.tr/upla-app/login
   ```

   In Windows PowerShell, type `curl.exe` instead of `curl` (there `curl` is another command). The wrong password
   counts as a failed login for your IP (see [Limits](#limits)).
5. Add a link so members can find their devices: Dashboard → Pages → add a page of type **Link** with the URL
   `https://upla.com.tr/upla-app/devices` (for example "Connected devices" / "Bağlı cihazlar"). Visitors who are not
   logged in are sent to the login page first.

Optional: a Cloudflare rate limiting rule for `POST /upla-app/login` as an extra layer. Do not set it lower than about
5 requests per minute per IP, because a sign-in with two-step verification takes two requests and a typo takes more.

## Real visitor IP

Behind Cloudflare (or any reverse proxy), PHP sees the proxy's address unless the real IP is restored. This matters for
these limits, but also for Chevereto's own login lockout and upload flood protection. Use one of these:

- **Recommended: restore the IP in the web server**, trusting the header only from Cloudflare's addresses
  (<https://www.cloudflare.com/ips/>):
  - Apache: `RemoteIPHeader CF-Connecting-IP` plus one `RemoteIPTrustedProxy <range>` line per Cloudflare range
    (`mod_remoteip`).
  - nginx: `real_ip_header CF-Connecting-IP;` plus one `set_real_ip_from <range>;` line per Cloudflare range.

  Keep the list up to date when Cloudflare adds ranges.
- **Only if the origin server accepts connections from Cloudflare alone** (firewall allowlist or Cloudflare Tunnel):
  set Chevereto's environment variable `CHEVERETO_HEADER_CLIENT_IP` to `CF-Connecting-IP`, in `app/env.php`
  (`'CHEVERETO_HEADER_CLIENT_IP' => 'CF-Connecting-IP',`) or in the Docker container's environment. If the origin can
  be reached directly, anyone can send that header with any IP and get unlimited attempts, so do not use this option
  then.

To check it, send one wrong sign-in (step 4) and look at the newest row of the `requests` table (`chv_requests` with
the default table prefix): `request_ip` must be your own public IP (shown as `ip=` on
<https://www.cloudflare.com/cdn-cgi/trace>), not a Cloudflare or proxy address.

## Limits

The app cannot show the website's CAPTCHA, so sign-in attempts are limited:

- per IP: 5 failed attempts per hour and 10 per day,
- per account: 10 failed attempts per day (from any IP),
- wrong passwords and wrong two-step codes both count, from the app and from the website alike: failures are recorded
  as Chevereto login failures, so website and app share these limits,
- a successful sign-in clears that member's own failed attempts from that IP, like a website login does,
- each attempt is counted before the password is checked, so parallel requests do not get extra guesses (in tests,
  40 simultaneous requests got exactly 5 checked),
- the failures also count toward Chevereto's own lockout: more than 25 failed logins, sign-ups or two-step codes from
  one IP in a day blocks that IP on the whole site.

When a limit is hit the app gets HTTP 429 with `retry_after` (3600 or 86400 seconds, also sent as the `Retry-After`
header) and tells the user to wait or to sign in on the website.

## Keys and devices

- Signing in again on the same computer replaces that computer's previous key; at most 10 app keys are kept per account
  (the oldest app keys are removed).
- Keys never expire, and **changing the password does not remove them**. A lost or sold computer must be removed on
  `/upla-app/devices`; the page says so too.
- `/upla-app/devices` can create API keys for other programs (e.g. ShareX), at most 5 per account. A new key is shown
  only once.
- Chevereto's own *Settings → API* page shows only the newest key of an account, which after an app sign-in is an app
  key, and its delete/regenerate button removes that key: that computer then has to sign in again. Members should
  manage keys on `/upla-app/devices`.
- Banning or deleting a user does not delete their API keys in Chevereto 4.5.7, and `/api/1/upload` does not check the
  account status, so a banned member's keys keep uploading. The member cannot use `/upla-app/devices` any more, so
  remove the keys in the database (default table prefix `chv_`):

  ```sql
  SELECT user_id FROM chv_users WHERE user_username = 'name';
  DELETE FROM chv_api_keys WHERE api_key_user_id = <user_id>;
  ```

## Errors

Unexpected server errors answer `{"status_code":500,"error":{"code":"server_error",...}}`. They are written to PHP's
error log with the exception and file/line only: the request data (which contains the password) is never logged.
The app shows that the sign-in failed and the user can try again.

## After Chevereto updates

The route uses Chevereto's internal classes (`ApiKey`, `Login`, `TwoFactor`, `RequestLog`, `User`, `DB`), which have no
stability promise. After every Chevereto update, run the checks in step 4 and sign in from the app once.

It was tested with Chevereto 4.5.7 (free edition) on PHP 8.3 and MariaDB 10.11. Check it once on the Pro edition too.

## Security notes for the site owner

- In Chevereto 4.5.7 the 30-day `KEEP_LOGIN` cookie is created before the two-step verification code is entered, and the
  pending verification is only remembered in the PHP session. Check whether this applies to your Pro version.
- Chevereto keys are shown only once and stored hashed (Argon2id); the app never sees a key other than its own.
