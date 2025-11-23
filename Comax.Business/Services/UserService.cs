using AutoMapper;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Business.Interfaces;
using Comax.Business.Services;

namespace Comax.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IJwtHelper _jwtHelper;

        public UserService(IUserRepository userRepo, IMapper mapper, IJwtHelper jwtHelper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<UserDTO> CreateAsync(UserCreateDTO dto)
        {
            var entity = _mapper.Map<User>(dto);
            entity.PasswordHash = PasswordHelper.HashPassword(dto.Password);

            await _userRepo.AddAsync(entity);
            return _mapper.Map<UserDTO>(entity);
        }

        public async Task<UserDTO> UpdateAsync(int id, UserUpdateDTO dto)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            _mapper.Map(dto, entity);

            if (!string.IsNullOrEmpty(dto.Password))
                entity.PasswordHash = PasswordHelper.HashPassword(dto.Password);

            await _userRepo.UpdateAsync(entity);
            return _mapper.Map<UserDTO>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            await _userRepo.DeleteAsync(entity);
            return true;
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            return _mapper.Map<UserDTO>(entity);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var entities = await _userRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(entities);
        }
        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already exists."); // kết thúc ở đây

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                RoleId = dto.RoleId
            };

            await _userRepo.AddAsync(user);

            // Load Role 
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

    }
}