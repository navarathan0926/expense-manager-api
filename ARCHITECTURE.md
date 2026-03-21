## Multi-Currency Support

Each expense stores its original `Amount`, `Currency` (ISO 4217 code 
e.g. USD, LKR), and an optional `ExchangeRate` field.

Full multi-currency conversion is not implemented in this version. 
The `ExchangeRate` field is nullable and reserved for future implementation 
where amounts can be normalized to a base currency for cross-currency 
reporting and summaries.

