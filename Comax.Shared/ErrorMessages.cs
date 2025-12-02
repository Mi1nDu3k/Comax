namespace Comax.Shared
{
    public static class ErrorMessages
    {
        // 1. Các thông báo liên quan đến xác thực (Auth)
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

        // 2. Các thông báo liên quan đến lỗi hệ thống (System)
        public static class System
        {
            public const string RoleNotFound = "Lỗi hệ thống: Không tìm thấy Role yêu cầu.";
            public const string DefaultRoleNotFound = "Lỗi hệ thống: Không tìm thấy Role mặc định.";
            public const string ConcurrencyConflict = "Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang.";
        }

        // 3. Các thông báo dùng cho Validation (FluentValidation)
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
        public static class Comic
        {
            public const string SlugRequired = "Slug không được để trống.";
            public const string NotFoundBySlug = "Không tìm thấy truyện với slug: {0}";
            public const string ViewCountIncreased = "Đã tăng lượt xem thành công.";
        }
    }
}