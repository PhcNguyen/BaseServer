# Lớp Singleton - Quản lý các instance duy nhất

Lớp `Singleton` cung cấp một cách tiếp cận đơn giản để quản lý các instance duy nhất của các lớp trong ứng dụng. 
Lớp này sử dụng `ConcurrentDictionary` để lưu trữ các instance đã khởi tạo và hỗ trợ nhiều phương thức để lấy, kiểm tra và reset các instance này.

## Các phương thức

### `GetAllRegisteredTypes()`
**Describe**: Return danh sách các loại (types) đã được đăng ký trong Singleton.
**Return**: `IEnumerable<Type>` - danh sách các loại đã được đăng ký.

### `IsInstanceCreated<T>()` 
**Describe**: Kiểm tra xem instance của một lớp đã được tạo hay chưa.
**Parameter**:
   - `T`: Loại của lớp cần kiểm tra.
**Return**: `bool` - `True` nếu instance đã được tạo, ngược lại `False`.

### `GetInstance<T>()` 
**Describe**: Lấy instance của một lớp singleton. Nếu instance chưa tồn tại, phương thức sẽ tự động khởi tạo một instance mới.
**Parameter**: 
   - `T`: Loại của lớp cần lấy instance.
**Return**: `T` - instance duy nhất của lớp.

### `GetInstance<T>(Func<T> initializer = null)` 
**Describe**: Lấy instance của một lớp singleton với hàm khởi tạo tùy chọn.
**Parameter**:
    - `initializer`: Hàm khởi tạo tùy chọn. Nếu không cung cấp, một instance mới sẽ được tạo ra.
**Return**: `T` - instance duy nhất của lớp.

### `GetInstance<T>(params object[] args)` 
**Describe**: Lấy instance của một lớp singleton với các Parameter khởi tạo.
**Parameter**:
   - `args`: Các Parameter được truyền vào constructor của lớp.
**Return**: `T` - instance duy nhất của lớp.

### `ResetInstance<T>()` 
**Describe**: Reset instance của lớp singleton và gọi `Dispose` nếu cần.
**Parameter**:
   - `T`: Loại của lớp cần reset instance.

### `ResetAll()` 
**Describe**: Reset tất cả các instance của lớp singleton và gọi `Dispose` nếu cần.

---

## **Giải thích về Singleton**
Lớp `Singleton` quản lý một instance duy nhất của mỗi lớp. Khi bạn gọi phương thức `GetInstance<T>()`, 
nó sẽ kiểm tra xem instance của lớp `T` đã tồn tại chưa. Nếu chưa, nó sẽ tạo một instance mới và Return.

**Các phương thức khởi tạo linh hoạt**
Bạn có thể khởi tạo instance của một lớp với các cách khác nhau:
1. **Khởi tạo mặc định**: Nếu không có Parameter khởi tạo, lớp sẽ tự động tạo một instance mới.
2. **Khởi tạo với hàm tùy chỉnh**: Bạn có thể cung cấp một hàm khởi tạo tùy chọn (`Func<T>`).
3. **Khởi tạo với các Parameter**: Lớp hỗ trợ việc khởi tạo với các Parameter (Parameter constructor).

**Reset Instance**
Lớp `Singleton` cho phép reset instance của một lớp hoặc tất cả các lớp đã đăng ký, giúp giải phóng tài nguyên và gọi `Dispose` nếu cần.

---

## **Lợi ích của Singleton**
- **Quản lý tài nguyên**: Singleton giúp quản lý tài nguyên bằng cách đảm bảo chỉ có một instance của mỗi lớp trong suốt vòng đời của ứng dụng.
- **Tiết kiệm bộ nhớ**: Vì chỉ có một instance, Singleton giúp tiết kiệm bộ nhớ và giảm chi phí khởi tạo đối tượng.
- **Dễ dàng truy cập**: Bạn có thể truy cập các đối tượng singleton một cách dễ dàng từ mọi nơi trong ứng dụng mà không cần phải tạo mới đối tượng mỗi lần.

---

## **Cách sử dụng Singleton**

```csharp
// Lấy instance của một lớp
var instance = Singleton.GetInstance<MyClass>();

// Kiểm tra xem instance của lớp đã được tạo chưa
bool isCreated = Singleton.IsInstanceCreated<MyClass>();

// Reset instance của lớp
Singleton.ResetInstance<MyClass>();

// Lấy tất cả các loại đã đăng ký trong Singleton
var types = Singleton.GetAllRegisteredTypes();
```