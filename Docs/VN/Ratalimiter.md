# RequestLimiter Documentation

## Overview
Lớp `RequestLimiter` được thiết kế để quản lý và giới hạn số lượng yêu cầu có thể được thực hiện bởi một địa chỉ IP cụ thể trong một khoảng thời gian nhất định.Nó cung cấp các chức năng để chặn các địa chỉ IP vượt quá giới hạn yêu cầu và xóa các yêu cầu không hoạt động.
 
 ## Constructor
 ```csharp
 public RequestLimiter(int limit, int timeWindow, int lockoutDuration = 300)
 ```
 ### Parameters
 - **`limit`**: Số lượng yêu cầu tối đa được phép trong khoảng thời gian.
 - **`timeWindow`**: Khoảng thời gian tính bằng giây trong đó các yêu cầu được đếm.
 - **`lockoutDuration`**: Thời gian tính bằng giây mà một địa chỉ IP bị chặn nếu vượt quá giới hạn yêu cầu.Mặc định là 300 giây (5 phút).
 
 ## Methods
 
 ### `Task<bool> IsAllowed(string ipAddress)`
 Kiểm tra xem địa chỉ IP đã cho có được phép thực hiện yêu cầu hay không.
 
 #### Parameters
 - **`ipAddress`**: Địa chỉ IP để kiểm tra.
 
 #### Returns
 - `Task<bool>`: Trả về `true` nếu địa chỉ IP được phép thực hiện yêu cầu, `false` nếu bị chặn.
 
 #### Example Usage
 ```csharp
 var requestLimiter = new RequestLimiter(10, 60); // Giới hạn 10 yêu cầu mỗi phút
bool isAllowed = await requestLimiter.IsAllowed("192.168.1.1");
 
 if (isAllowed)
 {
     // Xử lý yêu cầu
 }
 else
{
    // Chặn yêu cầu
}
 ```
 
 ### `Task ClearInactiveRequests()`
 Định kỳ xóa danh sách các yêu cầu của người dùng cho các địa chỉ IP không có yêu cầu trong khoảng thời gian quy định.
 
 #### Example Usage
 ```csharp
 var requestLimiter = new RequestLimiter(10, 60);
await requestLimiter.ClearInactiveRequests();
 ```
 
 ## Internal Details
 
 ### Private Fields
 -**`_limit`**: Số lượng yêu cầu tối đa được phép.
 - **`_timeWindow`**: Khoảng thời gian tính bằng giây.
 - **`_lockoutDuration`**: Thời gian tính bằng giây mà IP bị chặn.
 - **`_lock`**: Semaphore để đảm bảo an toàn cho luồng.
 - **`_blockedIps`**: Từ điển để theo dõi các địa chỉ IP bị chặn và thời gian kết thúc chặn của chúng.
 - **`_userRequests`**: Từ điển để theo dõi các yêu cầu được thực hiện bởi mỗi địa chỉ IP.
 
 ### Method Implementations
 
 #### `IsAllowed`
 - **Checks if the IP is blocked**: Nếu IP nằm trong danh sách chặn và thời gian hiện tại nhỏ hơn thời gian kết thúc chặn, trả về `false`.
 - **Removes expired blocks**: Xóa IP khỏi danh sách chặn nếu thời gian chặn đã hết hạn.
 - **Maintains request history**: Thêm thời gian yêu cầu hiện tại vào lịch sử và xóa các yêu cầu cũ hơn thời gian quy định.
 - **Checks request count**: Nếu số lượng yêu cầu ít hơn giới hạn, cho phép yêu cầu; nếu không, chặn IP.
 
 #### `ClearInactiveRequests`
 - **Clears inactive IPs**: Định kỳ kiểm tra và xóa các địa chỉ IP khỏi danh sách yêu cầu nếu không có yêu cầu nào trong khoảng thời gian quy định.
 
 ## Example
 
 ```csharp
 var requestLimiter = new RequestLimiter(5, 60, 300); // Cho phép 5 yêu cầu mỗi phút, chặn trong 5 phút nếu vượt quá
bool isAllowed = await requestLimiter.IsAllowed("192.168.1.2");

if (isAllowed)
{
    Console.WriteLine("Request allowed");
}
else
{
    Console.WriteLine("Request blocked");
}

// Chạy điều này trong một tác vụ nền để xóa các yêu cầu không hoạt động
_ = Task.Run(async () => await requestLimiter.ClearInactiveRequests());
 ```
 
 Tài liệu này cung cấp tổng quan, chi tiết phương thức và ví dụ sử dụng cho lớp `RequestLimiter`. Nếu bạn có bất kỳ câu hỏi nào hoặc cần hỗ trợ thêm, hãy cho tôi biết! 😊
