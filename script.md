# Script 

- script OutboxMessages,InboxMessages for sql sql server
```sql
-- B?ng ch?a Outbox Messages
CREATE TABLE OutboxMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Type NVARCHAR(500) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    OccurredOnUtc DATETIME2 NOT NULL,
    ProcessedOnUtc DATETIME2 NULL,
    Error NVARCHAR(MAX) NULL,
    RetryCount INT NOT NULL DEFAULT 0
);
CREATE INDEX IX_OutboxMessages_Processed_Occurred 
ON OutboxMessages(ProcessedOnUtc, OccurredOnUtc) 
INCLUDE (RetryCount);

-- B?ng ch?a Inbox Messages (T?n d?ng Unique Index ch?ng trùng)
CREATE TABLE InboxMessages (
    Id UNIQUEIDENTIFIER NOT NULL,
    HandlerName NVARCHAR(250) NOT NULL,
    ProcessedOnUtc DATETIME2 NOT NULL,
    CONSTRAINT PK_InboxMessages PRIMARY KEY CLUSTERED (Id, HandlerName)
);
```

- script OutboxMessages,InboxMessages for postgreSql
```sql
-- B?ng ch?a Outbox Messages
CREATE TABLE "OutboxMessages" (
    "Id" UUID PRIMARY KEY,
    "Type" VARCHAR(500) NOT NULL,
    "Content" TEXT NOT NULL,
    "OccurredOnUtc" TIMESTAMP NOT NULL,
    "ProcessedOnUtc" TIMESTAMP NULL,
    "Error" TEXT NULL,
    "RetryCount" INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX "IX_OutboxMessages_Processed_Occurred"
ON "OutboxMessages" ("ProcessedOnUtc", "OccurredOnUtc")
INCLUDE ("RetryCount");


-- B?ng ch?a Inbox Messages
-- Composite Primary Key dùng d? ch?ng x? lý trùng
CREATE TABLE "InboxMessages" (
    "Id" UUID NOT NULL,
    "HandlerName" VARCHAR(250) NOT NULL,
    "ProcessedOnUtc" TIMESTAMP NOT NULL,

    CONSTRAINT "PK_InboxMessages"
        PRIMARY KEY ("Id", "HandlerName")
);
```





