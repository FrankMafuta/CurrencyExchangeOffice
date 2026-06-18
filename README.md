# Currency Exchange Office — Network Application Development

A network-based currency exchange system built using **WCF (SOAP service)** and **WPF client application**, consuming live exchange rates from the **National Bank of Poland API (NBP)**.

---

## Project Structure

```
CurrencyExchangeOffice/
├── WCF-Service/          ← Self-hosted WCF SOAP service (business logic + NBP API)
├── Client-Application/   ← WPF desktop client consuming WCF service
├── Database/             ← (Optional / not fully integrated if unused)
├── Documentation/        ← Architecture description and report
└── README.md
```

---

## Technologies

- **WCF (Windows Communication Foundation)** — SOAP service layer
- **WPF (.NET Framework 4.8)** — desktop client UI
- **NBP API (api.nbp.pl)** — live exchange rate source
- **BasicHttpBinding** — SOAP over HTTP
- **SoapUI** — service testing tool

---

## How to Run

### 1. Start the WCF Service

Open the solution in **Visual Studio** and run:

```
WCF-Service → Start (F5)
```

Service runs at:

```
http://localhost:8080/ExchangeService
```

WSDL:

```
http://localhost:8080/ExchangeService?wsdl
```

MEX:

```
http://localhost:8080/ExchangeService/mex
```

> If access denied:
```
netsh http add urlacl url=http://localhost:8080/ExchangeService/ user=Everyone
```

---

### 2. Start the Client Application

Run WPF project in Visual Studio.

It connects to:

```
http://localhost:8080/ExchangeService
```

---

### 3. Test with SoapUI

- Create SOAP Project
- Import WSDL:
```
http://localhost:8080/ExchangeService?wsdl
```

Test operations:
- Ping
- GetRate
- GetAllRates
- GetHistoricalRates
- BuyCurrency
- SellCurrency

---

## WCF Service Operations

| Method | Description |
|------|-------------|
| Ping() | Health check |
| GetRate(code) | Current exchange rate |
| GetRates(codes) | Multiple currencies |
| GetAllRates() | All NBP currencies |
| GetHistoricalRates(code, start, end) | Historical rates |
| BuyCurrency(code, amountPln) | PLN → FX |
| SellCurrency(code, amountForeign) | FX → PLN |

---

## Exchange Rate Logic

Rates are fetched from:

```
https://api.nbp.pl/api/exchangerates/rates/A/{code}
```

Spread applied:

```
BuyRate  = MidRate × 0.98
SellRate = MidRate × 1.02
```

---

## Architecture

```
WPF Client
    ↓ SOAP
WCF Service
    ↓
NBP API
```

---

## Author

- Wirimai Frank Mafuta
