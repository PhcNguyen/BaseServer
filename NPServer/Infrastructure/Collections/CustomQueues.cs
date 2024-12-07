using System;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Collections;

/// <summary>
/// Hàng đợi ưu tiên cố định với khả năng thêm phần tử và duy trì thứ tự ưu tiên.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử, phải triển khai <see cref="IComparable{T}"/>.</typeparam>
/// <remarks>
/// Khởi tạo một đối tượng <see cref="FixedPriorityQueue{T}"/> với dung lượng ban đầu được chỉ định.
/// </remarks>
/// <param name="capacity">Dung lượng ban đầu của hàng đợi.</param>
public class FixedPriorityQueue<T>(int capacity) where T : IComparable<T>
{
    private readonly List<T> _items = new(capacity);

    /// <summary>
    /// Kiểm tra xem hàng đợi có trống hay không.
    /// </summary>
    public bool Empty => _items.Count == 0;

    /// <summary>
    /// Lấy số lượng phần tử trong hàng đợi.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Lấy phần tử ở đầu hàng đợi.
    /// </summary>
    public T Top => _items.Count > 0 ? _items[0] : throw new InvalidOperationException("Queue is empty");

    /// <summary>
    /// Thêm một phần tử vào hàng đợi.
    /// </summary>
    /// <param name="value">Phần tử cần thêm.</param>
    public void Push(T value)
    {
        _items.Add(value);
        HeapFunctions.PushHeap(_items);
    }

    /// <summary>
    /// Loại bỏ phần tử ở đầu hàng đợi.
    /// </summary>
    public void Pop()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        HeapFunctions.PopHeap(_items);
        _items.RemoveAt(_items.Count - 1);
    }

    /// <summary>
    /// Xóa tất cả các phần tử trong hàng đợi.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Chuyển đổi danh sách thành một heap hợp lệ.
    /// </summary>
    public void Heapify()
    {
        HeapFunctions.MakeHeap(_items);
    }
}

/// <summary>
/// Các hàm hỗ trợ thao tác với heap.
/// </summary>
public static class HeapFunctions
{
    /// <summary>
    /// Thêm một phần tử vào heap.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của phần tử, phải triển khai <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="list">Danh sách chứa các phần tử.</param>
    public static void PushHeap<T>(List<T> list) where T : IComparable<T>
    {
        int last = list.Count - 1;
        int holeIndex = last;
        T value = list[last];
        PushHeapIndex(list, holeIndex, 0, value);
    }

    private static void PushHeapIndex<T>(List<T> list, int holeIndex, int topIndex, T value) where T : IComparable<T>
    {
        int parent = (holeIndex - 1) / 2;
        while (holeIndex > topIndex && list[parent].CompareTo(value) < 0)
        {
            list[holeIndex] = list[parent];
            holeIndex = parent;
            parent = (holeIndex - 1) / 2;
        }
        list[holeIndex] = value;
    }

    /// <summary>
    /// Loại bỏ phần tử ở đầu heap.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của phần tử, phải triển khai <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="list">Danh sách chứa các phần tử.</param>
    public static void PopHeap<T>(List<T> list) where T : IComparable<T>
    {
        if (list.Count > 1)
        {
            int last = list.Count - 1;
            T value = list[last];
            list[last] = list[0];
            AdjustHeap(list, 0, last, value);
        }
    }

    /// <summary>
    /// Chuyển đổi danh sách thành một heap hợp lệ.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của phần tử, phải triển khai <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="list">Danh sách chứa các phần tử.</param>
    public static void MakeHeap<T>(List<T> list) where T : IComparable<T>
    {
        if (list.Count < 2) return;

        int len = list.Count;
        int parent = (len - 2) / 2;
        while (true)
        {
            T value = list[parent];
            AdjustHeap(list, parent, len, value);
            if (parent == 0) return;
            parent--;
        }
    }

    private static void AdjustHeap<T>(List<T> list, int holeIndex, int len, T value) where T : IComparable<T>
    {
        int topIndex = holeIndex;
        int secondChild = 2 * (holeIndex + 1);
        while (secondChild < len)
        {
            if (list[secondChild].CompareTo(list[secondChild - 1]) < 0)
                secondChild--;
            list[holeIndex] = list[secondChild];
            holeIndex = secondChild;
            secondChild = 2 * (secondChild + 1);
        }
        if (secondChild == len)
        {
            list[holeIndex] = list[secondChild - 1];
            holeIndex = secondChild - 1;
        }

        PushHeapIndex(list, holeIndex, topIndex, value);
    }
}

/// <summary>
/// Hàng đợi kép cố định.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử.</typeparam>
public class FixedDeque<T>
{
    private readonly T[] _items;
    private int _first;
    private int _last;
    private readonly int _maxSize;

    /// <summary>
    /// Khởi tạo một đối tượng <see cref="FixedDeque{T}"/> với kích thước được chỉ định.
    /// </summary>
    /// <param name="size">Kích thước của hàng đợi kép.</param>
    public FixedDeque(int size)
    {
        _maxSize = size + 1;
        _items = new T[_maxSize];
        _first = 0;
        _last = 0;
    }

    /// <summary>
    /// Lấy dung lượng của hàng đợi kép.
    /// </summary>
    public int Capacity => _maxSize - 1;

    /// <summary>
    /// Lấy số lượng phần tử trong hàng đợi kép.
    /// </summary>
    public int Size => (_last - _first + _maxSize) % _maxSize;

    /// <summary>
    /// Kiểm tra xem hàng đợi kép có trống hay không.
    /// </summary>
    public bool Empty => _first == _last;

    /// <summary>
    /// Xóa tất cả các phần tử trong hàng đợi kép.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_items, 0, _items.Length);
        _first = _last = 0;
    }

    /// <summary>
    /// Truy cập và thay đổi phần tử tại vị trí chỉ định.
    /// </summary>
    /// <param name="index">Vị trí của phần tử.</param>
    /// <returns>Phần tử tại vị trí chỉ định.</returns>
    public T this[int index]
    {
        get
        {
            if (index >= Size)
                throw new IndexOutOfRangeException();
            return _items[(_first + index) % _maxSize];
        }
        set
        {
            if (index >= Size)
                throw new IndexOutOfRangeException();
            _items[(_first + index) % _maxSize] = value;
        }
    }

    /// <summary>
    /// Lấy phần tử ở đầu hàng đợi kép.
    /// </summary>
    public T Front => !Empty ? _items[_first] : throw new InvalidOperationException("Deque is empty");

    /// <summary>
    /// Lấy phần tử ở cuối hàng đợi kép.
    /// </summary>
    public T Back => !Empty ? _items[(_last + _maxSize - 1) % _maxSize] : throw new InvalidOperationException("Deque is empty");

    /// <summary>
    /// Thêm phần tử vào cuối hàng đợi kép.
    /// </summary>
    /// <param name="value">Phần tử cần thêm.</param>
    public void PushBack(T value)
    {
        _items[_last] = value;
        _last = (_last + 1) % _maxSize;
    }

    /// <summary>
    /// Thêm phần tử vào đầu hàng đợi kép.
    /// </summary>
    /// <param name="value">Phần tử cần thêm.</param>
    public void PushFront(T value)
    {
        _first = (_first + _maxSize - 1) % _maxSize;
        _items[_first] = value;
    }

    /// <summary>
    /// Loại bỏ và trả về phần tử cuối của hàng đợi kép.
    /// </summary>
    /// <returns>Phần tử cuối của hàng đợi kép.</returns>
    public T PopBack()
    {
        if (Empty)
            throw new InvalidOperationException("Deque is empty");

        _last = (_last + _maxSize - 1) % _maxSize;
        T result = _items[_last];
        _items[_last] = default!;
        return result;
    }

    /// <summary>
    /// Cố gắng loại bỏ và trả về phần tử cuối của hàng đợi kép.
    /// </summary>
    /// <param name="value">Phần tử cuối của hàng đợi kép nếu thành công.</param>
    /// <returns>True nếu thành công, ngược lại là false.</returns>
    public bool TryPopBack(out T value)
    {
        if (Empty)
        {
            value = default!;
            return false;
        }

        _last = (_last + _maxSize - 1) % _maxSize;
        value = _items[_last];
        _items[_last] = default!;
        return true;
    }

    /// <summary>
    /// Loại bỏ và trả về phần tử đầu của hàng đợi kép.
    /// </summary>
    /// <returns>Phần tử đầu của hàng đợi kép.</returns>
    public T PopFront()
    {
        if (Empty)
            throw new InvalidOperationException("Deque is empty");

        T result = _items[_first];
        _items[_first] = default!;
        _first = (_first + 1) % _maxSize;
        return result;
    }

    /// <summary>
    /// Cố gắng loại bỏ và trả về phần tử đầu của hàng đợi kép.
    /// </summary>
    /// <param name="value">Phần tử đầu của hàng đợi kép nếu thành công.</param>
    /// <returns>True nếu thành công, ngược lại là false.</returns>
    public bool TryPopFront(out T value)
    {
        if (Empty)
        {
            value = default!;
            return false;
        }

        value = _items[_first];
        _items[_first] = default!;
        _first = (_first + 1) % _maxSize;
        return true;
    }
}