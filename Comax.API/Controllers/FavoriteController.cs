using Comax.Business.Interfaces;
using Comax.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favService;

    public FavoriteController(IFavoriteService favService) { _favService = favService; }

    [HttpPost("{comicId}")]
    public async Task<IActionResult> Toggle(int comicId)
    {
        // Lấy UserId từ Token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAdded = await _favService.ToggleFavoriteAsync(userId, comicId);
        return Ok(new { isFavorited = isAdded, message = isAdded ? "Đã thêm vào yêu thích" : "Đã bỏ yêu thích" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var list = await _favService.GetUserFavoritesAsync(userId);
        return Ok(list);
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
        return Ok(new { message = "Đã xóa khỏi danh sách yêu thích." });
    }
}