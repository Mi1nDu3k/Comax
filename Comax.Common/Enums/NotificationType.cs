using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Common.Enums
{
    public enum NotificationType
    {
        System = 1,      // Thông báo hệ thống (VD: Đăng nhập lạ, bảo trì)
        Account = 2,     // Tài khoản (VD: Lên VIP, bị Ban)
        Comic = 3,       // Truyện (VD: Có chap mới)
        Interaction = 4  // Tương tác (VD: Có người trả lời bình luận)
    }
}
