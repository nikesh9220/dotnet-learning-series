# Day 10: Where should an Angular SPA store the token?

Most SPAs I review keep the JWT in `localStorage` and attach it with an `HttpInterceptor`. Any script that runs on the page can read `localStorage`, so one XSS bug or one bad npm package is enough to copy the token and use it from somewhere else until it expires.

This demo shows the alternative I prefer: the server keeps the session in an **HttpOnly, Secure, SameSite** cookie, and state-changing calls are protected with an **antiforgery token** in the shape Angular's `HttpClient` already understands (`XSRF-TOKEN` cookie in, `X-XSRF-TOKEN` header out).

Domain: Customers / Orders.

## What it does

`Program.cs` starts a small minimal API on `http://127.0.0.1:5057`, then a client plays the role of the browser:

1. Logs in. The response has no token in the body, only a cookie with `HttpOnly; Secure; SameSite=Strict`.
2. Calls `GET /api/orders` without the cookie (401).
3. Calls it with the cookie (200).
4. Calls `POST /api/orders` without the XSRF header (400).
5. Fetches `/auth/xsrf`, then posts again with `X-XSRF-TOKEN` (201).

## Run it

Needs the .NET 10 SDK.

```bash
dotnet run
```

Expected output:

```
1. Login                         -> 204
   Set-Cookie flags              -> path=/; secure; samesite=strict; httponly
2. GET orders, no cookie         -> 401
3. GET orders, with cookie       -> 200
4. POST order, no XSRF header    -> 400
5. POST order, with XSRF header  -> 201
```

## Notes

- The demo runs over plain HTTP on localhost so it needs no certificate. The client handles cookies by hand because a normal cookie container will not send `Secure` cookies over HTTP. In a real app, serve everything over HTTPS.
- On the Angular side, `provideHttpClient()` has XSRF support on by default. It sends the header on POST/PUT/PATCH/DELETE to **relative** URLs, so call the API through the same origin (or a proxy) rather than an absolute URL.
- If your API lives on a different site from the SPA, put a small backend-for-frontend on the SPA's origin that holds the tokens and forwards calls.
- HttpOnly does not make XSS harmless. Injected script can still call the API while the tab is open. It just cannot take the token away and reuse it later.
