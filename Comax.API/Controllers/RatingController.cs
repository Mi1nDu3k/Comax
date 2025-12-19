using Comax.Common.DTOs.Rating; // Sửa lỗi RatingRequest
using Comax.Data.Entities;    // Sửa lỗi Rating entity
using Comax.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Sửa lỗi ClaimTypes

namespace Comax.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        // Sử dụng Repo thông qua UnitOfWork hoặc inject trực tiếp

        public RatingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Rate([FromBody] RatingRequest request)
        {
            // Lấy UserId từ Token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            // Truy cập Repo thông qua UnitOfWork (Ví dụ: _unitOfWork.Ratings)
            var existingRating = await _unitOfWork.Ratings.GetAsync(r =>
                r.ComicId == request.ComicId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Score = request.Score;
                _unitOfWork.Ratings.Update(existingRating);
            }
            else
            {
                await _unitOfWork.Ratings.AddAsync(new Rating
                {
                    ComicId = request.ComicId,
                    UserId = userId,
                    Score = request.Score
                });
            }

            await _unitOfWork.CommitAsync();
            return Ok(new { message = "Đánh giá thành công!" });
        }
    }
}