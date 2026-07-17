# Expense Manager Improvement Plan

## Phase 1 - Receipt Upload

- Create `Receipt` entity
- Upload receipt images (Azure Blob Storage)
- Link receipts with expenses
- Preview/download uploaded receipts
- Store receipt metadata:
  - File name
  - File URL
  - Upload date
  - Processing status

---

## Phase 2 - OCR Processing

- User uploads receipt image
- Store image in Blob Storage
- Create background OCR processing job
- Extract receipt information:
  - Merchant
  - Date
  - Total Amount
  - Currency
  - Tax (optional)
  - Suggested Category
- User reviews extracted data
- Create/update expense from extracted information

---

## Phase 3 - Background Processing (Hangfire)

Implement background jobs for:

- OCR processing
- Email notifications
- Monthly expense summary generation
- Recurring expense generation
- Temporary file cleanup
- Report generation

---

## Phase 4 - Audit Log System

Track important user activities:

- Login / Logout
- Expense Created
- Expense Updated
- Expense Deleted
- Budget Updated
- Receipt Uploaded
- Receipt Deleted
- OCR Processing Completed
- Export Generated

Store:

- User
- Action
- Entity
- Entity ID
- Timestamp
- IP Address (optional)

---

## Phase 5 - Rate Limiting

Apply rate limits for sensitive APIs:

- Login API
- Register API
- Forgot Password API
- OCR Upload API
- Email Sending APIs
- Export APIs

Examples:

- Login → 5 requests/minute
- OCR Upload → 10 uploads/hour

---

## Phase 6 - Email Notifications

Implement email notifications for:

- Email verification
- Forgot password
- Budget exceeded alerts
  - 80% usage warning
  - 100% exceeded warning
- Monthly expense summary
- OCR processing completion

---

## Phase 7 - Dashboard Improvements

Improve dashboard with:

- Spending analytics
- Recent transactions
- Recent audit activities
- OCR processing status
- Receipt statistics
- Budget alerts
- Monthly expense reports

---

## Phase 8 - AI Finance Assistant (PostgreSQL MCP)

Create an AI-powered finance assistant that can understand user queries and retrieve information from PostgreSQL using MCP.

Examples:

User queries:

- "How much did I spend on food this month?"
- "Show my top spending categories."
- "Compare my expenses with last month."
- "Did I exceed my monthly budget?"
- "Show expenses above Rs. 10,000."
- "Generate my monthly expense summary."
- "Show recent account activities."

Admin queries:

- "Who deleted an expense yesterday?"
- "Show today's user activities."
- "How many receipts are pending OCR processing?"

Architecture:
Next.js Chat Interface
│
▼
.NET Backend API
│
▼
AI Model
│
▼
PostgreSQL MCP Server
│
▼
PostgreSQL Database

Additional MCP tools:

- Get expense summary
- Get spending analytics
- Search transactions
- Get audit logs
- Generate reports

---

## Database Additions

New tables:

- `Receipts`
- `AuditLogs`
- `EmailTemplates`
- `ChatHistory`
- `AIRequests` (optional)

---

## Overall Flow

Upload Receipt
│
▼
Save Image (Azure Blob Storage)
│
▼
Create Background Job (Hangfire)
│
▼
OCR Extracts Data
│
▼
User Reviews Extracted Information
│
▼
Expense Created/Updated
│
▼
Audit Log Recorded
│
▼
Budget Recalculated
│
▼
Email Notification (if required)
│
▼
AI Finance Assistant Provides Insights
