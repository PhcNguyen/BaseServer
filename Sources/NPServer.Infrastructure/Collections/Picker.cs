using NPServer.Shared.Random;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Collections;

/// <summary>
/// Bộ chọn ngẫu nhiên các phần tử từ một bộ sưu tập, hỗ trợ các chế độ có trọng số và không trọng số.
/// </summary>
public sealed class Picker<T>
{
    /// <summary>
    /// Đại diện cho một phần tử với trọng số dùng để chọn ngẫu nhiên.
    /// </summary>
    /// <remarks>
    /// Khởi tạo một phiên bản mới của lớp <see cref="WeightedElement"/>.
    /// </remarks>
    /// <param name="element">Phần tử cần lưu trữ.</param>
    /// <param name="weight">Trọng số của phần tử.</param>
    public class WeightedElement(T? element, int weight)
    {
        public T? Element { get; } = element;
        public int Weight { get; } = weight;
    }

    /// <summary>
    /// Định nghĩa các chế độ trọng số cho việc chọn phần tử.
    /// </summary>
    private enum WeightMode
    {
        Invalid,
        Weighted,
        UnWeighted
    }

    private readonly List<WeightedElement> _elements;
    private readonly GRandom _random;
    private WeightMode _weightMode;
    private int _weights;

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp <see cref="Picker{T}"/> với bộ sinh số ngẫu nhiên được chỉ định.
    /// </summary>
    /// <param name="random">Bộ sinh số ngẫu nhiên để chọn các phần tử.</param>
    public Picker(GRandom random)
    {
        _elements = [];
        _random = random;
        _weightMode = WeightMode.Invalid;
        _weights = 0;
    }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp <see cref="Picker{T}"/> bằng cách sao chép một phiên bản <see cref="Picker{T}"/> khác.
    /// </summary>
    /// <param name="other">Phiên bản khác của <see cref="Picker{T}"/> để sao chép.</param>
    public Picker(Picker<T> other)
    {
        _elements = new List<WeightedElement>(other._elements);
        _random = new GRandom(other._random.GetSeed());
        _weightMode = other._weightMode;
        _weights = other._weights;
    }

    /// <summary>
    /// Thêm một phần tử vào bộ chọn với trọng số mặc định là 1.
    /// </summary>
    /// <param name="element">Phần tử cần thêm.</param>
    public void Add(T? element)
    {
        if (_weightMode == WeightMode.Invalid)
            _weightMode = WeightMode.UnWeighted;

        if (_weightMode == WeightMode.UnWeighted)
        {
            _elements.Add(new WeightedElement(element, 1));
            _weights += 1;
        }
    }

    /// <summary>
    /// Thêm một phần tử vào bộ chọn với trọng số được chỉ định.
    /// </summary>
    /// <param name="element">Phần tử cần thêm.</param>
    /// <param name="weight">Trọng số của phần tử.</param>
    public void Add(T? element, int weight)
    {
        if (_weightMode == WeightMode.Invalid)
            _weightMode = WeightMode.Weighted;

        if (_weightMode == WeightMode.Weighted && weight > 0)
        {
            _elements.Add(new WeightedElement(element, weight));
            _weights += weight;
        }
    }

    /// <summary>
    /// Kiểm tra xem bộ chọn có trống hay không.
    /// </summary>
    /// <returns>True nếu bộ chọn không có phần tử; ngược lại, false.</returns>
    public bool Empty() => _elements.Count == 0;

    /// <summary>
    /// Lấy số lượng phần tử trong bộ chọn.
    /// </summary>
    /// <returns>Số lượng phần tử trong bộ chọn.</returns>
    public int GetNumElements() => _elements.Count;

    /// <summary>
    /// Lấy một chỉ số ngẫu nhiên dựa trên chế độ chọn hiện tại (có trọng số hoặc không trọng số).
    /// </summary>
    /// <returns>Một chỉ số ngẫu nhiên của danh sách phần tử.</returns>
    public int GetRandomIndex() => _weightMode == WeightMode.UnWeighted ? GetRandomIndexUnweighted() : GetRandomIndexWeighted();

    /// <summary>
    /// Lấy một chỉ số ngẫu nhiên cho chế độ không trọng số.
    /// </summary>
    /// <returns>Một chỉ số ngẫu nhiên trong phạm vi số lượng phần tử.</returns>
    public int GetRandomIndexUnweighted()
    {
        return _random.Next(0, _elements.Count);
    }

    /// <summary>
    /// Lấy một chỉ số ngẫu nhiên dựa trên trọng số của các phần tử.
    /// </summary>
    /// <returns>Một chỉ số ngẫu nhiên, được trọng số bởi trọng số của phần tử.</returns>
    public int GetRandomIndexWeighted()
    {
        int r = _random.Next(1, _weights + 1);
        int sum = 0;
        int index = 0;

        foreach (var element in _elements)
        {
            sum += element.Weight;
            if (sum >= r) break;
            index++;
        }

        return index;
    }

    /// <summary>
    /// Lấy phần tử tại một chỉ số cụ thể.
    /// </summary>
    /// <param name="index">Chỉ số của phần tử cần lấy.</param>
    /// <param name="element">Phần tử tại chỉ số cụ thể.</param>
    /// <returns>True nếu phần tử được tìm thấy; ngược lại, false.</returns>
    public bool GetElementAt(int index, out T? element)
    {
        element = default;

        if (index >= 0 && index < _elements.Count)
        {
            element = _elements[index].Element;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Chọn một phần tử ngẫu nhiên.
    /// </summary>
    /// <returns>Phần tử được chọn.</returns>
    public T? Pick()
    {
        if (Empty()) return default;
        return _elements[GetRandomIndex()].Element;
    }

    /// <summary>
    /// Chọn một phần tử ngẫu nhiên và trả về nó.
    /// </summary>
    /// <param name="element">Phần tử được chọn.</param>
    /// <returns>True nếu một phần tử được chọn; ngược lại, false.</returns>
    public bool Pick(out T? element)
    {
        if (Empty())
        {
            element = default;
            return false;
        }

        element = _elements[GetRandomIndex()].Element;
        return true;
    }

    /// <summary>
    /// Chọn một phần tử ngẫu nhiên và trả về cả phần tử và chỉ số của nó.
    /// </summary>
    /// <param name="element">Phần tử được chọn.</param>
    /// <param name="index">Chỉ số của phần tử được chọn.</param>
    /// <returns>True nếu một phần tử được chọn; ngược lại, false.</returns>
    public bool Pick(out T? element, out int index)
    {
        if (Empty())
        {
            element = default;
            index = -1;
            return false;
        }

        index = GetRandomIndex();
        element = _elements[index].Element;

        return true;
    }

    /// <summary>
    /// Chọn một phần tử ngẫu nhiên và loại bỏ nó khỏi bộ chọn.
    /// </summary>
    /// <param name="element">Phần tử được chọn và loại bỏ.</param>
    /// <returns>True nếu một phần tử được chọn và loại bỏ; ngược lại, false.</returns>
    public bool PickRemove(out T? element)
    {
        if (Empty())
        {
            element = default;
            return false;
        }

        int index = GetRandomIndex();

        element = _elements[index].Element;
        _weights -= _elements[index].Weight;

        _elements[index] = _elements[^1];  // Swap với phần tử cuối cùng
        _elements.RemoveAt(_elements.Count - 1);

        return true;
    }

    /// <summary>
    /// Loại bỏ phần tử tại chỉ số được chỉ định.
    /// </summary>
    /// <param name="index">Chỉ số của phần tử cần loại bỏ.</param>
    /// <returns>True nếu phần tử được loại bỏ; ngược lại, false.</returns>
    public bool RemoveIndex(int index)
    {
        if (Empty()) return false;

        if (index >= 0 && index < _elements.Count)
        {
            _weights -= _elements[index].Weight;
            _elements.RemoveAt(index);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Xóa tất cả các phần tử khỏi bộ chọn.
    /// </summary>
    public void Clear()
    {
        _elements.Clear();
        _weights = 0;
    }
}