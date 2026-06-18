# System Architecture — Currency Exchange Office

## Overview

The system is a three-tier network application simulating an online currency exchange office. It follows a client-server architecture where a WCF Web Service acts as the central business logic layer, a WPF application provides the user interface, and a SQL Server database handles data persistence.

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT TIER                              │
│                                                                 │
│   ┌─────────────────────────────────┐                          │
│   │       WPF Client Application    │                          │
│   │  - Check exchange rates         │                          │
│   │  - Buy / sell currencies        │                          │
│   │  - View transaction history     │                          │
│   └────────────┬────────────────────┘                          │
└────────────────│────────────────────────────────────────────────┘
                 │ BasicHttpBinding (HTTP/SOAP)
                 │ http://localhost:8080/ExchangeService
┌────────────────│────────────────────────────────────────────────┐
│                │       SERVICE TIER                              │
│   ┌────────────▼────────────────────┐                          │
│   │       WCF Service Host          │                          │
│   │  ExchangeService.cs             │                          │
│   │  - GetRate / GetAllRates        │                          │
│   │  - BuyCurrency / SellCurrency   │                          │
│   │  - GetHistoricalRates           │                          │
│   └────────────┬────────────────────┘                          │
│                │ HTTP GET                                        │
│   ┌────────────▼────────────────────┐                          │
│   │       NbpApiClient.cs           │                          │
│   │  Fetches live rates from        │                          │
│   │  api.nbp.pl (Table A)           │                          │
│   └─────────────────────────────────┘                          │
└────────────────────────────────────────────────────────────────┘
                 │ ADO.NET / SQL
┌────────────────│────────────────────────────────────────────────┐
│                │       DATA TIER                                 │
│   ┌────────────▼────────────────────┐                          │
│   │    SQL Server / LocalDB         │                          │
│   │  - Users                        │                          │
│   │  - Accounts (balances)          │                          │
│   │  - Transactions                 │                          │
│   │  - ExchangeRateCache            │                          │
│   └─────────────────────────────────┘                          │
└────────────────────────────────────────────────────────────────┘
```

---

## Components

### WCF Service (`WCF-Service/`)

A self-hosted WCF service running as a console application. Exposes operations via `BasicHttpBinding` — a standard HTTP/SOAP binding compatible with any .NET client.

**Key files:**
- `IExchangeService.cs` — service contract (interface + data contracts)
- `ExchangeService.cs` — service implementation
- `NbpApiClient.cs` — HTTP client wrapping the NBP API
- `App.config` — endpoint and binding configuration
- `Program.cs` — `ServiceHost` setup

**Binding choice:** `BasicHttpBinding` was chosen for maximum interoperability. It requires no client-side certificates or WS-Security configuration, satisfying the no-authorization requirement.

### NBP API Integration

The National Bank of Poland publishes daily exchange rates at `api.nbp.pl`. The service uses **Table A** which contains mid rates for ~35 currencies.

Endpoints used:
- `GET /api/exchangerates/tables/A/?format=json` — all currencies
- `GET /api/exchangerates/rates/A/{code}/?format=json` — single currency
- `GET /api/exchangerates/rates/A/{code}/{start}/{end}/?format=json` — historical

A **2% spread** is applied symmetrically to derive buy/sell rates from the NBP mid rate.

### Client Application (`Client-Application/`)

A WPF desktop application that adds a Service Reference to the WCF service and consumes it via the generated proxy client. Provides a GUI for all service operations.

### Database (`Database/`)

SQL Server schema with four tables:
- `Users` — account credentials
- `Accounts` — per-user per-currency balances
- `Transactions` — full audit log of every exchange
- `ExchangeRateCache` — optional local cache of NBP rates

---

## Communication Protocol

- **Transport:** HTTP
- **Encoding:** SOAP/XML (BasicHttpBinding default)
- **Security:** None (as required by the lab specification)
- **Metadata:** WSDL exposed at `/ExchangeService?wsdl` for automatic proxy generation

---

## Authors

Frank Mafuta, Tanatswa Mandudzo, Wirimai Frank Mafuta  
Network Application Development — Lab Project
