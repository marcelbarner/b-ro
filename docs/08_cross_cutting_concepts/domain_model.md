# Domain Model

This section describes the core domain concepts and their relationships across the three main bounded contexts of the application.

## Domain Overview

The application is organized around three distinct bounded contexts (domains), each with its own ubiquitous language and domain model:

1. **Finance Domain** - Financial transaction and budget management
2. **Documents Domain** - Document management system (DMS)
3. **Storage Domain** - Inventory and storage location management

## Finance Domain

### Core Entities

#### Transaction
Represents a financial transaction (income or expense).

**Attributes:**
- `TransactionId` (Guid) - Unique identifier
- `Amount` (decimal) - Transaction amount
- `Currency` (string) - Currency code (ISO 4217)
- `Type` (enum) - Income or Expense
- `Date` (DateTimeOffset) - Transaction date and time
- `Description` (string) - Transaction description
- `CategoryId` (Guid) - Reference to Category
- `PaymentMethodId` (Guid) - Reference to PaymentMethod
- `AccountId` (Guid) - Reference to Account
- `Tags` (List<string>) - Searchable tags
- `Attachments` (List<AttachmentReference>) - Related documents
- `IsRecurring` (bool) - Whether transaction is recurring
- `RecurrenceRule` (RecurrenceRule) - Recurrence pattern if applicable
- `CreatedAt` (DateTimeOffset)
- `UpdatedAt` (DateTimeOffset)

#### Category
Hierarchical categorization of transactions.

**Attributes:**
- `CategoryId` (Guid)
- `Name` (string)
- `Type` (enum) - Income or Expense
- `ParentCategoryId` (Guid?) - For hierarchical categories
- `ColorCode` (string) - UI visualization
- `Icon` (string) - Icon identifier

#### Account
Financial account (bank account, cash, credit card).

**Attributes:**
- `AccountId` (Guid)
- `Name` (string)
- `Type` (enum) - Checking, Savings, Cash, CreditCard, Investment
- `Currency` (string)
- `InitialBalance` (decimal)
- `CurrentBalance` (decimal) - Calculated from transactions
- `IsActive` (bool)

#### Budget
Budget definition for spending control.

**Attributes:**
- `BudgetId` (Guid)
- `Name` (string)
- `CategoryId` (Guid) - Associated category
- `Amount` (decimal) - Budget limit
- `Period` (enum) - Daily, Weekly, Monthly, Yearly
- `StartDate` (DateTimeOffset)
- `EndDate` (DateTimeOffset?)
- `Alerts` (List<BudgetAlert>) - Alert thresholds

#### PaymentMethod
Payment instrument used for transactions.

**Attributes:**
- `PaymentMethodId` (Guid)
- `Name` (string) - e.g., "Cash", "Visa Card", "Bank Transfer"
- `Type` (enum) - Cash, Card, BankTransfer, Digital
- `AccountId` (Guid?) - Associated account if applicable

### Value Objects

- `Money` - Amount + Currency
- `RecurrenceRule` - Pattern for recurring transactions
- `BudgetAlert` - Threshold percentage and notification settings

### Domain Services

- `TransactionService` - Business logic for transaction operations
- `BudgetCalculator` - Budget tracking and alert generation
- `RecurrenceEngine` - Generates recurring transactions

## Documents Domain

### Core Entities

#### Document
Represents a managed document in the system.

**Attributes:**
- `DocumentId` (Guid)
- `FileName` (string)
- `DisplayName` (string)
- `FileSize` (long) - Size in bytes
- `MimeType` (string)
- `FileHash` (string) - SHA-256 for deduplication
- `StoragePath` (string) - Physical storage location
- `Version` (int) - Version number
- `VersionHistory` (List<DocumentVersion>)
- `Metadata` (Dictionary<string, string>) - Custom metadata
- `Tags` (List<string>)
- `CategoryId` (Guid?) - Optional categorization
- `OwnerId` (Guid) - User who uploaded
- `AccessLevel` (enum) - Private, Shared, Public
- `CreatedAt` (DateTimeOffset)
- `UpdatedAt` (DateTimeOffset)
- `DeletedAt` (DateTimeOffset?) - Soft delete

#### DocumentVersion
Historical version of a document.

**Attributes:**
- `VersionId` (Guid)
- `DocumentId` (Guid)
- `Version` (int)
- `StoragePath` (string)
- `FileSize` (long)
- `FileHash` (string)
- `UploadedBy` (Guid)
- `UploadedAt` (DateTimeOffset)
- `ChangeNotes` (string)

#### Folder
Hierarchical organization structure.

**Attributes:**
- `FolderId` (Guid)
- `Name` (string)
- `ParentFolderId` (Guid?) - Null for root folders
- `Path` (string) - Full hierarchical path
- `OwnerId` (Guid)
- `CreatedAt` (DateTimeOffset)

#### DocumentShare
Sharing and access control.

**Attributes:**
- `ShareId` (Guid)
- `DocumentId` (Guid)
- `SharedWithUserId` (Guid)
- `Permission` (enum) - Read, Write, Delete
- `ExpiresAt` (DateTimeOffset?)
- `SharedBy` (Guid)
- `SharedAt` (DateTimeOffset)

### Value Objects

- `FileMetadata` - Name, Size, Type, Hash
- `AccessPermission` - Permission level with expiration
- `DocumentPath` - Hierarchical path representation

### Domain Services

- `DocumentStorageService` - Physical file storage and retrieval
- `VersionControlService` - Version management
- `SearchIndexService` - Full-text search indexing
- `OCRService` - Text extraction from images/PDFs
- `ThumbnailGenerator` - Preview generation

## Storage Domain

### Core Entities

#### StorageItem
Represents a physical item in inventory.

**Attributes:**
- `ItemId` (Guid)
- `Name` (string)
- `Description` (string)
- `CategoryId` (Guid)
- `Barcode` (string?) - EAN, UPC, or custom
- `SKU` (string?) - Stock keeping unit
- `Unit` (string) - e.g., "pieces", "kg", "liters"
- `MinimumStock` (int) - Low stock alert threshold
- `PurchaseInfo` (PurchaseInfo?) - Purchase details
- `ExpirationDate` (DateTimeOffset?)
- `Tags` (List<string>)
- `DocumentReferences` (List<Guid>) - Related documents (manuals, receipts)
- `CreatedAt` (DateTimeOffset)
- `UpdatedAt` (DateTimeOffset)

#### StorageLocation
Physical location where items are stored.

**Attributes:**
- `LocationId` (Guid)
- `Name` (string) - e.g., "Kitchen", "Basement", "Garage"
- `Type` (enum) - Room, Shelf, Container, Drawer
- `ParentLocationId` (Guid?) - For hierarchical structure
- `Path` (string) - Full hierarchical path
- `Description` (string)
- `Capacity` (decimal?) - Optional capacity limit
- `Unit` (string?) - Unit for capacity

#### ItemStock
Tracks quantity of items at specific locations.

**Attributes:**
- `StockId` (Guid)
- `ItemId` (Guid)
- `LocationId` (Guid)
- `Quantity` (decimal)
- `LastUpdated` (DateTimeOffset)
- `Notes` (string)

**Example:**
- Item: "Pasta"
- Location: "Basement" → Quantity: 3
- Location: "Kitchen Pantry" → Quantity: 1

#### StockMovement
Historical record of stock changes.

**Attributes:**
- `MovementId` (Guid)
- `ItemId` (Guid)
- `FromLocationId` (Guid?)
- `ToLocationId` (Guid?)
- `Quantity` (decimal)
- `MovementType` (enum) - Add, Remove, Transfer, Adjustment
- `Reason` (string)
- `PerformedBy` (Guid)
- `PerformedAt` (DateTimeOffset)

#### StorageCategory
Categorization of stored items.

**Attributes:**
- `CategoryId` (Guid)
- `Name` (string) - e.g., "Food", "Tools", "Electronics"
- `ParentCategoryId` (Guid?)
- `StorageConditions` (string?) - e.g., "Keep refrigerated", "Store in dry place"

### Value Objects

- `Quantity` - Value + Unit
- `PurchaseInfo` - Date, Price, Store, Receipt reference
- `LocationPath` - Hierarchical location representation
- `StockAlert` - Low stock warning configuration

### Domain Services

- `InventoryService` - Stock management operations
- `LocationService` - Location hierarchy management
- `StockAlertService` - Low stock notifications
- `BarcodeService` - Barcode scanning and lookup
- `ExpirationTracker` - Expiration date monitoring

## Cross-Domain Relationships

### Finance → Documents
- Transactions can have attached receipts/invoices (Document references)
- `Transaction.Attachments → List<DocumentId>`

### Storage → Documents
- Items can reference product manuals, warranties, receipts
- `StorageItem.DocumentReferences → List<DocumentId>`

### Finance → Storage
- Purchase transactions may link to items added to inventory
- Optional integration: Transaction creates or references Storage items
- `Transaction.InventoryItemId (Guid?)` - Optional reference

### Documents → All Domains
- Documents can be tagged/categorized for finance, storage, or general use
- Unified search across all document types
- Document metadata includes domain context

## Shared Concepts

### User
User account (shared across all domains).

**Attributes:**
- `UserId` (Guid)
- `Email` (string)
- `DisplayName` (string)
- `Roles` (List<string>)
- `Preferences` (Dictionary<string, string>)
- `CreatedAt` (DateTimeOffset)

### Tag
Flexible tagging across all domains.

**Attributes:**
- `Tag` (string)
- `Usage Count` (int)
- `Color` (string?)

### AuditLog
Audit trail for all domains.

**Attributes:**
- `LogId` (Guid)
- `EntityType` (string)
- `EntityId` (Guid)
- `Action` (enum) - Create, Update, Delete
- `UserId` (Guid)
- `Timestamp` (DateTimeOffset)
- `Changes` (JSON) - Before/after values

## Domain Boundaries

Each domain maintains its own:
- Database schema
- API endpoints
- Business rules
- Data validation

Cross-domain communication occurs only through:
- Well-defined API contracts
- Reference IDs (no direct database access)
- Eventual consistency patterns where appropriate

## Ubiquitous Language

### Finance Domain Terms
- **Transaction**: Financial event (income/expense)
- **Budget**: Spending limit for a category/period
- **Account**: Financial account (bank, cash, card)
- **Category**: Classification of transactions
- **Recurring**: Repeating transaction pattern

### Documents Domain Terms
- **Document**: Managed file with metadata
- **Version**: Historical state of a document
- **Folder**: Organizational container
- **Share**: Access grant to another user
- **OCR**: Optical Character Recognition

### Storage Domain Terms
- **Item**: Physical object in inventory
- **Stock**: Quantity of item at a location
- **Location**: Physical storage place
- **Movement**: Change in stock quantity/location
- **Expiration**: Date when item is no longer usable
- **Barcode**: Machine-readable item identifier

This domain model provides a foundation for implementation while maintaining clear boundaries between the three core domains of the application.
