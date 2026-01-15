using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Common.Constants;
using Comax.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favService;

    public FavoritesController(IFavoriteService favService) { _favService = favService; }

    [HttpPost("{comicId}")]
    public async Task<IActionResult> Toggle(int comicId)
    {
        // Lấy UserId từ Token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAdded = await _favService.ToggleFavoriteAsync(userId, comicId);
        return Ok(new { isFavorited = isAdded, message = isAdded ? SystemMessages.Favorite.Added : SystemMessages.Favorite.Removed });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var comics = await _favService.GetFavoritesByUserIdAsync(userId);
        return Ok(comics);
    }

    [HttpGet("check/{comicId}")]
    public async Task<IActionResult> CheckStatus(int comicId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isFav = await _favService.IsFavoritedAsync(userId, comicId);
        return Ok(new { isFavorited = isFav });
    }
    [HttpDelete("{comicId}")]
    public async Task<IActionResult> Unfavorite(int comicId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _favService.UnfavoriteAsync(userId, comicId);
        return Ok(new { message = SystemMessages.Favorite.Deleted });
    }
}