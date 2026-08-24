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

## 5. Client Scope: `launches.stock.write`

Go to **Client scopes → Create client scope**.

| Field | Value |
|---|---|
| Name | `launches.stock.write` |
| Description | Reserve and release launch stock on behalf of the order-creation saga |
| Type | Optional |
| Protocol | OpenID Connect |
| Display on consent screen | OFF |
| Include in token scope | ON |

Create any further service-to-service scope the same way, named `<service>.<capability>`.

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

Go to **Clients → flash-sales-orders-svc → Client scopes → Add client scope**, select `launches.stock.write` (§5), and add it as **Optional**.

### Credentials

Go to **Clients → flash-sales-orders-svc → Credentials**, copy the **Client secret**, and set it in the Orders service's configuration:

```
ClientCredentials__Authority=http://localhost:8080/realms/flash-sales-dev
ClientCredentials__ClientId=flash-sales-orders-svc
ClientCredentials__ClientSecret=<secret>
```

---

## 9. Client: `flash-sales-users-admin` (Users service — Keycloak admin)

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

## 10. Identity Providers

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

## 11. First Broker Login Flow (Account Linking)

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

## 12. Theme

1. Mount `docker/keycloak/themes/flash-sales` into the Keycloak container.
2. Go to **Realm Settings → Themes → Login theme** and select `flash-sales`.

---

## 13. Summary

| Component | Value |
|---|---|
| Realm | `flash-sales-dev` |
| User registration | OFF |
| Duplicate emails | OFF |
| User client | `flash-sales-public` |
| Resource-server clients | `flash-sales-catalog`, `flash-sales-launches`, `flash-sales-orders`, `flash-sales-payments`, `flash-sales-users` |
| Service client | `flash-sales-orders-svc` — holds `launches.stock.write` |
| Admin client | `flash-sales-users-admin` — holds `manage-users`/`view-users` |
| Role `activated` | Realm role, checked on every request |
| Roles `customer` / `seller` | Used only by `flash-sales-public` mappers |
| Scope `launches.stock.write` | Optional, granted only to `flash-sales-orders-svc` |
| Identity Providers | GitHub + Google with First Broker Login flow |
