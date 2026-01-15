namespace Comax.Common.Constants
{
    public static class SystemMessages
    {
   
        public static class Common
        {
            public const string NotFound = "Không tìm thấy dữ liệu yêu cầu.";
            public const string Unauthorized = "Bạn không có quyền thực hiện thao tác này.";
            public const string TokenInvalid = "Token không hợp lệ hoặc đã hết hạn.";
            public const string UserClaimNotFound = "Token hợp lệ nhưng không tìm thấy thông tin định danh người dùng.";
            public const string ServerError = "Lỗi hệ thống nội bộ.";
            public const string InvalidInput = "Dữ liệu đầu vào không hợp lệ.";
            public const string DbSaveError = "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu.";
            public const string MinioNotInit = "Minio Client chưa được khởi tạo.";
            public const string UserNotFound = "Không tìm thấy người dùng."; // Dùng chung cho nhiều chỗ
            public const string ActionFailed = "Thao tác thất bại.";

        }
        public static class Auth
        {
            public const string EmailCheckRequired = "Vui lòng kiểm tra email để lấy mã xác thực.";
            public const string EmailExists = "Email đã tồn tại.";
            public const string RegisterSuccess = "Đăng ký thành công.";
            public const string UserNotFound = "Người dùng không tồn tại.";
            public const string LoginFailed = "Email hoặc mật khẩu không đúng.";
            public const string Banned = "Tài khoản đã bị khóa.";
            public const string Unbanned = "Đã mở khóa tài khoản.";
            public const string VipUpgraded = "Đã nâng cấp VIP thành công.";
            public const string VipDowngraded = "Đã hủy gói VIP.";
            public const string ForgotPasswordSent = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi.";
            public const string ResetPasswordSuccess = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.";
            public const string OtpInvalid = "Mã xác thực không đúng hoặc đã hết hạn.";
            public const string OtpValid = "Mã xác thực hợp lệ.";
        }

        public static class Comic
        {
            public const string NotFound = "Không tìm thấy truyện hoặc truyện đã bị xóa.";
            public const string SlugRequired = "Vui lòng cung cấp đường dẫn tĩnh (slug).";
            public const string NotFoundBySlug = "Không tìm thấy truyện với slug: {0}";
            public const string NotFoundInTrash = "Không tìm thấy truyện trong thùng rác.";
            public const string RestoreSuccess = "Khôi phục truyện thành công.";
            public const string PurgeSuccess = "Đã xóa vĩnh viễn truyện và các dữ liệu liên quan.";
            public const string PremiumContent = "Nội dung này chỉ dành cho tài khoản VIP.";
            public const string ConcurrencyConflict = "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang.";
        }

        public static class Subscription
        {
            // {0} sẽ là ngày hết hạn
            public const string VipUpgradeSuccess = "Chúc mừng! Bạn đã nâng cấp VIP thành công. Hạn dùng đến: {0}";
            public const string InvalidMonths = "Số tháng không hợp lệ.";
            public const string ExtendSuccess = "Gia hạn VIP thành công!";
            public const string ExtendFailed = "Không thể thực hiện gia hạn.";
        }
        public static class Chapter
        {
            public const string NotFound = "Không tìm thấy chương truyện.";
            public const string ImageRequired = "Vui lòng chọn ít nhất 1 ảnh để tạo chương.";
            public const string SlugExists = "Slug '{0}' đã tồn tại trong truyện này.";
            public const string TitleExists = "Chương '{0}' đã tồn tại!";
        }

     
        public static class Payment
        {
            public const string InvalidToken = "SePay Webhook: Sai Token bảo mật.";
            public const string IgnoredTransaction = "Bỏ qua giao dịch chi tiền (Outgoing transaction).";
            public const string Success = "Xử lý thanh toán thành công.";
           
            public const string LogReceivedMoney = "Nhận được {0} VND từ nội dung: {1}";
            public const string LogVipActivated = "Đã kích hoạt VIP cho User ID: {0}";
            public const string LogUserIdNotFound = "Không tìm thấy User ID trong nội dung chuyển khoản: {0}";
        }

        public static class Favorite
        {
            public const string Added = "Đã thêm vào danh sách yêu thích.";
            public const string Removed = "Đã bỏ yêu thích.";
            public const string Deleted = "Đã xóa khỏi danh sách yêu thích.";
        }

      
        public static class Comment
        {
            public const string LoginRequired = "Bạn cần đăng nhập để bình luận.";
            public const string NotFound = "Không tìm thấy bình luận.";
        }

       
        public static class Rating
        {
            public const string Success = "Đánh giá truyện thành công!";
        }

     
        public static class History
        {
            public const string SaveSuccess = "Đã lưu lịch sử đọc truyện.";
            public const string DeleteSuccess = "Đã xóa lịch sử đọc.";
            public const string DeleteAllSuccess = "Đã xóa toàn bộ lịch sử đọc truyện.";
        }
        public static class Notification
        {
            public const string NewChapter = "Truyện '{0}' vừa có chương mới: {1}";
            public const string CommentReply = "{0} đã trả lời bình luận của bạn.";
        }


        public static class Email
        {
            public const string OtpSubject = "[Comax] Mã xác thực của bạn";
        }
    }
}