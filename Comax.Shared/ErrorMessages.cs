namespace Comax.Shared
{
    public static class ErrorMessages
    {
        /// <summary>
        /// 1. Các thông báo liên quan đến xác thực (Auth)
        /// </summary>
        public static class Auth
        {
            public const string EmailExists = "Email đã tồn tại trong hệ thống.";
            public const string RegisterSuccess = "Đăng ký thành công!";
            public const string LoginSuccess = "Đăng nhập thành công!";
            public const string InvalidCredentials = "Tài khoản hoặc mật khẩu không đúng.";
            public const string AccountLocked = "Tài khoản đã bị khóa do vi phạm quy định.";
            public const string UserNotFound = "Không tìm thấy người dùng.";
            public const string VIPUpgradedSuccess = "Nâng cấp VIP thành công! Vui lòng đăng nhập lại.";
            public const string VIPDowngradedSuccess = "Đã hạ cấp tài khoản về thành viên thường.";
            public const string PremiumContent = "Đây là nội dung độc quyền cho VIP!";
            public const string Banned = "User đã bị cấm";
            public const string Unbanned = "User đã được mở khóa";
        }

        /// <summary>
        /// 2. Các thông báo liên quan đến lỗi hệ thống (System)
        /// </summary>
        public static class System
        {
            public const string RoleNotFound = "Lỗi hệ thống: Không tìm thấy Role yêu cầu.";
            public const string UserNotFound = "Lỗi hệ thống: Không tìm thấy User yêu cầu.";
            public const string DefaultRoleNotFound = "Lỗi hệ thống: Không tìm thấy Role mặc định.";
            public const string ConcurrencyConflict = "Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang.";
        }

        /// <summary>
        /// 3. Các thông báo dùng cho Validation (FluentValidation)
        /// </summary>
        public static class Validation
        {
            public const string UsernameRequired = "Tên đăng nhập không được để trống.";
            public const string UsernameMinLength = "Tên đăng nhập phải dài hơn {0} ký tự.";

            public const string EmailRequired = "Email không được để trống.";
            public const string EmailInvalid = "Định dạng email không hợp lệ.";

            public const string PasswordRequired = "Mật khẩu không được để trống.";
            public const string PasswordMinLength = "Mật khẩu phải có ít nhất {0} ký tự.";
            public const string PasswordUppercase = "Mật khẩu phải chứa ít nhất 1 ký tự viết hoa.";
            public const string PasswordLowercase = "Mật khẩu phải chứa ít nhất 1 ký tự viết thường.";
            public const string PasswordDigit = "Mật khẩu phải chứa ít nhất 1 chữ số.";
            public const string PasswordSpecialChar = "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (VD: ! @ # $ % ...)";

            public const string RoleInvalid = "Vui lòng chọn quyền hạn hợp lệ.";
        }
        public static class Comment
        {
            public const string ContentRequired = "Nội dung bình luận không được để trống.";
            public const string ContentTooLong = "Nội dung bình luận không được vượt quá {0} ký tự.";
            public const string ComicNotFound = "Không tìm thấy truyện với ID: {0}";
            public const string ParentCommentNotFound = "Không tìm thấy bình luận cha với ID: {0}";
            public const string HavingRepplyCommnet ="Đã trả lời bình luận của bạn";
            public const string CommentSuccess = "Bình luận của bạn đã được gửi";
        }
        public static class Comic
        {
            public const string SlugRequired = "Slug không được để trống.";
            public const string NotFoundBySlug = "Không tìm thấy truyện với slug: {0}";
            public const string ViewCountIncreased = "Đã tăng lượt xem thành công.";
        }

        public static class User
        {
            public const string FavoriteAdded = "Đã thêm truyện vào danh sách yêu thích.";
            public const string FavoriteRemoved = "Đã bỏ truyện khỏi danh sách yêu thích.";
            public const string AlreadyFavorited = "Truyện đã có trong danh sách yêu thích.";
            public const string NotFavorited = "Truyện không có trong danh sách yêu thích.";
            public const string UpgradeSuccess = "Chúc mừng bạn đã trở thành thành viên vip,các chức năng vip của bạn đã được mở khóa.";
            public const string DowngradeSuccess = "Tài khoản của bạn đã được hạ cấp về thành viên thường. Gia hạn vip nếu bạn muốn ";
            public const string InsufficientPrivileges = "Bạn không có quyền thực hiện hành động này.";
            public const string ProfileUpdated = "Cập nhật thông tin cá nhân thành công.";
            public const string PasswordChanged = "Đổi mật khẩu thành công.";
            public const string BannedUser = "Tài khoản của bạn đã bị khóa với lí do  ... .";
        }
    }
}