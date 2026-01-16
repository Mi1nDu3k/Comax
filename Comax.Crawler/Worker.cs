using Comax.Data;
using Comax.Data.Entities;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Comax.Crawler
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IMinioClient _minioClient;

        // --- CẤU HÌNH TARGET (LUOTTRUYEN / NETTRUYENZM) ---
        private const string TARGET_URL = "https://nettruyenzm.com/truyen-tranh/vo-dich-chi-voi-1-mau";
        private const string DOMAIN_URL = "https://nettruyenzm.com";
        private const string REFERER_URL = "https://nettruyenzm.com/";

        // --- BỘ XPATH CHUẨN ---
        // 1. Danh sách Chapter (Tìm div chứa list-chapter -> chapter -> a)
        private const string XPATH_CHAPTER_LIST = "//div[contains(@class, 'list-chapter')]//div[contains(@class, 'chapter')]/a";

        // 2. Tên truyện
        private const string XPATH_TITLE = "//h1[@class='title-detail']";

        // 3. Mô tả
        private const string XPATH_DESC = "//div[@class='detail-content']//p";

        // 4. Ảnh bìa
        private const string XPATH_COVER = "//div[@class='col-image']/img";

        // 5. Tác giả
        private const string XPATH_AUTHOR = "//li[contains(@class, 'author')]//p[2]";

        // 6. Ảnh nội dung (Lấy tất cả thẻ img trong khung đọc)
        private const string XPATH_CHAPTER_IMAGES = "//div[contains(@class, 'reading-detail')]//img";

        public Worker(
            IServiceProvider serviceProvider,
            ILogger<Worker> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IMinioClient minioClient)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _minioClient = minioClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Đảm bảo Bucket tồn tại trước khi chạy
            await EnsureBucketExistsAsync("comics-bucket");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("🕷️ Crawler bắt đầu hoạt động: {time}", DateTimeOffset.Now);

                try
                {
                    await ProcessComicAsync(TARGET_URL);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Lỗi nghiêm trọng trong quá trình Crawl");
                }

                _logger.LogInformation("💤 Nghỉ ngơi 30 phút trước lần chạy tiếp theo...");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task ProcessComicAsync(string url)
        {
            // 1. Lấy HTML từ Flaresolverr
            string html = await GetHtmlViaFlaresolverr(url);
            if (string.IsNullOrEmpty(html))
            {
                _logger.LogWarning("⚠️ Không lấy được HTML (Có thể do Captcha hoặc sai URL).");
                return;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 2. Parse thông tin truyện
            var title = doc.DocumentNode.SelectSingleNode(XPATH_TITLE)?.InnerText.Trim() ?? "Unknown";
            var desc = doc.DocumentNode.SelectSingleNode(XPATH_DESC)?.InnerText.Trim() ?? "Đang cập nhật...";
            var authorName = doc.DocumentNode.SelectSingleNode(XPATH_AUTHOR)?.InnerText.Trim() ?? "Unknown";

            // --- XỬ LÝ LINK ẢNH BÌA (Hỗ trợ Lazy Load) ---
            var coverNode = doc.DocumentNode.SelectSingleNode(XPATH_COVER);
            string coverUrl = "";
            if (coverNode != null)
            {
                // Ưu tiên data-original -> data-src -> src
                coverUrl = coverNode.GetAttributeValue("data-original", "");
                if (string.IsNullOrEmpty(coverUrl)) coverUrl = coverNode.GetAttributeValue("data-src", "");
                if (string.IsNullOrEmpty(coverUrl)) coverUrl = coverNode.GetAttributeValue("src", "");
            }
            if (coverUrl.StartsWith("//")) coverUrl = "https:" + coverUrl;
            // ---------------------------------------------

            _logger.LogInformation($"🔎 Đang xử lý truyện: {title}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();
                var comic = await db.Comics.FirstOrDefaultAsync(c => c.Title == title);
                string savedCoverPath = "";

                // 3. Logic tải Ảnh Bìa (Tải nếu truyện mới hoặc truyện cũ bị thiếu ảnh)
                if (comic == null || string.IsNullOrEmpty(comic.CoverImage))
                {
                    if (!string.IsNullOrEmpty(coverUrl))
                    {
                        _logger.LogInformation("... Đang tải ảnh bìa về MinIO");
                        savedCoverPath = await DownloadAndUploadImage(coverUrl, "comics-bucket", "covers");
                    }
                }

                // 4. Lưu/Cập nhật Truyện vào DB
                if (comic == null)
                {
                    // Tạo tác giả nếu chưa có
                    int authorId = await GetOrCreateAuthorIdAsync(db, authorName);

                    comic = new Comic
                    {
                        Title = title,
                        Slug = GenerateSlug(title),
                        Description = desc,
                        AuthorId = authorId,
                        CoverImage = savedCoverPath, // Lưu đường dẫn MinIO
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = "1",
                        ViewCount = 0
                    };

                    db.Comics.Add(comic);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"✅ Đã tạo mới truyện: {title}");
                }
                else
                {
                    _logger.LogInformation("ℹ️ Truyện đã tồn tại.");

                    // Nếu truyện cũ thiếu ảnh bìa thì cập nhật bổ sung ngay
                    if (string.IsNullOrEmpty(comic.CoverImage) && !string.IsNullOrEmpty(savedCoverPath))
                    {
                        comic.CoverImage = savedCoverPath;
                        comic.UpdatedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        _logger.LogInformation("✅ Đã cập nhật bổ sung ảnh bìa còn thiếu!");
                    }
                }

                // 5. Xử lý danh sách Chapter
                var chapterNodes = doc.DocumentNode.SelectNodes(XPATH_CHAPTER_LIST);
                if (chapterNodes != null)
                {
                    _logger.LogInformation($"... Tìm thấy {chapterNodes.Count} chapters trên web.");

                    // Duyệt ngược từ dưới lên (Chapter 1 -> Mới nhất)
                    foreach (var node in chapterNodes.Reverse())
                    {
                        string chapName = node.InnerText.Trim();
                        string chapUrl = node.GetAttributeValue("href", "");

                        // Nối domain nếu link tương đối
                        if (!chapUrl.StartsWith("http"))
                        {
                            chapUrl = DOMAIN_URL + chapUrl;
                        }

                        float chapNum = ParseChapterNumber(chapName);

                        // Kiểm tra nếu chapter chưa có thì mới cào
                        bool exists = await db.Chapters.AnyAsync(c => c.ComicId == comic.Id && c.ChapterNumber == chapNum);
                        if (!exists)
                        {
                            await ProcessChapterAsync(db, comic.Id, chapNum, chapName, chapUrl);
                        }
                    }
                }
                else
                {
                    _logger.LogError("⚠️ KHÔNG tìm thấy danh sách chapter! (Có thể do lỗi XPath hoặc Captcha)");
                }
            }
        }

        private async Task ProcessChapterAsync(ComaxDbContext db, int comicId, float chapNum, string chapName, string chapUrl)
        {
            _logger.LogInformation($"   ⬇️ Đang cào: {chapName}");

            string html = await GetHtmlViaFlaresolverr(chapUrl);
            if (string.IsNullOrEmpty(html)) return;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var imgNodes = doc.DocumentNode.SelectNodes(XPATH_CHAPTER_IMAGES);
            if (imgNodes == null)
            {
                _logger.LogWarning($"   ⚠️ Không thấy ảnh trong {chapName}");
                return;
            }

            var imageUrls = new List<string>();
            foreach (var node in imgNodes)
            {
                // --- LOGIC LẤY ẢNH THÔNG MINH ---
                string src = node.GetAttributeValue("data-original", "");
                if (string.IsNullOrEmpty(src)) src = node.GetAttributeValue("data-src", "");
                if (string.IsNullOrEmpty(src)) src = node.GetAttributeValue("src", "");

                if (src.StartsWith("//")) src = "https:" + src;

                // Chỉ lấy nếu link hợp lệ
                if (!string.IsNullOrEmpty(src) && src.StartsWith("http"))
                {
                    imageUrls.Add(src);
                }
            }

            // Tải ảnh song song (Tăng tốc độ)
            var uploadTasks = imageUrls.Select((url, index) =>
                DownloadAndUploadImage(url, "comics-bucket", $"chapters/{comicId}/{chapNum}")
            ).ToList();

            var savedPaths = await Task.WhenAll(uploadTasks);

            // Nếu không tải được ảnh nào -> Bỏ qua, không lưu chapter rỗng
            if (savedPaths.All(p => string.IsNullOrEmpty(p)))
            {
                _logger.LogError($"   ❌ Thất bại: Không tải được ảnh nào của {chapName}");
                return;
            }

            // Lưu Chapter vào DB
            var chapter = new Chapter
            {
                ComicId = comicId,
                ChapterNumber = chapNum,
                Title = chapName,
                Slug = GenerateSlug(chapName),
                CreatedAt = DateTime.UtcNow,
                PublishDate = DateTime.UtcNow,
                Pages = new List<Page>()
            };

            for (int i = 0; i < savedPaths.Length; i++)
            {
                if (!string.IsNullOrEmpty(savedPaths[i]))
                {
                    chapter.Pages.Add(new Page
                    {
                        ImageUrl = savedPaths[i],
                        Index = i,
                        FileName = Path.GetFileName(savedPaths[i])
                    });
                }
            }

            db.Chapters.Add(chapter);

            // Cập nhật "Ngày cập nhật" cho truyện chính
            var comic = await db.Comics.FindAsync(comicId);
            if (comic != null) comic.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            _logger.LogInformation($"   ✨ Hoàn thành {chapName} ({chapter.Pages.Count} ảnh)");

            // Delay nhẹ để tránh bị chặn IP
            await Task.Delay(2000);
        }

        // --- CÁC HÀM HELPER ---

        private async Task<int> GetOrCreateAuthorIdAsync(ComaxDbContext db, string authorName)
        {
            if (string.IsNullOrEmpty(authorName)) authorName = "Unknown";
            var author = await db.Authors.FirstOrDefaultAsync(a => a.Name == authorName);

            if (author == null)
            {
                author = new Author
                {
                    Name = authorName,
                    // Slug = GenerateSlug(authorName), // Bỏ comment nếu Entity Author có cột Slug
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Authors.Add(author);
                await db.SaveChangesAsync();
            }
            return author.Id;
        }

        private async Task<string> DownloadAndUploadImage(string url, string bucket, string folder)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return "";

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                // QUAN TRỌNG: Giả lập trình duyệt và Referer để vượt qua Hotlink Protection
                client.DefaultRequestHeaders.Add("Referer", REFERER_URL);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                var imageBytes = await client.GetByteArrayAsync(url);
                using var stream = new MemoryStream(imageBytes);

                string fileName = $"{folder}/{Guid.NewGuid()}.jpg";

                var args = new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType("image/jpeg");

                await _minioClient.PutObjectAsync(args);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tải ảnh {url}: {ex.Message}");
                return "";
            }
        }

        private async Task<string> GetHtmlViaFlaresolverr(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Tăng timeout lên 60s để chờ Cloudflare
                var payload = new { cmd = "request.get", url = url, maxTimeout = 60000 };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var flareUrl = _config["Flaresolverr:Url"] ?? "http://flaresolverr:8191/v1";
                var response = await client.PostAsync(flareUrl, content);

                if (!response.IsSuccessStatusCode) return "";

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("solution", out var solution))
                {
                    return solution.GetProperty("response").GetString() ?? "";
                }
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi kết nối Flaresolverr: {ex.Message}");
                return "";
            }
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
                if (!found)
                {
                    // 1. Tạo Bucket
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                    _logger.LogInformation($"✅ Đã tạo Bucket mới: {bucketName}");

                    string policyJson = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{ ""AWS"": [""*""] }},
                                ""Action"": [""s3:GetObject""],
                                ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                            }}
                        ]
                    }}";

                    await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                        .WithBucket(bucketName)
                        .WithPolicy(policyJson));

                    _logger.LogInformation($"🔓 Đã set quyền PUBLIC cho Bucket: {bucketName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi kiểm tra/tạo Bucket MinIO: {ex.Message}");
            }
        }

        private float ParseChapterNumber(string text)
        {
            // Lấy số từ chuỗi (VD: "Chapter 10.5" -> 10.5)
            var match = Regex.Match(text, @"\d+(\.\d+)?");
            if (match.Success && float.TryParse(match.Value, out float result))
            {
                return result;
            }
            return 0;
        }

        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";
            string str = phrase.ToLower();
            // Xóa ký tự đặc biệt
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Thay khoảng trắng bằng dấu gạch ngang
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}