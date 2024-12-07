﻿using System.Collections.Generic;

namespace NPServer.Infrastructure.Services.Random
{
    /// <summary>
    /// Lớp hỗ trợ sinh số ngẫu nhiên với nhiều kiểu dữ liệu và phạm vi khác nhau.
    /// </summary>
    public class GRandom
    {
        public const int RandMax = 0x7fffffff; // Giá trị lớn nhất có thể sinh ra

        private int _seed;         // Hạt giống cho bộ sinh số ngẫu nhiên
        private readonly Rand _rand; // Bộ xử lý sinh số ngẫu nhiên

        public GRandom()
        {
            _seed = 0;
            _rand = new Rand(0);
        }

        // Khởi tạo với hạt giống do người dùng cung cấp
        public GRandom(int seed)
        {
            _seed = seed;
            _rand = new Rand((uint)seed);
        }

        /// <summary>
        /// Đặt lại hạt giống cho bộ sinh số ngẫu nhiên.
        /// </summary>
        public void Seed(int seed)
        {
            _seed = seed;
            _rand.SetSeed((uint)seed);
        }

        /// <summary>
        /// Lấy giá trị hạt giống hiện tại.
        /// </summary>
        public int GetSeed()
        {
            return _seed;
        }

        /// <summary>
        /// Sinh số nguyên ngẫu nhiên trong khoảng [0, RandMax].
        /// </summary>
        public int Next()
        {
            return Next(RandMax);
        }

        /// <summary>
        /// Sinh số nguyên ngẫu nhiên trong khoảng [0, max).
        /// </summary>
        public int Next(int max)
        {
            return Next(0, max);
        }

        /// <summary>
        /// Sinh số nguyên ngẫu nhiên trong khoảng [min, max).
        /// </summary>
        public int Next(int min, int max)
        {
            if (min == max)
                return min;
            int range = max - min;
            return (int)(_rand.Get() & RandMax) % range + min;
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên trong khoảng [0.0f, 1.0f].
        /// </summary>
        public float NextFloat()
        {
            return _rand.GetFloat();
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên trong khoảng [0.0f, max).
        /// </summary>
        public float NextFloat(float max)
        {
            return _rand.Get(0.0f, max);
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên trong khoảng [min, max).
        /// </summary>
        public float NextFloat(float min, float max)
        {
            return _rand.Get(min, max);
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên (double) trong khoảng [0.0, 1.0].
        /// </summary>
        public double NextDouble()
        {
            return _rand.GetDouble();
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên (double) trong khoảng [0.0, max).
        /// </summary>
        public double NextDouble(double max)
        {
            return _rand.Get(0.0, max);
        }

        /// <summary>
        /// Sinh số thực ngẫu nhiên (double) trong khoảng [min, max).
        /// </summary>
        public double NextDouble(double min, double max)
        {
            return _rand.Get(min, max);
        }

        /// <summary>
        /// Kiểm tra ngẫu nhiên với tỷ lệ phần trăm (percent).
        /// </summary>
        public bool NextPct(int pct)
        {
            return Next(0, 100) < pct;
        }

        /// <summary>
        /// Trộn ngẫu nhiên một danh sách.
        /// </summary>
        public void ShuffleList<T>(List<T> list)
        {
            if (list.Count > 1)
                for (int i = 0; i < list.Count; i++)
                {
                    int j = Next(i, list.Count);
                    (list[j], list[i]) = (list[i], list[j]);
                }
        }

        /// <summary>
        /// Trả về chuỗi đại diện cho trạng thái của bộ sinh số ngẫu nhiên.
        /// </summary>
        public override string ToString()
        {
            return _rand.ToString();
        }
    }
}
