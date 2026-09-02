# Keycloak — Realm `flash-sales-dev` Setup

> Full recreation guide via the admin console. Keycloak version: **26.6.1**

---

## 1. Docker Compose

```yaml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:26.6.1
    command: start-dev
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin_pass
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://keycloak-postgres:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: keycloak_pass
    ports:
      - "8080:8080"
    depends_on:
      keycloak-postgres:
        condition: service_healthy

  keycloak-postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: keycloak_pass
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U keycloak"]
      interval: 5s
      retries: 10
```

Go to `http://localhost:8080` and log in with `admin / admin_pass`.

---

## 2. Create the Realm

1. Top-left menu → **Create Realm**
2. **Realm name:** `flash-sales-dev`
3. **Enabled:** ON
4. **Create**

---

## 3. Realm Settings

Go to **Realm Settings** and configure the following tabs:

### Login tab

| Setting | Value |
|---|---|
| User registration | OFF |
| Login with email | ON |
| Duplicate emails | OFF |
| Remember me | ON |
| Verify email | OFF |
| Password reset | ON |
| Edit username | OFF |

### Sessions tab

| Setting | Value |
|---|---|
| SSO Session Idle | 30 minutes |
| SSO Session Max | 10 hours |
| Access Token Lifespan | 5 minutes |

---

## 4. Realm Roles

Go to **Realm roles → Create role** and create the three roles below.

### `activated`
- **Role name:** `activated`
- Assigned by the API after the user completes the activation flow (`POST /api/v1/users` or `POST /api/v1/users/customer/activate`).

### `customer`
- **Role name:** `customer`
- Not assigned in Keycloak — used only by the `flash-sales-public` mappers in §6.

### `seller`
- **Role name:** `seller`
- Not assigned in Keycloak — used only by the `flash-sales-public` mappers in §6.

---

## 5. Client Scopes

Go to **Client scopes → Create client scope** and create each of the scopes below. All of them: Protocol `OpenID Connect`, Display on consent screen `OFF`, Include in token scope `ON`.

### Service-to-service

| Name | Type | Description |
|---|---|---|
| `launches.stock.write` | Optional | Reserve and release launch stock on behalf of the order-creation saga |
| `users.permissions.read` | Optional | Look up any identity's permissions via the Users gRPC service |

Create any further service-to-service scope the same way, named `<service>.<capability>`, and register it as **Optional** on the calling service's client (§8).

### Per-module read/write

| Name | Type | Description |
|---|---|---|
| `catalog.read` | Default | Read access to the Catalog API |
| `catalog.write` | Default | Write access to the Catalog API |
| `launches.read` | Default | Read access to the Launches API |
| `launches.write` | Default | Write access to the Launches API |
| `orders.read` | Default | Read access to the Orders API |
| `orders.write` | Default | Write access to the Orders API |
| `users.read` | Default | Read access to the Users API |
| `users.write` | Default | Write access to the Users API |
| `payments.write` | Default | Write access to the Payments API (checkout) |

These are assigned to `flash-sales-public` in §6, not created there — the client scope itself has no "Type" until it's attached to a client.

---

## 6. Client: `flash-sales-public` (Frontend)

Go to **Clients → Create client**.

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-public` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | OFF (public client) |
| Standard flow | ON |
| Direct access grants | ON |
| Implicit flow | OFF |
| Service accounts | OFF |

### Login Settings
| Field | Value |
|---|---|
| Valid redirect URIs | `http://localhost:3000/*` |
| Valid post logout URIs | `http://localhost:3000/*` |
| Web origins | `http://localhost:3000` |

### Client Scopes

Go to **Clients → flash-sales-public → Client scopes → Add client scope**, select all nine per-module scopes from §5 (`catalog.read`, `catalog.write`, `launches.read`, `launches.write`, `orders.read`, `orders.write`, `users.read`, `users.write`, `payments.write`), and add them as **Default** — not Optional. Every token issued to the SPA needs to carry these automatically, without the frontend having to request them explicitly.

### Protocol Mappers

Go to **Clients → flash-sales-public → Client scopes → flash-sales-public-dedicated → Add mapper → By configuration**.

#### Mapper: `birth_date`
| Field | Value |
|---|---|
| Mapper type | User Attribute |
| Name | `birth_date` |
| User Attribute | `birth_date` |
| Token Claim Name | `birth_date` |
| Claim JSON Type | String |
| Add to ID token / access token / userinfo | ON |

#### Mapper: `activated`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `activated` |
| Token Claim Name | `activated` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token / access token / userinfo | ON |

#### Mapper: `customer`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `customer` |
| Token Claim Name | `customer` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token / access token / userinfo | ON |

#### Mapper: `seller`
| Field | Value |
|---|---|
| Mapper type | User Realm Role |
| Name | `seller` |
| Token Claim Name | `seller` |
| Claim JSON Type | String |
| Multivalued | ON |
| Add to ID token / access token / userinfo | ON |

#### Audience mappers

Create one per resource-server client from §7 (create those clients first — the dropdown below only lists clients that already exist):

| Field | Value (repeat per service) |
|---|---|
| Mapper type | Audience |
| Name | `audience-catalog`, `audience-launches`, `audience-orders`, `audience-payments`, `audience-users` |
| Included Client Audience | `flash-sales-catalog`, `flash-sales-launches`, `flash-sales-orders`, `flash-sales-payments`, `flash-sales-users` (matching the name) |
| Add to ID token | OFF |
| Add to access token | ON |

---

## 7. Clients: Resource Servers

Create one client per service: `flash-sales-catalog`, `flash-sales-launches`, `flash-sales-orders`, `flash-sales-payments`, `flash-sales-users`.

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-catalog` *(repeat for the other four)* |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | OFF (public) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Implicit flow | OFF |
| Service accounts roles | OFF |

No redirect URIs, mappers, or credentials needed on these clients.

---

## 8. Client: `flash-sales-orders-svc` (Orders service account)

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-orders-svc` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### Client Scopes

Go to **Clients → flash-sales-orders-svc → Client scopes → Add client scope**, select `launches.stock.write` and `users.permissions.read` (§5), and add both as **Optional**.

### Protocol Mappers

Go to **Clients → flash-sales-orders-svc → Client scopes → flash-sales-orders-svc-dedicated → Add mapper → By configuration**, and create both mappers below:

| Field | Value (repeat per audience) |
|---|---|
| Mapper type | Audience |
| Name | `audience-launches`, `audience-users` |
| Included Client Audience | `flash-sales-launches`, `flash-sales-users` (matching the name) |
| Add to ID token | OFF |
| Add to access token | ON |

Both are required, not optional — without them the token this client gets from `client_credentials` won't carry the target service as audience, and that service will reject it at authentication.

### Credentials

Go to **Clients → flash-sales-orders-svc → Credentials**, copy the **Client secret**, and set it in the Orders service's configuration:

```
ClientCredentials__Authority=http://localhost:8080/realms/flash-sales-dev
ClientCredentials__ClientId=flash-sales-orders-svc
ClientCredentials__ClientSecret=<secret>
```

---

## 9. Client: `flash-sales-catalog-svc` (Catalog service account)

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-catalog-svc` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### Client Scopes

Go to **Clients → flash-sales-catalog-svc → Client scopes → Add client scope**, select `users.permissions.read` (§5), and add it as **Optional**.

### Protocol Mappers

Go to **Clients → flash-sales-catalog-svc → Client scopes → flash-sales-catalog-svc-dedicated → Add mapper → By configuration**.

| Field | Value |
|---|---|
| Mapper type | Audience |
| Name | `audience-users` |
| Included Client Audience | `flash-sales-users` |
| Add to ID token | OFF |
| Add to access token | ON |

This is required, not optional — without it the token this client gets from `client_credentials` won't carry `flash-sales-users` as audience, and the Users gRPC service will reject it at authentication.

### Credentials

Go to **Clients → flash-sales-catalog-svc → Credentials**, copy the **Client secret**, and set it in the Catalog service's configuration:

```
ClientCredentials__Authority=http://localhost:8080/realms/flash-sales-dev
ClientCredentials__ClientId=flash-sales-catalog-svc
ClientCredentials__ClientSecret=<secret>
```

---

## 10. Client: `flash-sales-launches-svc` (Launches service account)

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-launches-svc` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### Client Scopes

Go to **Clients → flash-sales-launches-svc → Client scopes → Add client scope**, select `users.permissions.read` (§5), and add it as **Optional**.

### Protocol Mappers

Go to **Clients → flash-sales-launches-svc → Client scopes → flash-sales-launches-svc-dedicated → Add mapper → By configuration**.

| Field | Value |
|---|---|
| Mapper type | Audience |
| Name | `audience-users` |
| Included Client Audience | `flash-sales-users` |
| Add to ID token | OFF |
| Add to access token | ON |

This is required, not optional — without it the token this client gets from `client_credentials` won't carry `flash-sales-users` as audience, and the Users gRPC service will reject it at authentication.

### Credentials

Go to **Clients → flash-sales-launches-svc → Credentials**, copy the **Client secret**, and set it in the Launches service's configuration:

```
ClientCredentials__Authority=http://localhost:8080/realms/flash-sales-dev
ClientCredentials__ClientId=flash-sales-launches-svc
ClientCredentials__ClientSecret=<secret>
```

---

## 11. Client: `flash-sales-payments-svc` (Payments service account)

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-payments-svc` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### Client Scopes

Go to **Clients → flash-sales-payments-svc → Client scopes → Add client scope**, select `users.permissions.read` (§5), and add it as **Optional**.

### Protocol Mappers

Go to **Clients → flash-sales-payments-svc → Client scopes → flash-sales-payments-svc-dedicated → Add mapper → By configuration**.

| Field | Value |
|---|---|
| Mapper type | Audience |
| Name | `audience-users` |
| Included Client Audience | `flash-sales-users` |
| Add to ID token | OFF |
| Add to access token | ON |

This is required, not optional — without it the token this client gets from `client_credentials` won't carry `flash-sales-users` as audience, and the Users gRPC service will reject it at authentication.

### Credentials

Go to **Clients → flash-sales-payments-svc → Credentials**, copy the **Client secret**, and set it in the Payments service's configuration:

```
ClientCredentials__Authority=http://localhost:8080/realms/flash-sales-dev
ClientCredentials__ClientId=flash-sales-payments-svc
ClientCredentials__ClientSecret=<secret>
```

---

## 12. Client: `flash-sales-users-admin` (Users service — Keycloak admin)

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-users-admin` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | ON (confidential client) |
| Standard flow | OFF |
| Direct access grants | OFF |
| Service accounts roles | ON |

### Service Account Roles

Go to **Clients → flash-sales-users-admin → Service account roles → Assign role → Filter by clients → realm-management** and assign:

- `manage-users`
- `view-users`
- `view-realm` — required to look up a realm role's id (`GET /roles/{name}`) before assigning it to a user via `POST /users/{id}/role-mappings/realm`. Without it, `AssignRoleAsync` (`KeyCloakClient.cs`) gets a 403 on that lookup and role assignment never happens, even though `manage-users` alone is enough for the assignment call itself.

### Credentials

Go to **Clients → flash-sales-users-admin → Credentials**, copy the **Client secret**, and set it in the Users service's configuration:

```
KeyCloak__ConfidentialClientId=flash-sales-users-admin
KeyCloak__ConfidentialClientSecret=<secret>
KeyCloak__BaseUrl=http://localhost:8080/realms/
KeyCloak__AdminUrl=http://localhost:8080/admin/realms/
KeyCloak__CurrentRealm=flash-sales-dev
```

---

## 13. Client: `flash-sales-swagger` (Swagger UI)

Lets you click "Authorize" inside each service's own `/swagger` page and log in interactively (Authorization Code + PKCE). Separate from `flash-sales-public` (§6), which is the SPA's client — Swagger's OAuth2 redirect always lands back on that *service's own* origin, never on the SPA's.

### General Settings
| Field | Value |
|---|---|
| Client type | OpenID Connect |
| Client ID | `flash-sales-swagger` |

### Capability Config
| Field | Value |
|---|---|
| Client authentication | OFF (public client) |
| Standard flow | ON |
| Direct access grants | OFF |
| Implicit flow | OFF |
| Service accounts | OFF |

### Login Settings

One redirect URI per service, both ports (`https` dev-cert port and plain `http` port) since either may be running:

| Field | Value |
|---|---|
| Valid redirect URIs | `https://localhost:7239/swagger/oauth2-redirect.html`, `http://localhost:5062/swagger/oauth2-redirect.html` (Catalog) · `https://localhost:7172/swagger/oauth2-redirect.html`, `http://localhost:5116/swagger/oauth2-redirect.html` (Launches) · `https://localhost:7229/swagger/oauth2-redirect.html`, `http://localhost:5070/swagger/oauth2-redirect.html` (Orders) · `https://localhost:7106/swagger/oauth2-redirect.html`, `http://localhost:5063/swagger/oauth2-redirect.html` (Payments) · `https://localhost:7088/swagger/oauth2-redirect.html`, `http://localhost:5051/swagger/oauth2-redirect.html` (Users) |
| Web origins | the ten origins above, without the path |

### Client Scopes

Go to **Clients → flash-sales-swagger → Client scopes → Add client scope**, select the eight non-Payments per-module scopes from §5 (`catalog.read`, `catalog.write`, `launches.read`, `launches.write`, `orders.read`, `orders.write`, `users.read`, `users.write`), and add them as **Default**. Without these, every `RequireScope(...)`-protected endpoint (nearly all of them — see each module's `*Scopes.cs`) 403s for a token minted through Swagger, even with a valid, authenticated, activated user. Note this client does **not** carry `payments.write` — only `flash-sales-public` does — so a Swagger-issued token still can't call `CheckoutPaymentEndpoint`; add it here too if you need to exercise checkout from Swagger.

### Protocol Mappers

Same audience mappers as `flash-sales-public` (§6) — go to **Clients → flash-sales-swagger → Client scopes → flash-sales-swagger-dedicated → Add mapper → By configuration**, one per resource-server client:

| Field | Value (repeat per service) |
|---|---|
| Mapper type | Audience |
| Name | `audience-catalog`, `audience-launches`, `audience-orders`, `audience-payments`, `audience-users` |
| Included Client Audience | `flash-sales-catalog`, `flash-sales-launches`, `flash-sales-orders`, `flash-sales-payments`, `flash-sales-users` (matching the name) |
| Add to ID token | OFF |
| Add to access token | ON |

All five are required — whichever service's Swagger you're testing against validates the token's `aud` claim against that service's own name, so the same client needs to be able to mint a token that satisfies any of them.

### Configuration

Set the client ID in a `Swagger` section of each service's own configuration — kept separate from `Authentication`, which configures the JWT bearer validation the service performs on incoming requests, not the interactive login button Swagger shows:

```
Swagger__ClientId=flash-sales-swagger
```

---

## 14. Identity Providers

### GitHub

Go to **Identity Providers → Add provider → GitHub**.

| Field | Value |
|---|---|
| Client ID | `<your GitHub App client_id>` |
| Client Secret | `<your GitHub App client_secret>` |
| Trust Email | OFF |
| Sync mode | LEGACY |
| First Login Flow | `first broker login` (default) |

Authorization callback URL to set in the GitHub App:
```
http://localhost:8080/realms/flash-sales-dev/broker/github/endpoint
```

### Google

Go to **Identity Providers → Add provider → Google**.

| Field | Value |
|---|---|
| Client ID | `<your Google OAuth client_id>` |
| Client Secret | `<your Google OAuth client_secret>` |
| Trust Email | OFF |
| Sync mode | LEGACY |
| First Login Flow | `first broker login` (default) |

Authorized redirect URI to set in the Google Cloud Console:
```
http://localhost:8080/realms/flash-sales-dev/broker/google/endpoint
```

---

## 15. First Broker Login Flow (Account Linking)

Verify under **Authentication → first broker login** that the authenticators are configured as follows:

| Authenticator | Requirement |
|---|---|
| Review Profile | REQUIRED |
| Create User If Unique | ALTERNATIVE |
| Handle Existing Account | ALTERNATIVE |
| ↳ Confirm Link Existing Account | REQUIRED (sub-flow) |
| ↳ Verify Existing Account By Email | ALTERNATIVE |
| ↳ Verify Existing Account By Re-authentication | ALTERNATIVE |

---

## 16. Theme

1. Mount `docker/keycloak/themes/flash-sales` into the Keycloak container.
2. Go to **Realm Settings → Themes → Login theme** and select `flash-sales`.

---

## 17. Summary

| Component | Value |
|---|---|
| Realm | `flash-sales-dev` |
| User registration | OFF |
| Duplicate emails | OFF |
| User client | `flash-sales-public` |
| Resource-server clients | `flash-sales-catalog`, `flash-sales-launches`, `flash-sales-orders`, `flash-sales-payments`, `flash-sales-users` |
| Service clients | `flash-sales-orders-svc` (holds `launches.stock.write` + `users.permissions.read`, audience mapped to `flash-sales-launches` and `flash-sales-users`), `flash-sales-catalog-svc` / `flash-sales-launches-svc` / `flash-sales-payments-svc` (each holds `users.permissions.read`, audience mapped to `flash-sales-users`) |
| Admin client | `flash-sales-users-admin` — holds `manage-users`/`view-users`/`view-realm` |
| Swagger client | `flash-sales-swagger` — public, PKCE, one redirect URI per service's own `/swagger/oauth2-redirect.html`, audience-mapped to all five resource-server clients, the eight `catalog.*`/`launches.*`/`orders.*`/`users.*` default scopes (not `payments.write` — that's `flash-sales-public`-only) |
| Role `activated` | Realm role, checked on every request |
| Roles `customer` / `seller` | Used only by `flash-sales-public` mappers |
| Scope `launches.stock.write` | Optional, granted only to `flash-sales-orders-svc` |
| Scope `users.permissions.read` | Optional, granted to `flash-sales-orders-svc`, `flash-sales-catalog-svc`, `flash-sales-launches-svc`, `flash-sales-payments-svc` — every service that needs to resolve a caller's permissions |
| Scopes `catalog.*`/`launches.*`/`orders.*`/`users.*` (`.read`/`.write`) and `payments.write` | Default on `flash-sales-public` — every user token carries them |
| Identity Providers | GitHub + Google with First Broker Login flow |
