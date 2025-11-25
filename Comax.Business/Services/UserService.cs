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

        // GIỮ LẠI các phương thức độc nhất: Auth
        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                // Assuming default role is User for registration, but using RoleId from DTO
                RoleId = dto.RoleId
            };

            await _userRepo.AddAsync(user);

            // Load Role (cần Include Role trong repo GetByIdAsync để tránh lỗi)
            var createdUser = await _userRepo.GetByIdAsync(user.Id);

            var token = _jwtHelper.GenerateToken(createdUser.Id.ToString(), createdUser.Role.Name);

            return new AuthResultDTO
            {
                Token = token,
                Username = createdUser.Username,
                Role = createdUser.Role.Name
            };
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials.");

            var token = _jwtHelper.GenerateToken(user.Id.ToString(), user.Role.Name);

            return new AuthResultDTO
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.Name
            };
        }

        // TẤT CẢ các phương thức CRUD còn lại được KẾ THỪA.
    }
}