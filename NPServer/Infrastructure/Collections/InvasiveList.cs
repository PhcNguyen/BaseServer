using System;
using System.Collections;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Collections;

/// <summary>
/// Danh sách xâm nhập quản lý các phần tử có thể được lặp qua và có thể truy cập thông qua các chỉ số xâm nhập.
/// </summary>
public class InvasiveList<T>
{
    public int Id { get; private set; }
    public T? Head { get; set; }
    public T? Tail { get; private set; }
    public int Count { get; private set; }

    private readonly Iterator?[] _iterators;
    private int _numIterators;
    private readonly int _maxIterators;

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="InvasiveList{T}"/> với số lượng tối đa của các iterator.
    /// </summary>
    /// <param name="maxIterators">Số lượng tối đa của các iterator.</param>
    public InvasiveList(int maxIterators)
    {
        _maxIterators = maxIterators;
        _iterators = new Iterator[_maxIterators];
    }

    /// <summary>
    /// Khởi tạo một phiên bản mới của <see cref="InvasiveList{T}"/> với số lượng tối đa của các iterator và một ID.
    /// </summary>
    /// <param name="maxIterators">Số lượng tối đa của các iterator.</param>
    /// <param name="id">ID của danh sách.</param>
    public InvasiveList(int maxIterators, int id)
    {
        _maxIterators = maxIterators;
        _iterators = new Iterator[_maxIterators];
        Id = id;
    }

    /// <summary>
    /// Lặp qua các phần tử trong danh sách.
    /// </summary>
    /// <returns>Một đối tượng IEnumerable cho phép lặp qua các phần tử trong danh sách.</returns>
    public IEnumerable<T?> Iterate()
    {
        var iterator = new Iterator(this);

        try
        {
            while (!iterator.End())
            {
                var element = iterator.Current;
                iterator.MoveNext();
                yield return element;
            }
        }
        finally
        {
            UnregisterIterator(iterator);
        }
    }

    /// <summary>
    /// Kiểm tra danh sách có trống hay không.
    /// </summary>
    /// <returns>True nếu danh sách trống; ngược lại, false.</returns>
    public bool IsEmpty() => Head == null;

    /// <summary>
    /// Xóa một phần tử khỏi danh sách.
    /// </summary>
    /// <param name="element">Phần tử cần xóa.</param>
    public void Remove(T? element)
    {
        if (element == null || !Contains(element)) return;

        var node = GetInvasiveListNode(element, Id);
        if (node == null) return;

        for (int i = 0; i < _numIterators; i++)
        {
            Iterator iterator = _iterators[i]!;
            if (element.Equals(iterator.Current))
            {
                iterator.SkipNext = false;
                iterator.MoveNext();
                iterator.SkipNext = true;
            }
        }

        if (node.Next != null)
        {
            T? nextElement = node.Next;
            var nextNode = GetInvasiveListNode(nextElement, Id);
            if (nextNode != null)
                nextNode.Prev = node.Prev;
        }

        if (node.Prev != null)
        {
            T? prevElement = node.Prev;
            var prevNode = GetInvasiveListNode(prevElement, Id);
            if (prevNode != null)
                prevNode.Next = node.Next;
        }

        if (Head != null && Head.Equals(element)) Head = node.Next;
        if (Tail != null && Tail.Equals(element)) Tail = node.Prev;
        node.Clear();

        if (Count > 0) Count--;
    }

    /// <summary>
    /// Thêm một phần tử vào cuối danh sách.
    /// </summary>
    /// <param name="element">Phần tử cần thêm.</param>
    public void AddBack(T? element)
    {
        if (element == null || Contains(element)) return;

        var node = GetInvasiveListNode(element, Id);
        if (node == null) return;

        node.Prev = Tail;
        if (Tail != null)
        {
            var tailNode = GetInvasiveListNode(Tail, Id);
            if (tailNode != null)
                tailNode.Next = element;
        }
        else
            Head = element;

        Tail = element;
        Count++;
    }

    /// <summary>
    /// Trả về nút xâm nhập của một phần tử.
    /// </summary>
    /// <param name="element">Phần tử cần tìm nút xâm nhập.</param>
    /// <param name="listId">ID của danh sách.</param>
    /// <returns>Nút xâm nhập của phần tử.</returns>
    public virtual InvasiveListNode<T?>? GetInvasiveListNode(T element, int listId) => null;

    /// <summary>
    /// Kiểm tra danh sách có chứa phần tử hay không.
    /// </summary>
    /// <param name="element">Phần tử cần kiểm tra.</param>
    /// <returns>True nếu danh sách chứa phần tử; ngược lại, false.</returns>
    public bool Contains(T? element)
    {
        if (element == null) return false;
        var node = GetInvasiveListNode(element, Id);
        if (node == null) return false;
        return node.Next != null || node.Prev != null || element.Equals(Head);
    }

    private void RegisterIterator(Iterator iterator)
    {
        if (_numIterators >= _maxIterators)
            throw new InvalidOperationException($"Số lượng iterator quá nhiều '{_maxIterators}' cho danh sách xâm nhập");

        _iterators[_numIterators++] = iterator;
    }

    private void UnregisterIterator(Iterator iterator)
    {
        for (int i = 0; i < _numIterators; i++)
        {
            if (_iterators[i] == iterator)
            {
                if (_numIterators > 1 && i != _numIterators - 1)
                    _iterators[i] = _iterators[_numIterators - 1];

                _iterators[_numIterators - 1] = null;
                _numIterators--;
                return;
            }
        }

        throw new InvalidOperationException("Iterator không tìm thấy trong bộ sưu tập iterator của danh sách xâm nhập!");
    }

    /// <summary>
    /// Iterator cho phép lặp qua các phần tử trong danh sách xâm nhập.
    /// </summary>
    public class Iterator : IEnumerator<T?>
    {
        private readonly InvasiveList<T> _list;
        public bool SkipNext { get; set; }

        /// <summary>
        /// Khởi tạo một phiên bản mới của Iterator với danh sách xâm nhập.
        /// </summary>
        /// <param name="invasiveList">Danh sách xâm nhập.</param>
        public Iterator(InvasiveList<T> invasiveList)
        {
            _list = invasiveList;
            Current = _list.Head;
            SkipNext = false;
            _list.RegisterIterator(this);
        }

        public T? Current { get; private set; }
        object? IEnumerator.Current => Current;
        public void Dispose() { }
        public void Reset() { }

        public bool MoveNext()
        {
            if (SkipNext)
            {
                SkipNext = false;
            }
            else if (Current != null)
            {
                var node = _list.GetInvasiveListNode(Current, _list.Id);
                if (node != null)
                {
                    Current = node.Next;
                }
            }

            return Current != null;
        }

        /// <summary>
        /// Kiểm tra iterator đã đến cuối danh sách hay chưa.
        /// </summary>
        /// <returns>True nếu đã đến cuối; ngược lại, false.</returns>
        public bool End() => Current == null;
    }
}

/// <summary>
/// Nút của danh sách xâm nhập lưu trữ các tham chiếu tới phần tử tiếp theo và phần tử trước đó.
/// </summary>
public class InvasiveListNode<T>
{
    public T? Next { get; set; }
    public T? Prev { get; set; }

    /// <summary>
    /// Xóa sạch các tham chiếu của nút.
    /// </summary>
    public void Clear() => Next = Prev = default;
}

/// <summary>
/// Bộ sưu tập các nút của danh sách xâm nhập.
/// </summary>
public class InvasiveListNodeCollection<T>
{
    private readonly InvasiveListNode<T>[] _nodes;
    private readonly int _numLists;

    /// <summary>
    /// Khởi tạo một bộ sưu tập mới của các nút danh sách xâm nhập với số lượng danh sách cụ thể.
    /// </summary>
    /// <param name="numLists">Số lượng danh sách.</param>
    public InvasiveListNodeCollection(int numLists)
    {
        _numLists = numLists;
        _nodes = new InvasiveListNode<T>[_numLists];
        for (int i = 0; i < _numLists; i++)
            _nodes[i] = new InvasiveListNode<T>();
    }

    /// <summary>
    /// Trả về nút xâm nhập tại chỉ số danh sách được chỉ định.
    /// </summary>
    /// <param name="listIndex">Chỉ số danh sách.</param>
    /// <returns>Nút xâm nhập tại chỉ số danh sách được chỉ định.</returns>
    public InvasiveListNode<T?>? GetInvasiveListNode(int listIndex)
    {
        if (listIndex >= 0 && listIndex < _numLists)
            return _nodes[listIndex]!;
        else
            return null;
    }
}