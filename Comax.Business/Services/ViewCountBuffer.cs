using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Comax.Business.Services
{
    // Interface để inject vào Service
    public interface IViewCountBuffer
    {
        void Increment(int comicId);
        Dictionary<int, int> PopAll();
    }

    public class ViewCountBuffer : IViewCountBuffer
    {
        // Sử dụng ConcurrentDictionary để đảm bảo Thread-Safe (an toàn đa luồng)
        private ConcurrentDictionary<int, int> _buffer = new();

        public void Increment(int comicId)
        {
            // Tăng giá trị view cho comicId, nếu chưa có thì khởi tạo là 1
            _buffer.AddOrUpdate(comicId, 1, (key, oldValue) => oldValue + 1);
        }

        // Hàm này sẽ được Background Service gọi để lấy dữ liệu và xóa bộ đệm cũ
        public Dictionary<int, int> PopAll()
        {
            if (_buffer.IsEmpty) return new Dictionary<int, int>();

            // Kỹ thuật Swap: Tạo buffer mới và thay thế buffer cũ ngay lập tức
            // để không làm gián đoạn các request đang đến.
            var newBuffer = new ConcurrentDictionary<int, int>();
            var oldBuffer = Interlocked.Exchange(ref _buffer, newBuffer);

            return oldBuffer.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}