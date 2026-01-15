using Comax.Business.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Rating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Comax.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        // Inject Service, KHÔNG Inject UnitOfWork
        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

      
        /// <summary>
        /// GET: api/rating/check/9
        /// </summary>
        /// <param name="comicId"></param>
        /// <returns></returns>
        [HttpGet("check/{comicId}")]
        public async Task<IActionResult> CheckRating(int comicId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var score = await _ratingService.GetUserRatingForComicAsync(userId, comicId);
                return Ok(new { score });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// 2. API Đánh giá (Upsert)  và tính điểm trung bình
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> Rate([FromBody] RatingCreateDTO request)
        {
            try
            {

                var userId = GetCurrentUserId();


                var newAverage = await _ratingService.AddOrUpdateRatingAsync(userId, request);

                return Ok(new { message = SystemMessages.Rating.Success, newRating = newAverage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        private int GetCurrentUserId()
        { 
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type} | Value: {claim.Value}");
            }

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (idClaim == null)
            {
                idClaim = User.FindFirst("id");
            }
            if (idClaim == null)
            {
                idClaim = User.FindFirst("sub");
            }

            if (idClaim == null)
            {
                throw new Exception(SystemMessages.Common.UserClaimNotFound);
            }

            return int.Parse(idClaim.Value);
        }
    }
}