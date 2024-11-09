# RequestLimiter Documentation
 
## Overview
The `RequestLimiter` class is designed to manage and limit the number of requests that can be made by a specific IP address within a given time window. It provides functionalities to block IP addresses that exceed the request limit and to clear inactive requests.
 
## Constructor
```csharp
public RequestLimiter(int limit, int timeWindow, int lockoutDuration = 300)
```
### Parameters
- `limit`: The maximum number of requests allowed within the time window.
- `timeWindow`: The time window in seconds within which the requests are counted.
- `lockoutDuration`: The duration in seconds for which an IP address is blocked if it exceeds the request limit. Default is 300 seconds (5 minutes).
 
## Methods
 
### `Task<bool> IsAllowed(string ipAddress)`
Checks if the given IP address is allowed to make a request.
 
#### Parameters
- `ipAddress`: The IP address to check.
 
#### Returns
- `Task<bool>`: Returns `true` if the IP address is allowed to make a request, `false` if it is blocked.
 
#### Example Usage
```csharp
var requestLimiter = new RequestLimiter(10, 60); // Limit to 10 requests per minute
bool isAllowed = await requestLimiter.IsAllowed("192.168.1.1");
 
if (isAllowed)
{
    // Process the request
}
else
{
    // Block the request
}
```
 
### `Task ClearInactiveRequests()`
Periodically clears the list of user requests for IP addresses that have no requests within the specified time window.
 
#### Example Usage
```csharp
var requestLimiter = new RequestLimiter(10, 60);
await requestLimiter.ClearInactiveRequests();
```
 
## Internal Details
 
### Private Fields
- `_limit`: The maximum number of requests allowed.
- `_timeWindow`: The time window in seconds.
- `_lockoutDuration`: The duration in seconds for which the IP is blocked.
- `_lock`: A semaphore to ensure thread safety.
- `_blockedIps`: A dictionary to keep track of blocked IP addresses and their block end time.
- `_userRequests`: A dictionary to keep track of requests made by each IP address.
 
### Method Implementations
 
#### `IsAllowed`
- **Checks if the IP is blocked**: If the IP is in the blocked list and the current time is less than the block end time, it returns `false`.
- **Removes expired blocks**: Removes the IP from the blocked list if the block duration has expired.
- **Maintains request history**: Adds the current request time to the history and removes requests older than the time window.
- **Checks request count**: If the number of requests is less than the limit, it allows the request; otherwise, it blocks the IP.
 
#### `ClearInactiveRequests`
- **Clears inactive IPs**: Periodically checks and removes IP addresses from the request list if they have no requests within the specified time window.
 
## Example
 
```csharp
var requestLimiter = new RequestLimiter(5, 60, 300); // Allow 5 requests per minute, block for 5 minutes if exceeded
bool isAllowed = await requestLimiter.IsAllowed("192.168.1.2");
 
if (isAllowed)
{
    Console.WriteLine("Request allowed");
}
else
{
    Console.WriteLine("Request blocked");
}
 
// Run this in a background task to clear inactive requests
_ = Task.Run(async () => await requestLimiter.ClearInactiveRequests());
```
 
This documentation provides an overview, method details, and example usage for the `RequestLimiter` class. If you have any questions or need further assistance, feel free to ask! 😊
