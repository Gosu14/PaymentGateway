# PaymentGateway
Payment Gateway Challenge: Building a Payment Gateway

- [Routes](#Routes)
    - [payment-demand](#payment-demand)
    - [payment-details](#payment-details)
- [Usages](#Usages)
    - [Data-validation](#Data-validation)
- [Architecture](#Architecture)
    - [Projects](#Projects)
    - [Code](#Code)
- [CI/CD](#CI/CD)
    - [Github-Workflows](#Github-Workflows)
- [Potential-Enhancement](#Potential-Enhancement)

# Routes
## **payment-demand**
----
  Route to process a payment request

* **URL**
 `POST` /payments/payment-demand
 
 **Auth Header Params**
 `ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5`
  
* **Data Params**
```json
{
  "amount": 0,
  "currency": "string",
  "paymentMethod": {
    "cardBrand": "string",
    "cardCountry": "string",
    "cardExpiryMonth": 0,
    "cardExpiryYear": 0,
    "cardNumber": "string",
    "cardCvv": "string"
  }
}
```
Valid Example (using test card data):

```json
{
  "amount": 500,
  "currency": "usd",
  "paymentMethod": {
    "cardBrand": "visa",
    "cardCountry": "fr",
    "cardExpiryMonth": 3,
    "cardExpiryYear": 2030,
    "cardNumber": "4977 9494 9494 9497",
    "cardCvv": "737"
  }
}
```
or

```json
{
  "amount": 500,
  "currency": "usd",
  "paymentMethod": {
    "cardBrand": "mastercard",
    "cardCountry": "gb",
    "cardExpiryMonth": 3,
    "cardExpiryYear": 2030,
    "cardNumber": "5555 5555 5555 4444",
    "cardCvv": "737"
  }
}
```

* **Success Response:**

  * **Code:** 200 <br />
    **Content:**
    ```json
    {
        "id": "Guid",
        "status": "string",
        "amount": 0,
        "currency": "string",
        "type": 0,
        "cardBrand": "string",
        "cardCountry": "string",
        "cardExpiryYear": 0,
        "last4": "string"
     }
     ```
     Example:
     ```json
     {
        "id": "b123b0e1-74b8-47c9-8140-2beb6c84a277",
        "status": "PAYMENT_ACCEPTED",
        "amount": 500,
        "currency": "usd",
        "type": 1,
        "cardBrand": "mastercard",
        "cardCountry": "gb",
        "cardExpiryYear": 2030,
        "last4": "4444"
     }
     ```
* **Error Response:**

  * **Code:** 401 UNAUTHORIZED <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        "title": "Unauthorized",
        "status": 401
    }
    ```
    OR
    
  * **Code:** 400 Validation Errors & Bad request data format <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        "title": "One or more validation errors occurred.",
        "status": 400,
        "errors": {}
    }
    ```
    OR
    
  * **Code:** 402 PAYMENT DECLINED <br />
    **Content - Payment confirmation object similar to response 200:**
    ```json
    {
        "id": "Guid",
        "status": "string",
        "amount": 0,
        "currency": "string",
        "type": 0,
        "cardBrand": "string",
        "cardCountry": "string",
        "cardExpiryYear": 0,
        "last4": "string"
     }
     ```
     Example:
     ```json
     {
        "id": "b547d346-29e9-4970-8874-e1734ab93ec5",
        "status": "PAYMENT_DECLINED_CARD_INVALID_NUMBER",
        "amount": 500,
        "currency": "usd",
        "type": 1,
        "cardBrand": "visa",
        "cardCountry": "gb",
        "cardExpiryYear": 2025,
        "last4": "4444"
     }
     ```
     OR
     
  * **Code:** 500 INTERNAL SERVER ERROR <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        "title": "An error occurred while processing your request.",
        "status": 500
    }
    ```
    
* **Sample Call:**
    **Valid Card:**
    ```curl
    curl -X POST "https://localhost:5001/payments/payment-demand" -H  "accept: */*" -H  "ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5" -H  "Content-Type: application/json" -d "{\"amount\":500,\"currency\":\"usd\",\"paymentMethod\":{\"cardBrand\":\"visa\",\"cardCountry\":\"fr\",\"cardExpiryMonth\":3,\"cardExpiryYear\":2030,\"cardNumber\":\"4977 9494 9494 9497\",\"cardCvv\":\"737\"}}"
    ```
    OR
     **Invalid Card:**
  ```curl
  curl -X POST "https://localhost:5001/payments/payment-demand" -H  "accept: */*" -H  "ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5" -H  "Content-Type: application/json" -d "{\"amount\":500,\"currency\":\"usd\",\"paymentMethod\":{\"paymentType\":\"card\",\"cardBrand\":\"visa\",\"cardCountry\":\"gb\",\"cardExpiryMonth\":\"11\",\"cardExpiryYear\":\"2025\",\"cardNumber\":\"5555 5555 5555 4444\",\"cardCvv\":\"555\"}}"
    ```
    OR
    **Stolen Card:**
    ```curl
    curl -X POST "https://localhost:5001/payments/payment-demand" -H  "accept: */*" -H  "ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5" -H  "Content-Type: application/json" -d "{\"amount\":500,\"currency\":\"usd\",\"paymentMethod\":{\"cardBrand\":\"visa\",\"cardCountry\":\"US\",\"cardExpiryMonth\":3,\"cardExpiryYear\":2030,\"cardNumber\":\"4000 0200 0000 0000\",\"cardCvv\":\"737\"}}"
    ```
  
## **payment-details**
----
Route to fetch payment confirmation processed

* **URL**
 `GET` /payments/payment-details
 
 **Auth Header Params**
 `ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5`
  
*  **URL Params**
   **Required:**
   `id=[string]`

* **Success Response:**

  * **Code:** 200 <br />
    **Content:**
    ```json
    {
        "id": "Guid",
        "status": "string",
        "amount": 0,
        "currency": "string",
        "type": 0,
        "cardBrand": "string",
        "cardCountry": "string",
        "cardExpiryYear": 0,
        "last4": "string"
     }
     ```
* **Error Response:**

 * **Code:** 401 UNAUTHORIZED <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        "title": "Unauthorized",
        "status": 401
    }
    ```
  OR
  
  * **Code:** 404 RESOURCE NOT FOUND <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        "title": "The specified resource was not found.",
        "status": 404
    }
    ```
  OR
  
  * **Code:** 500 INTERNAL SERVER ERROR <br />
    **Content:**
    ```json
    {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        "title": "An error occurred while processing your request.",
        "status": 500
    }
    ```

* **Sample Call:**

  ```curl
    curl -X GET "https://localhost:5001/payments/payment-details?id=theresourceid" -H  "accept: */*" -H  "ApiKey: ApiKey-11848329-5477-49A6-8182-03830D03AEA5""
    ```
  
# Usages
## **Data-validation**
* **payment-demand payload validations**
```js
{
  "amount": 0, //Amount must be between 0 and 10000000 - integer notation no decimal payment managed
  "currency": "string", //Currency must be a valid ISO 4217 code
  "paymentMethod": {
    "cardBrand": "string", //Card Brand must be either visa or mastercard - everything else will trigger a Payment Declined
    "cardCountry": "string", //Must be a valid ISO 3166-1 code
    "cardExpiryMonth": 0, //must be between 1 and 12 included
    "cardExpiryYear": 0, // could be anything but the year will be checked and anything in the past will trigger a Card Expired
    "cardNumber": "string", //Must be a valid Card number
    "cardCvv": "string" //Must have a lenght of minimum 3 and maximum 4
  }
}
```
* **Tests Data and Tests cases**
* As the Acquiring bank is mocked, I have implemented specific behavior on the mock
* Valid Card (will return PAYMENT_ACCEPTED status if amout and ccy are correct - **Only the 2 test cards below are valid**:
```json
{
  "amount": 500,
  "currency": "usd",
  "paymentMethod": {
    "cardBrand": "visa",
    "cardCountry": "fr",
    "cardExpiryMonth": 3,
    "cardExpiryYear": 2030,
    "cardNumber": "4977 9494 9494 9497",
    "cardCvv": "737"
  }
}
```
or

```json
{
  "amount": 500,
  "currency": "usd",
  "paymentMethod": {
    "cardBrand": "mastercard",
    "cardCountry": "gb",
    "cardExpiryMonth": 3,
    "cardExpiryYear": 2030,
    "cardNumber": "5555 5555 5555 4444",
    "cardCvv": "737"
  }
}
```
* Stolen Card data (will return PAYMENT_DECLINED_CARD_STOLEN status if amout and ccy are correct - **Only this card tests will return this status**:
```json
{
  "amount": 500,
  "currency": "usd",
  "paymentMethod": {
    "cardBrand": "visa",
    "cardCountry": "us",
    "cardExpiryMonth": 3,
    "cardExpiryYear": 2030,
    "cardNumber": "4000 0200 0000 0000",
    "cardCvv": "737"
  }
}
```
* On valid card if the amount requeste is above 1000000 - it will return PAYMENT_DECLINED_INSUFFICIENT_FUNDS status
* On valid card, playing with the fields other than cardBrand and cardNumber will trigger different other status code (PAYMENT_DECLINED_CARD_INVALID_CVV, PAYMENT_DECLINED_CARD_INVALID_EXPIRY_YEAR, PAYMENT_DECLINED_CARD_INVALID_EXPIRY_MONTH, PAYMENT_DECLINED_CARD_INVALID_NUMBER)
* Any other valid input with brand != visa or mastercard will return PAYMENT_DECLINED_CARD_NOT_SUPPORTED status
* Feel free to use the Swagger automaticaly generated to tests these

# Architecture
## **Projects**
**Target framework used:** .NET 5.0

**3 differents projects:**
  * **PaymentGateway.Api:** Api entry point, hosting, containing startup injection, Filters (Exception & Authentication), Middleware (Logging), DTO mapping and the PaymentController
  * **PaymentGateway.Application:** Main code business logic, Command handler, validation, Eceptions and the Mock IAcquiringBankGateway
  * **PaymentGateway.Domain:** Core object, interface and entities
  * **PaymentGateway.Infrastructure:** repositories management (DB) and other services infra related.

**2 Tests project:**
  * **PaymentGateway.Api.IntegrationsTests:** integration tests using TestServer and inMemory SQL DB
  * **PaymentGateway.Application.UnitTests:** business logic tests and unit tests - Testing Coomand Handler, Validation and AcquiringBankGateway behaviors
**Run:** local, WSL2 container or Docker container (linux) 

## **Code**
* **Framework leveraged on (apart from .Net):** EF (DB), FluentValidation (Validation), Xunit (tests), Moq (Mock), AutoMapper (Object Mapping), Swashbuckle (Swagger)
* **Persistence layer:** In Memory SQL database through EF
* **Security:** Managed via an ApiKey and HTTPS - only 1 ApiKey works for this POC - this can be enhanced by using IdentityServer to implement a complete OAUTH
* **Logging:** Managed via Logging Middleware implemented as an .Net Core middleware. Using Microsoft.Extensions.Logging.ILogger on standard outputs.
* **Validation:** Managed with FluentValidation
* **Mapping:** Managed with Automapper - use mainly to convert dto to domain object and vis versa
* **Exception:** Managed with an ExceptionFilter - Business exception are managed and logged centrally
* **Formating:** Use .editorconfig in the sln + dotnet format tool

# CI/CD
## **Github-Workflows**
* **Build & tests:** Github action setup to build and and tests when push/pull_request on master
* **Security"** CodeQL analysis running on pull_request on master
* **Release:** Release drafter used when new release are done on master

# Potential-Enhancement
* **logging:** Plug the output to external monitoring platform (Datadog, Application Insights, etc.)
* **Authentication:** Manage request routing and authorization outside of the API and on the hosting platform (ISTIO, etc.)
* **CI/CD:** Deploy container on a cloud platform
* **Architecture:** De-couple even more with an event based architecture (trigger event that could be consumed by other services) - for instance this API could just return an ID for a payment-demand and trigger a payment-demand event managed by another service. This could be done with framework such as Dapr or MassTransit.
* **Persistence**: Finish the data model properly and plug to sql instances
* **Webhook**: implement webhook for clients call back.

