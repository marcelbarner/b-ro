# System Scope and Context

## Business Context

The system operates in a household/small business management context, providing integrated management capabilities across three distinct but interconnected domains:

### Core Domains

#### 1. Finance Domain
The Finance domain handles all monetary transactions and financial data management. This includes:
- Income and expense tracking
- Budget management
- Financial reporting and analytics
- Payment processing
- Account reconciliation

**External Interfaces:**
- Bank APIs for transaction imports
- Payment gateways
- Tax calculation services

#### 2. Documents Domain
The Documents domain implements a Document Management System (DMS) for comprehensive document handling, storage, and organization. This includes:
- Document upload and storage
- Version control and history
- Metadata management and tagging
- Full-text search capabilities
- Access control and sharing

**External Interfaces:**
- Cloud storage providers (optional backup)
- OCR services for text extraction
- Email integration for document imports

#### 3. Storage Domain
The Storage domain manages physical inventory and storage location tracking. This includes:
- Inventory item management
- Storage location hierarchy (e.g., rooms, shelves, containers)
- Quantity tracking across multiple locations
- Low-stock alerts
- Item categorization and search

**Example Use Case:** 3 packs of pasta in the basement, 1 pack in the kitchen

**External Interfaces:**
- Barcode/QR code scanning services
- Shopping list applications

### Domain Relationships

```
┌─────────────────┐
│  Finance Domain │
└────────┬────────┘
         │ References
         ▼
┌─────────────────┐         ┌──────────────────┐
│ Documents Domain│◄────────┤ Storage Domain   │
└─────────────────┘         └──────────────────┘
    Attachments                Item Documentation
```

- **Finance → Documents**: Receipts, invoices, and financial documents can be attached to transactions
- **Storage → Documents**: Product manuals, warranties, and purchase receipts can be linked to inventory items
- **Finance ↔ Storage**: Purchase transactions may reference items added to storage

## Technical Context

The system follows a microservices-oriented architecture with domain-driven design principles:

### Service Architecture

```
┌──────────────────────────────────────────────────┐
│              Frontend (Angular 19)                │
│         Basic View | Expert View Modes            │
└───────────────────┬──────────────────────────────┘
                    │ HTTPS/REST
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
┌───────────────┐       ┌───────────────┐
│   API Gateway │       │   API Gateway │
│   (Optional)  │       │   (Optional)  │
└───────┬───────┘       └───────┬───────┘
        │                       │
        └───────────┬───────────┘
                    │
    ┌───────────────┼───────────────┐
    │               │               │
    ▼               ▼               ▼
┌─────────┐   ┌──────────┐   ┌──────────┐
│ Finance │   │Documents │   │ Storage  │
│ Service │   │ Service  │   │ Service  │
│(.NET 9) │   │(.NET 9)  │   │(.NET 9)  │
└────┬────┘   └────┬─────┘   └────┬─────┘
     │             │              │
     └─────────────┼──────────────┘
                   │
                   ▼
          ┌────────────────┐
          │  Shared Libs   │
          │ (Common Code)  │
          └────────────────┘
```

### Communication Protocols

- **Frontend ↔ Backend**: RESTful APIs over HTTPS
- **Service ↔ Service**: Direct HTTP calls or message bus (to be defined in deployment view)
- **Data Format**: JSON for API communication

### Deployment Context

All services and components run within Docker containers, orchestrated via Docker Compose for local development and potentially Kubernetes for production deployment (to be defined in deployment view).
