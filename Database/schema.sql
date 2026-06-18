-- ============================================================
-- Currency Exchange Office — Database Schema
-- Compatible with: SQL Server / SQL Server Express / LocalDB
-- ============================================================

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CurrencyExchangeDB')
BEGIN
    CREATE DATABASE CurrencyExchangeDB;
    PRINT 'Database CurrencyExchangeDB created.';
END
GO

USE CurrencyExchangeDB;
GO

-- ── USERS ──────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId      INT IDENTITY(1,1) PRIMARY KEY,
        Username    NVARCHAR(50)  NOT NULL UNIQUE,
        Email       NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,       -- store hashed, never plaintext
        CreatedAt   DATETIME2     NOT NULL DEFAULT GETDATE(),
        IsActive    BIT           NOT NULL DEFAULT 1
    );
    PRINT 'Table Users created.';
END
GO

-- ── CURRENCY ACCOUNTS ──────────────────────────────────────────────────────
-- Each user can hold balances in multiple currencies

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts')
BEGIN
    CREATE TABLE Accounts (
        AccountId    INT IDENTITY(1,1) PRIMARY KEY,
        UserId       INT            NOT NULL REFERENCES Users(UserId),
        CurrencyCode NCHAR(3)       NOT NULL,       -- ISO 4217: PLN, USD, EUR ...
        Balance      DECIMAL(18, 4) NOT NULL DEFAULT 0,
        UpdatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_UserCurrency UNIQUE (UserId, CurrencyCode),
        CONSTRAINT CHK_Balance CHECK (Balance >= 0)
    );
    PRINT 'Table Accounts created.';
END
GO

-- ── TRANSACTIONS ────────────────────────────────────────────────────────────
-- Records every buy/sell exchange operation

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        TransactionId   INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT             NOT NULL REFERENCES Users(UserId),
        TransactionType NVARCHAR(10)    NOT NULL,       -- 'BUY' or 'SELL'
        FromCurrency    NCHAR(3)        NOT NULL,
        ToCurrency      NCHAR(3)        NOT NULL,
        AmountSent      DECIMAL(18, 4)  NOT NULL,
        AmountReceived  DECIMAL(18, 4)  NOT NULL,
        RateUsed        DECIMAL(18, 6)  NOT NULL,       -- rate at time of transaction
        NbpMidRate      DECIMAL(18, 6)  NULL,           -- NBP mid for reference
        ExecutedAt      DATETIME2       NOT NULL DEFAULT GETDATE(),
        Notes           NVARCHAR(255)   NULL,

        CONSTRAINT CHK_TransactionType CHECK (TransactionType IN ('BUY', 'SELL')),
        CONSTRAINT CHK_AmountSent     CHECK (AmountSent > 0),
        CONSTRAINT CHK_AmountReceived CHECK (AmountReceived > 0)
    );
    PRINT 'Table Transactions created.';
END
GO

-- ── EXCHANGE RATE CACHE ─────────────────────────────────────────────────────
-- Optionally cache NBP rates locally to reduce API calls

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExchangeRateCache')
BEGIN
    CREATE TABLE ExchangeRateCache (
        CacheId         INT IDENTITY(1,1) PRIMARY KEY,
        CurrencyCode    NCHAR(3)       NOT NULL,
        CurrencyName    NVARCHAR(100)  NOT NULL,
        MidRate         DECIMAL(18, 6) NOT NULL,
        BuyRate         DECIMAL(18, 6) NOT NULL,
        SellRate        DECIMAL(18, 6) NOT NULL,
        EffectiveDate   DATE           NOT NULL,
        CachedAt        DATETIME2      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_RateCache UNIQUE (CurrencyCode, EffectiveDate)
    );
    PRINT 'Table ExchangeRateCache created.';
END
GO

-- ── INDEXES ─────────────────────────────────────────────────────────────────

CREATE INDEX IF NOT EXISTS IX_Transactions_UserId
    ON Transactions(UserId);

CREATE INDEX IF NOT EXISTS IX_Transactions_ExecutedAt
    ON Transactions(ExecutedAt DESC);

CREATE INDEX IF NOT EXISTS IX_Accounts_UserId
    ON Accounts(UserId);
GO

-- ── SEED DATA ───────────────────────────────────────────────────────────────

-- Demo user (password hash is SHA-256 of "Password123!" — replace with real hashing)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'demo')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash)
    VALUES ('demo', 'demo@exchange.local',
            'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f');

    -- Give demo user a PLN balance to start with
    DECLARE @DemoUserId INT = SCOPE_IDENTITY();

    INSERT INTO Accounts (UserId, CurrencyCode, Balance)
    VALUES
        (@DemoUserId, 'PLN', 10000.00),
        (@DemoUserId, 'USD', 0.00),
        (@DemoUserId, 'EUR', 0.00);

    PRINT 'Demo user and accounts seeded.';
END
GO

PRINT '========================================';
PRINT 'CurrencyExchangeDB schema ready.';
PRINT '========================================';
