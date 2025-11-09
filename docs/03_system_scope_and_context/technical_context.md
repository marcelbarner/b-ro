# Technical Context

## Technology Stack

### Backend
- **Framework**: .NET 9.0
- **Language**: C# 12+
- **API Style**: RESTful Web APIs (ASP.NET Core)
- **Runtime**: Docker containers

### Frontend
- **Framework**: Angular 20
- **UI Libraries**:
  - ng-matero (admin template)
  - ng-matero/extensions
  - Angular Material 20 (component library)
  - ng-matero 20 (admin template)
- **Language**: TypeScript 5.x

### Infrastructure
- **Containerization**: Docker
- **Orchestration**: Docker Compose (development and production)
- **Reverse Proxy**: To be defined (e.g., nginx, Traefik)

## Technical Interfaces

### API Endpoints Structure

Each domain service exposes RESTful endpoints following consistent patterns:

```
/api/finance/*
/api/documents/*
/api/storage/*
```

### Data Exchange Format

- **Request/Response**: JSON
- **Date/Time**: ISO 8601 format
- **Character Encoding**: UTF-8
- **API Versioning**: URL-based (e.g., /api/v1/*)

### Authentication & Authorization

- **Authentication**: JWT-based authentication (to be detailed in security concepts)
- **Authorization**: Role-based access control (RBAC)
- **Token Storage**: HTTP-only cookies or Authorization header

### File Handling (Documents Domain)

- **Upload**: Multipart form-data
- **Download**: Binary stream with appropriate MIME types
- **Maximum File Size**: To be configured per deployment
- **Supported Formats**: All common document formats (PDF, DOCX, XLSX, images, etc.)

## External System Integration

### Planned External Interfaces

1. **Banking APIs** (Finance Domain)
   - Protocol: REST/HTTPS
   - Format: JSON (bank-specific formats)
   - Purpose: Transaction import, account balance retrieval

2. **Cloud Storage** (Documents Domain - optional)
   - Protocol: Provider-specific APIs (S3, Azure Blob, etc.)
   - Purpose: Backup and archival storage

3. **OCR Services** (Documents Domain)
   - Protocol: REST/HTTPS
   - Purpose: Text extraction from scanned documents

## Network Architecture

### Port Allocation (Docker Compose)

```yaml
Services:
  frontend:         80, 443 (HTTPS)
  finance-api:      5001 (internal)
  documents-api:    5002 (internal)
  storage-api:      5003 (internal)
  
Databases:
  postgres:         5432 (internal)
  # Or separate DB per service
```

### Container Communication

- **Frontend → Backend**: Via exposed API ports
- **Service → Service**: Internal Docker network
- **All → Database**: Internal Docker network on dedicated database network

## Development Environment

### Local Development Setup

```bash
# Start all services
docker-compose up -d

# Frontend dev server (hot reload)
http://localhost:4200

# Backend APIs (Swagger/OpenAPI)
http://localhost:5001/swagger
http://localhost:5002/swagger
http://localhost:5003/swagger
```

### Build and Deployment

- **Backend Build**: `dotnet build` → Docker image
- **Frontend Build**: `ng build --prod` → Docker image with nginx
- **Orchestration**: `docker-compose build && docker-compose up`
