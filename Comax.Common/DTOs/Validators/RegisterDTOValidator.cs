using Comax.Common.DTOs;
using FluentValidation;

namespace Comax.Common.DTOs.Validators.Auth
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
                .MinimumLength(3).WithMessage("Tên đăng nhập phải dài hơn 3 ký tự");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không hợp lệ");

            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự viết hoa.")
                .Matches(@"[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự viết thường.")
                .Matches(@"[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
                .Matches(@"[\!\?\*\@\#\$\%\^\&\(\)\.\,\;\:\<\>\{\}\[\]\-_=\+]")
                .WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (VD: ! @ # $ % ...)");
        }
    }
}