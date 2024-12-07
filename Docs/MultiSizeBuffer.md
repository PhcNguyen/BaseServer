# MultiSizeBuffer Class - Quản lý các pool bộ đệm với nhiều kích thước khác nhau

Lớp `MultiSizeBuffer` chịu trách nhiệm quản lý các bộ đệm có kích thước khác nhau thông qua việc phân bổ và quản lý các pool bộ đệm. Nó sử dụng một `ConcurrentDictionary` để quản lý các pool bộ đệm, giúp việc thuê và trả bộ đệm trở nên dễ dàng và hiệu quả hơn.

## Các phương thức

### `AllocateBuffers()`
- **Description**: Phân bổ bộ đệm lần đầu. Nếu bộ đệm đã được phân bổ, phương thức sẽ ném ra một ngoại lệ `InvalidOperationException`.
- **Returns**: Không có giá trị trả về.

### `RentBuffer(int size)`
- **Description**: Thuê một bộ đệm với kích thước chỉ định.
- **Parameters**:
  - `size`: Kích thước bộ đệm yêu cầu.
- **Returns**: Bộ đệm byte phù hợp với kích thước yêu cầu.
- **Note**: Phương thức này tìm bộ đệm gần nhất trong các pool để đáp ứng yêu cầu và tự động tăng dung lượng của pool nếu cần.

### `ReturnBuffer(byte[] buffer)`
- **Description**: Trả lại bộ đệm vào pool.
- **Parameters**:
  - `buffer`: Bộ đệm cần trả lại.
- **Note**: Phương thức này kiểm tra kích thước bộ đệm và trả lại đúng pool.

### `AdjustBufferAllocationAsync(int bufferSize, double newPercentage)`
- **Description**: Điều chỉnh tỷ lệ phân bổ bộ đệm khi tài nguyên thay đổi.
- **Parameters**:
  - `bufferSize`: Kích thước bộ đệm cần điều chỉnh.
  - `newPercentage`: Tỷ lệ phân bổ mới.
- **Returns**: `Task` - phương thức này thực hiện bất đồng bộ và gọi lại việc phân bổ lại bộ đệm.

### `ReallocateBuffersAsync()`
- **Description**: Phân bổ lại bộ đệm theo tỷ lệ phân bổ mới.
- **Returns**: `Task` - phương thức này thực hiện bất đồng bộ và gọi lại phương thức `AllocateBuffers()` để phân bổ lại bộ đệm.

### `IncreaseBufferPoolSize(int bufferSize)`
- **Description**: Tăng kích thước pool bộ đệm khi cần thiết.
- **Parameters**:
  - `bufferSize`: Kích thước bộ đệm cần tăng.
- **Returns**: `Task` - phương thức này thực hiện bất đồng bộ để tăng dung lượng pool bộ đệm.

### `AdjustBufferAllocationBasedOnUsage()`
- **Description**: Điều chỉnh bộ đệm theo mức sử dụng. Nếu bộ đệm không được sử dụng đầy đủ, nó sẽ giảm kích thước pool.
- **Returns**: Không có giá trị trả về.

### `ShrinkBufferPoolSize(int bufferSize)`
- **Description**: Giảm kích thước pool bộ đệm.
- **Parameters**:
  - `bufferSize`: Kích thước bộ đệm cần giảm.
- **Returns**: Không có giá trị trả về.

### `GetPoolInfo(int bufferSize, out int freeCount, out int totalBuffers, out int bufferSizeOut, out int misses)`
- **Description**: Lấy thông tin về pool của bộ đệm.
- **Parameters**:
  - `bufferSize`: Kích thước bộ đệm cần lấy thông tin.
  - `freeCount`: Số lượng bộ đệm còn lại trong pool.
  - `totalBuffers`: Tổng số bộ đệm trong pool.
  - `bufferSizeOut`: Kích thước của bộ đệm.
  - `misses`: Số lần thiếu hụt bộ đệm.
- **Returns**: Không có giá trị trả về.

### `Dispose()`
- **Description**: Dọn dẹp các pool bộ đệm và giải phóng tài nguyên.
- **Returns**: Không có giá trị trả về.

---

## **Giải thích về MultiSizeBuffer**
Lớp `MultiSizeBuffer` quản lý các pool bộ đệm với kích thước khác nhau, giúp việc phân bổ và tái sử dụng bộ đệm hiệu quả hơn. Lớp này cho phép ứng dụng quản lý tài nguyên bộ nhớ tốt hơn thông qua các phương thức để thuê và trả bộ đệm, điều chỉnh phân bổ bộ đệm và quản lý dung lượng của các pool bộ đệm.

### **Tính năng**
- **Rent and Return Buffers**: Bạn có thể thuê bộ đệm với kích thước yêu cầu và trả lại sau khi sử dụng.
- **Buffer Pool Management**: Lớp này tự động tăng hoặc giảm kích thước của pool bộ đệm tùy thuộc vào mức độ sử dụng.
- **Flexible Allocation Adjustment**: Bạn có thể điều chỉnh tỷ lệ phân bổ bộ đệm và phân bổ lại bộ đệm khi tài nguyên thay đổi.

### **Lợi ích**
- **Memory Savings**: Thay vì tạo mới bộ đệm mỗi khi cần, lớp này tái sử dụng bộ đệm từ các pool.
- **High Performance**: Việc phân bổ lại bộ đệm khi cần và điều chỉnh dung lượng pool giúp tối ưu hiệu suất.
- **Easy Resource Management**: Các phương thức như `IncreaseBufferPoolSize` và `AdjustBufferAllocationBasedOnUsage` giúp bạn dễ dàng điều chỉnh bộ đệm theo nhu cầu ứng dụng.

---

## **Cách sử dụng MultiSizeBuffer**

```csharp
// Khởi tạo và phân bổ bộ đệm
var bufferManager = new MultiSizeBuffer();
bufferManager.AllocateBuffers();

// Thuê bộ đệm với kích thước 1024 byte
byte[] buffer = bufferManager.RentBuffer(1024);

// Trả lại bộ đệm về pool
bufferManager.ReturnBuffer(buffer);

// Điều chỉnh tỷ lệ phân bổ bộ đệm
await bufferManager.AdjustBufferAllocationAsync(1024, 0.2);

// Lấy thông tin về pool bộ đệm
bufferManager.GetPoolInfo(1024, out int freeCount, out int totalBuffers, out int bufferSizeOut, out int misses);
```