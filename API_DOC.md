# Expense Manager API Documentation

This document describes the API endpoints for the Expense Manager application. All endpoints are prefixed with `/api/v1`.

## Authentication

Authentication is handled via JWT tokens. Include the token in the `Authorization` header as `Bearer <token>`.

### Auth Endpoints

#### POST `/api/v1/auth/register`

Registers a new user.

- **Request Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!",
    "userName": "johndoe"
  }
  ```
- **Response**: `201 Created`
  ```json
  {
    "token": "JWT_TOKEN_HERE",
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "userName": "johndoe",
      "role": "User"
    }
  }
  ```

#### POST `/api/v1/auth/login`

Authenticates a user and returns a token.

- **Request Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```
- **Response**: `200 OK`
  ```json
  {
    "token": "JWT_TOKEN_HERE",
    "user": { ... }
  }
  ```

---

## Expenses

All expense endpoints require the `Authorization` header.

#### GET `/api/v1/expense`

Retrieves all expenses for the current user. Supports filtering via query parameters.

- **Query Parameters (Optional)**:
  - `fromDate`: `DateOnly` (YYYY-MM-DD)
  - `toDate`: `DateOnly` (YYYY-MM-DD)
  - `categoryId`: `Guid`
  - `minAmount`: `decimal`
  - `maxAmount`: `decimal`
  - `currency`: `string`
- **Response**: `200 OK`
  ```json
  [
    {
      "id": "guid",
      "amount": 100.0,
      "currency": "USD",
      "exchangeRate": 1.0,
      "description": "Lunch",
      "date": "2026-03-22T10:00:00Z",
      "categoryId": "guid",
      "categoryName": "Food",
      "receiptId": "guid"
    }
  ]
  ```


#### GET `/api/v1/expense/{id}`

Retrieves a specific expense by ID.

- **Response**: `200 OK` or `404 Not Found`

#### POST `/api/v1/expense`

Creates a new expense.

- **Request Body**:
  ```json
  {
    "categoryId": "guid",
    "amount": 100.0,
    "currency": "USD",
    "exchangeRate": 1.0,
    "description": "Lunch",
    "date": "2026-03-22T10:00:00Z",
    "receiptId": "guid"
  }
  ```
- `receiptId` is optional. When set, the receipt must belong to the current user. Multiple expenses may share the same receipt.
- **Response**: `201 Created`

#### PUT `/api/v1/expense/{id}`

Updates an existing expense.

- **Request Body**: (Same as POST; `receiptId` optional — omit or `null` to clear)
- **Response**: `200 OK`

#### DELETE `/api/v1/expense/{id}`

Deletes an expense.

- **Response**: `204 No Content`

---

## Receipts

All receipt endpoints require the `Authorization` header. Allowed file types: JPEG, PNG, WebP, PDF. Max size: 5 MB.

#### POST `/api/v1/receipt`

Uploads a receipt file to Azure Blob Storage.

- **Content-Type**: `multipart/form-data`
- **Form field**: `file` (required)
- **Response**: `201 Created`
  ```json
  {
    "id": "guid",
    "fileName": "lunch.jpg",
    "fileUrl": "https://.../receipts/...",
    "contentType": "image/jpeg",
    "size": 123456,
    "status": "Uploaded",
    "lineItemCount": 0,
    "createdAt": "2026-07-16T10:00:00Z"
  }
  ```
- `status`: `Pending` | `Uploaded` | `Failed` | `Processing` | `ReadyForReview` | `Confirmed` | `OcrFailed`

Upload automatically enqueues OCR processing in the background.

#### GET `/api/v1/receipt`

Lists receipts for the current user. Returns lightweight metadata only (does not include OCR line-item JSON).

- **Response**: `200 OK` (array of receipt objects; includes `lineItemCount`)

#### GET `/api/v1/receipt/{id}`

Returns receipt metadata.

- **Response**: `200 OK` or `404 Not Found`

#### GET `/api/v1/receipt/{id}/status`

Returns lightweight OCR processing status for polling (no line-item payload).

- **Response**: `200 OK`
  ```json
  {
    "receiptId": "guid",
    "status": "Processing",
    "lineItemCount": 0,
    "ocrErrorMessage": null
  }
  ```

#### GET `/api/v1/receipt/{id}/extraction`

Returns OCR-extracted fields for user review.

- **Response**: `200 OK`
  ```json
  {
    "receiptId": "guid",
    "status": "ReadyForReview",
    "merchant": "Coffee Shop",
    "transactionDate": "2026-07-16T10:00:00Z",
    "totalAmount": 12.50,
    "currency": "USD",
    "taxAmount": 1.05,
    "suggestedCategoryId": "guid",
    "ocrErrorMessage": null,
    "lineItems": [
      {
        "description": "Latte",
        "quantity": 1,
        "unitPrice": 4.50,
        "totalPrice": 4.50,
        "suggestedCategoryId": "guid"
      }
    ]
  }
  ```

#### POST `/api/v1/receipt/{id}/confirm`

Creates one or more expenses from reviewed OCR line items. All expenses share the same `receiptId`. Receipt must be `ReadyForReview` and must not already have linked expenses.

- **Request Body**:
  ```json
  {
    "currency": "USD",
    "date": "2026-07-16T10:00:00Z",
    "importMode": "Itemized",
    "expenses": [
      {
        "amount": 4.50,
        "categoryId": "guid",
        "description": "Latte"
      },
      {
        "amount": 8.00,
        "categoryId": "guid",
        "description": "Sandwich"
      }
    ]
  }
  ```
- `importMode`: `Combined` | `Itemized` (defaults to `Itemized` if omitted)
  - `Combined`: exactly one expense (bill total); line items remain on the receipt for reference
  - `Itemized`: one expense per selected line item; unselected lines are not imported
- **Response**: `201 Created` (array of expense objects)

#### POST `/api/v1/receipt/{id}/retry-ocr`

Re-queues OCR for a receipt in `OcrFailed` status.

- **Response**: `204 No Content`

#### GET `/api/v1/receipt/{id}/file`

Streams the receipt file for preview/download (`Content-Disposition: inline`).

- **Response**: `200 OK` (file bytes) or `404 Not Found`

#### DELETE `/api/v1/receipt/{id}`

Soft-deletes the receipt, removes the blob, and clears `receiptId` on linked expenses.

- **Response**: `204 No Content`

---

## Categories

#### GET `/api/v1/category`

Retrieves all predefined categories and user-specific categories.

- **Response**: `200 OK`
  ```json
  [
    {
      "id": "guid",
      "name": "Food",
      "description": "Food and dining",
      "isPredefined": true,
      "userId": null
    }
  ]
  ```

#### POST `/api/v1/category`

Creates a new custom category for the user. (Requires Auth)

- **Request Body**:
  ```json
  {
    "name": "Hobbies",
    "description": "Gaming and sports"
  }
  ```
- **Response**: `201 Created`

#### DELETE `/api/v1/category/{id}`

Deletes a user-specific category. (Requires Auth)

- **Response**: `204 No Content`

---

## Reports and Export

All report/export endpoints require the `Authorization` header.

#### GET `/api/v1/report/monthly-summary`

Gets summary for a specific month.

- **Query Parameters**:
  - `year`: `int`
  - `month`: `int`
- **Response**: `200 OK`
  ```json
  {
    "year": 2026,
    "month": 3,
    "totalAmount": 1500.0,
    "transactionCount": 15,
    "averageTransactionAmount": 100.0
  }
  ```

#### GET `/api/v1/report/category-breakdown`

Gets expense breakdown by category for a specific month.

- **Query Parameters**:
  - `year`: `int`
  - `month`: `int`
- **Response**: `200 OK`
  ```json
  [
    {
      "categoryId": "guid",
      "categoryName": "Food",
      "totalAmount": 500.0
    }
  ]
  ```

#### GET `/api/v1/export/csv`

Exports user's expenses to a CSV file.

- **Query Parameters**: (Same as GET `/api/v1/expense`)
- **Response**: `200 OK` (File Download: `text/csv`)
