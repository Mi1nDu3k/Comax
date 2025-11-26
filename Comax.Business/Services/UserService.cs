using AutoMapper;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Comax.Business.Services
{
    // CẬP NHẬT: Kế thừa BaseService và triển khai IUserService
    public class UserService : BaseService<User, UserDTO, UserCreateDTO, UserUpdateDTO>, IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IJwtHelper _jwtHelper;

        public UserService(IUserRepository userRepo, IMapper mapper, IJwtHelper jwtHelper) : base(userRepo, mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return new AuthResultDTO
                {
                    Success = false,
                    Message = "Email has been used"
                };

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
               
                RoleId = dto.RoleId
            };

            await _userRepo.AddAsync(user);
            var createdUser = await _userRepo.GetByIdWithRoleAsync(user.Id);


            if(createdUser == null) throw new Exception("User creation failed.");

            var token = _jwtHelper.GenerateToken(createdUser.Id.ToString(), createdUser.Role.Name);

            return new AuthResultDTO
            {
                Success = true,
                Message = "Register success!",
                Token = token,
                Username = createdUser.Username,
                Role = createdUser.Role.Name
            };
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new AuthResultDTO
                {
                    Success = false,
                    Message = "Tài khoản hoặc mật khẩu không đúng."
                };
            }

            var token = _jwtHelper.GenerateToken(user.Id.ToString(), user.Role.Name);

            return new AuthResultDTO
            {
                Success = true,
                Message = "Welcome to Comax",
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.Name
            };
        }

       
    }
}