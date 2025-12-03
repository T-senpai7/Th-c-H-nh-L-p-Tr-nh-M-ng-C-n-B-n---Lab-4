using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Bai4
{
    public class ScrapedMovieInfo
    {
        public string title { get; set; } = "";
        public string poster { get; set; } = "";
        public string description { get; set; } = "";
        public string duration { get; set; } = "";
        public string genre { get; set; } = "";
        public double basePrice { get; set; } = 85000; // Default price
        public string detailUrl { get; set; } = ""; // Link to movie detail page
    }

    public class MovieScraper
    {
        private const string BETA_CINEMAS_URL = "https://betacinemas.vn/phim.htm";
        private const string MOVIES_JSON_FILE = "movies.json";

        public async Task<List<ScrapedMovieInfo>> CrawlMoviesAsync()
        {
            var movies = new List<ScrapedMovieInfo>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    
                    Console.WriteLine($"Fetching HTML from: {BETA_CINEMAS_URL}");
                    var html = await httpClient.GetStringAsync(BETA_CINEMAS_URL);
                    Console.WriteLine($"HTML fetched, length: {html.Length}");
                    
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    // Betacinemas.vn structure - look for movie cards
                    // Common selectors for movie items
                    var movieNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, 'chi-tiet-phim')] | //div[contains(@class, 'movie')]//a | //article[contains(@class, 'movie')]");

                    if (movieNodes == null || movieNodes.Count == 0)
                    {
                        // Try more generic selectors
                        movieNodes = doc.DocumentNode.SelectNodes("//div[@class='film-item'] | //div[contains(@class, 'film')] | //div[@class='movie-item']");
                    }

                    var processedTitles = new HashSet<string>();

                    if (movieNodes != null)
                    {
                        Console.WriteLine($"Found {movieNodes.Count} potential movie nodes");
                        
                        foreach (var node in movieNodes)
                        {
                            try
                            {
                                var movie = new ScrapedMovieInfo();

                                // Extract detail URL first
                                var linkNode = node.Name == "a" ? node : node.SelectSingleNode(".//a[contains(@href, 'chi-tiet-phim')]");
                                if (linkNode != null)
                                {
                                    var href = linkNode.GetAttributeValue("href", "");
                                    if (!string.IsNullOrEmpty(href))
                                    {
                                        if (href.StartsWith("http"))
                                            movie.detailUrl = href;
                                        else if (href.StartsWith("/"))
                                            movie.detailUrl = "https://betacinemas.vn" + href;
                                        else
                                            movie.detailUrl = "https://betacinemas.vn/" + href;
                                    }
                                }

                                // Extract title - try multiple selectors
                                var titleNode = node.SelectSingleNode(".//h2 | .//h3 | .//h4 | .//div[contains(@class, 'title')] | .//span[contains(@class, 'title')] | .//a[contains(@class, 'title')]");
                                if (titleNode != null)
                                {
                                    movie.title = WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                                }
                                else if (linkNode != null)
                                {
                                    // Try to get title from link text or alt
                                    movie.title = WebUtility.HtmlDecode(linkNode.InnerText.Trim());
                                    if (string.IsNullOrEmpty(movie.title))
                                    {
                                        var imgInLink = linkNode.SelectSingleNode(".//img");
                                        if (imgInLink != null)
                                        {
                                            movie.title = imgInLink.GetAttributeValue("alt", "").Trim();
                                        }
                                    }
                                }

                                // Filter out invalid titles
                                if (string.IsNullOrEmpty(movie.title) || 
                                    movie.title.Contains("TRAILER") || 
                                    movie.title.Contains("LỊCH CHIẾU") ||
                                    movie.title.Contains("BẠN ĐANG") ||
                                    movie.title.Length < 3)
                                {
                                    continue;
                                }

                                // Skip duplicates
                                if (processedTitles.Contains(movie.title))
                                    continue;
                                processedTitles.Add(movie.title);

                                // Extract poster/image - tìm tất cả ảnh và ưu tiên poster thật từ betacorp
                                var allImgNodes = node.SelectNodes(".//img");
                                if (allImgNodes != null && allImgNodes.Count > 0)
                                {
                                    string bestPosterUrl = "";
                                    int priority = 0;
                                    
                                    foreach (var imgNode in allImgNodes)
                                    {
                                        string posterUrl = imgNode.GetAttributeValue("src", "");
                                        if (string.IsNullOrEmpty(posterUrl))
                                        {
                                            posterUrl = imgNode.GetAttributeValue("data-src", "");
                                        }
                                        if (string.IsNullOrEmpty(posterUrl))
                                        {
                                            posterUrl = imgNode.GetAttributeValue("data-lazy-src", "");
                                        }
                                        
                                        if (!string.IsNullOrEmpty(posterUrl))
                                        {
                                            // Make URL absolute
                                            if (posterUrl.StartsWith("//"))
                                                posterUrl = "https:" + posterUrl;
                                            else if (posterUrl.StartsWith("/"))
                                                posterUrl = "https://betacinemas.vn" + posterUrl;
                                            else if (!posterUrl.StartsWith("http"))
                                                posterUrl = "https://betacinemas.vn/" + posterUrl;
                                            
                                            var lowerUrl = posterUrl.ToLower();
                                            
                                            // Loại bỏ các ảnh icon, flag, logo - không phải poster
                                            if (lowerUrl.Contains("/icons/") || 
                                                lowerUrl.Contains("/icon") ||
                                                lowerUrl.Contains("united-kingdom") ||
                                                lowerUrl.Contains("flag") ||
                                                lowerUrl.Contains("/assets/common/") ||
                                                lowerUrl.Contains("/logo") ||
                                                lowerUrl.Contains("common/icons"))
                                            {
                                                continue; // Bỏ qua ảnh này
                                            }
                                            
                                            // Tính priority: ưu tiên files.betacorp.vn cao nhất
                                            int currentPriority = 0;
                                            
                                            // Ưu tiên cao nhất cho files.betacorp.vn (nơi lưu poster thật)
                                            if (lowerUrl.Contains("files.betacorp.vn"))
                                                currentPriority = 50; // Rất cao
                                            else if (lowerUrl.Contains("betacorp") || lowerUrl.Contains("betacinemas"))
                                                currentPriority = 20;
                                            else
                                                currentPriority = 5; // Thấp cho các domain khác
                                            
                                            // Bonus cho format ảnh
                                            if (lowerUrl.Contains(".png"))
                                                currentPriority += 5;
                                            else if (lowerUrl.Contains(".jpg") || lowerUrl.Contains(".jpeg"))
                                                currentPriority += 5;
                                            else if (lowerUrl.Contains(".webp"))
                                                currentPriority += 3;
                                            
                                            // Bonus rất cao nếu có từ "poster" trong URL
                                            if (lowerUrl.Contains("poster"))
                                                currentPriority += 10;
                                            
                                            if (currentPriority > priority)
                                            {
                                                priority = currentPriority;
                                                bestPosterUrl = posterUrl;
                                            }
                                        }
                                        
                                        // Also get alt text as title if title is empty
                                        if (string.IsNullOrEmpty(movie.title))
                                        {
                                            movie.title = imgNode.GetAttributeValue("alt", "").Trim();
                                        }
                                    }
                                    
                                    if (!string.IsNullOrEmpty(bestPosterUrl))
                                    {
                                        movie.poster = bestPosterUrl;
                                        Console.WriteLine($"Found poster: {bestPosterUrl} (Priority: {priority})");
                                    }
                                }

                                // Extract description - try to find synopsis
                                var descNode = node.SelectSingleNode(".//p[contains(@class, 'desc')] | .//div[contains(@class, 'description')] | .//div[contains(@class, 'synopsis')] | .//p");
                                if (descNode != null)
                                {
                                    movie.description = WebUtility.HtmlDecode(descNode.InnerText.Trim());
                                    if (movie.description.Length > 300)
                                        movie.description = movie.description.Substring(0, 300) + "...";
                                }

                                // Try to fetch detail page for more info
                                if (!string.IsNullOrEmpty(movie.detailUrl))
                                {
                                    try
                                    {
                                        await EnhanceMovieFromDetailPage(movie, httpClient);
                                    }
                                    catch
                                    {
                                        // If detail page fetch fails, continue with basic info
                                    }
                                }

                                // Extract price if available on listing page
                                var priceNode = node.SelectSingleNode(".//span[contains(@class, 'price')] | .//div[contains(@class, 'price')]");
                                if (priceNode != null)
                                {
                                    var priceText = priceNode.InnerText;
                                    // Try to parse price (e.g., "85.000 VNĐ" -> 85000)
                                    var priceMatch = System.Text.RegularExpressions.Regex.Match(priceText, @"(\d{1,3}(?:[.,]\d{3})*)");
                                    if (priceMatch.Success)
                                    {
                                        var priceStr = priceMatch.Groups[1].Value.Replace(".", "").Replace(",", "");
                                        if (double.TryParse(priceStr, out double price))
                                        {
                                            movie.basePrice = price;
                                        }
                                    }
                                }

                                // Only add if we have title
                                if (!string.IsNullOrEmpty(movie.title) && movie.title.Length >= 3)
                                {
                                    movies.Add(movie);
                                    Console.WriteLine($"Added movie: {movie.title}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error parsing movie node: {ex.Message}");
                            }
                        }
                    }

                    // Limit to 3 newest + others
                    if (movies.Count > 0)
                    {
                        // Sort by detailUrl or keep order (first ones are usually newest)
                        var finalMovies = movies.Take(3).ToList(); // First 3 are newest
                        Console.WriteLine($"Crawled {finalMovies.Count} movies (3 newest)");
                    }

                    // If no movies found, use sample data
                    if (movies.Count == 0)
                    {
                        Console.WriteLine("No movies found, using sample data");
                        movies.AddRange(CreateSampleMovies());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crawling movies: {ex.Message}\n{ex.StackTrace}");
                movies.AddRange(CreateSampleMovies());
            }

            return movies;
        }

        private async Task EnhanceMovieFromDetailPage(ScrapedMovieInfo movie, HttpClient httpClient)
        {
            try
            {
                var detailHtml = await httpClient.GetStringAsync(movie.detailUrl);
                var detailDoc = new HtmlAgilityPack.HtmlDocument();
                detailDoc.LoadHtml(detailHtml);

                // Extract description from detail page
                // Tìm description trong thẻ <p> với class cụ thể như trong ảnh
                var descNode = detailDoc.DocumentNode.SelectSingleNode(
                    "//p[contains(@class, 'margin-bottom-15') and contains(@class, 'font-lg') and contains(@class, 'font-family-san') and contains(@class, 'text-justify')] | " +
                    "//p[@class='margin-bottom-15 font-lg font-family-san text-justify'] | " +
                    "//p[contains(@class, 'margin-bottom-15')] | " +
                    "//div[contains(@class, 'synopsis')] | " +
                    "//div[contains(@class, 'description')] | " +
                    "//p[contains(@class, 'desc')]");
                
                if (descNode != null)
                {
                    var descriptionText = WebUtility.HtmlDecode(descNode.InnerText.Trim());
                    // Lấy toàn bộ description, không giới hạn độ dài
                    if (!string.IsNullOrEmpty(descriptionText))
                    {
                        movie.description = descriptionText;
                        Console.WriteLine($"Found description: {movie.description.Substring(0, Math.Min(50, movie.description.Length))}...");
                    }
                }
                
                // Nếu vẫn chưa có description, thử tìm trong các thẻ p khác
                if (string.IsNullOrEmpty(movie.description))
                {
                    var allPNodes = detailDoc.DocumentNode.SelectNodes("//p");
                    if (allPNodes != null)
                    {
                        foreach (var pNode in allPNodes)
                        {
                            var pText = WebUtility.HtmlDecode(pNode.InnerText.Trim());
                            // Tìm đoạn văn dài nhất có vẻ là description (ít nhất 50 ký tự)
                            if (pText.Length >= 50 && 
                                !pText.Contains("ĐẠO DIỄN") && 
                                !pText.Contains("DIỄN VIÊN") &&
                                !pText.Contains("THỂ LOẠI") &&
                                !pText.Contains("THỜI LƯỢNG") &&
                                !pText.Contains("NGÔN NGỮ") &&
                                !pText.Contains("NGÀY KHỞI CHIẾU"))
                            {
                                movie.description = pText;
                                Console.WriteLine($"Found description from fallback: {movie.description.Substring(0, Math.Min(50, movie.description.Length))}...");
                                break;
                            }
                        }
                    }
                }

                // Extract duration - tìm theo cấu trúc HTML chính xác của betacinemas.vn
                // Label "THỜI LƯỢNG" nằm trong div class "col-lg-4 col-md-4 col-sm-4 col-xs-6"
                // Giá trị nằm trong div class "col-lg-12 col-md-12 col-sm-12 col-xs-10" (sibling tiếp theo trong cùng row)
                var durationLabelNode = detailDoc.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'col-lg-4') and (contains(text(), 'THỜI LƯỢNG') or contains(text(), 'Thời lượng'))] | " +
                    "//div[contains(text(), 'THỜI LƯỢNG')] | " +
                    "//div[contains(text(), 'Thời lượng')]");
                
                if (durationLabelNode != null)
                {
                    // Tìm trong cùng row (div có class chứa "row")
                    var parentRow = durationLabelNode.SelectSingleNode("./ancestor::div[contains(@class, 'row')]");
                    if (parentRow != null)
                    {
                        // Tìm tất cả các div col trong cùng row và duyệt tuần tự
                        var allCols = parentRow.SelectNodes(".//div[contains(@class, 'col-lg') or contains(@class, 'col-md')]");
                        if (allCols != null)
                        {
                            bool foundLabel = false;
                            foreach (var col in allCols)
                            {
                                var colText = WebUtility.HtmlDecode(col.InnerText.Trim());
                                var colClass = col.GetAttributeValue("class", "");
                                
                                // Tìm label "THỜI LƯỢNG" trong div có class chứa "col-lg-4"
                                if ((colText.Contains("THỜI LƯỢNG") || colText.Contains("Thời lượng")) && 
                                    colClass.Contains("col-lg-4"))
                                {
                                    foundLabel = true;
                                    continue;
                                }
                                
                                // Nếu đã tìm thấy label, lấy div tiếp theo có class chứa "col-lg-12"
                                if (foundLabel && colClass.Contains("col-lg-12"))
                                {
                                    var durationText = WebUtility.HtmlDecode(col.InnerText.Trim());
                                    
                                    // Tìm số phút trong text
                                    var durationMatch = System.Text.RegularExpressions.Regex.Match(durationText, @"(\d+)\s*phút");
                                    if (durationMatch.Success)
                                    {
                                        movie.duration = durationMatch.Groups[1].Value + " phút";
                                        Console.WriteLine($"Found duration: {movie.duration}");
                                        break;
                                    }
                                    else if (System.Text.RegularExpressions.Regex.IsMatch(durationText, @"\d+"))
                                    {
                                        // Nếu có số nhưng không có "phút", thử thêm "phút"
                                        var numMatch = System.Text.RegularExpressions.Regex.Match(durationText, @"(\d+)");
                                        if (numMatch.Success)
                                        {
                                            movie.duration = numMatch.Groups[1].Value + " phút";
                                            Console.WriteLine($"Found duration (with added 'phút'): {movie.duration}");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // Fallback: tìm div sibling tiếp theo có class col-lg-12
                    if (string.IsNullOrEmpty(movie.duration))
                    {
                        var followingCol = durationLabelNode.SelectSingleNode(
                            "./following-sibling::div[contains(@class, 'col-lg-12')] | " +
                            "../div[contains(@class, 'col-lg-12')]");
                        
                        if (followingCol != null)
                        {
                            var durationText = WebUtility.HtmlDecode(followingCol.InnerText.Trim());
                            var durationMatch = System.Text.RegularExpressions.Regex.Match(durationText, @"(\d+)\s*phút");
                            if (durationMatch.Success)
                            {
                                movie.duration = durationMatch.Groups[1].Value + " phút";
                                Console.WriteLine($"Found duration (fallback): {movie.duration}");
                            }
                        }
                    }
                }
                
                // Fallback: tìm trực tiếp text chứa "phút"
                if (string.IsNullOrEmpty(movie.duration))
                {
                    var durationTextNodes = detailDoc.DocumentNode.SelectNodes("//*[contains(text(), 'phút')]");
                    if (durationTextNodes != null)
                    {
                        foreach (var node in durationTextNodes)
                        {
                            var text = WebUtility.HtmlDecode(node.InnerText.Trim());
                            var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)\s*phút");
                            if (match.Success)
                            {
                                movie.duration = match.Groups[1].Value + " phút";
                                Console.WriteLine($"Found duration (fallback search): {movie.duration}");
                                break;
                            }
                        }
                    }
                }

                // Extract genre - tìm theo cấu trúc HTML chính xác của betacinemas.vn
                // Label "Thể loại:" nằm trong div class "col-lg-4 col-md-4 col-sm-4 col-xs-6" (có thể có span bên trong)
                // Giá trị nằm trong div class "col-lg-12 col-md-12 col-sm-12 col-xs-10" (sibling tiếp theo trong cùng row)
                var genreLabelNode = detailDoc.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'col-lg-4') and (contains(text(), 'Thể loại') or contains(text(), 'THỂ LOẠI'))] | " +
                    "//div[contains(@class, 'col-lg-4')]//span[contains(text(), 'Thể loại')] | " +
                    "//div[contains(text(), 'Thể loại')] | " +
                    "//div[contains(text(), 'THỂ LOẠI')]");
                
                if (genreLabelNode != null)
                {
                    // Nếu tìm thấy span, lấy parent div
                    if (genreLabelNode.Name == "span")
                    {
                        genreLabelNode = genreLabelNode.SelectSingleNode("./ancestor::div[contains(@class, 'col-lg-4')]");
                    }
                    
                    if (genreLabelNode != null)
                    {
                        // Tìm trong cùng row (div có class chứa "row")
                        var parentRow = genreLabelNode.SelectSingleNode("./ancestor::div[contains(@class, 'row')]");
                        if (parentRow != null)
                        {
                            // Tìm div sibling tiếp theo có class chứa "col-lg-12 col-md-12 col-sm-12 col-xs-10"
                            var genreValueNode = genreLabelNode.SelectSingleNode(
                                "./following-sibling::div[contains(@class, 'col-lg-12') and contains(@class, 'col-md-12') and contains(@class, 'col-sm-12') and contains(@class, 'col-xs-10')] | " +
                                "./following-sibling::div[contains(@class, 'col-lg-12') and contains(@class, 'col-md-12')]");
                            
                            if (genreValueNode != null)
                            {
                                var genreText = WebUtility.HtmlDecode(genreValueNode.InnerText.Trim());
                                
                                // Kiểm tra xem có phải là giá trị hợp lệ không
                                if (!string.IsNullOrEmpty(genreText) && 
                                    genreText.Length > 0 && 
                                    genreText.Length < 200 &&
                                    !genreText.Contains("ĐẠO DIỄN") &&
                                    !genreText.Contains("DIỄN VIÊN") &&
                                    !genreText.Contains("THỜI LƯỢNG") &&
                                    !genreText.Contains("NGÔN NGỮ") &&
                                    !genreText.Contains("NGÀY KHỞI CHIẾU") &&
                                    !genreText.Contains("Đạo diễn") &&
                                    !genreText.Contains("Diễn viên") &&
                                    !genreText.Contains("Thời lượng") &&
                                    !genreText.Contains("Ngôn ngữ") &&
                                    !genreText.Contains("Ngày khởi chiếu") &&
                                    !genreText.Contains("Thể loại") &&
                                    !genreText.Contains("THỂ LOẠI"))
                                {
                                    movie.genre = genreText;
                                    Console.WriteLine($"Found genre: {movie.genre}");
                                }
                            }
                            
                            // Fallback: tìm tất cả các div col trong cùng row và duyệt tuần tự
                            if (string.IsNullOrEmpty(movie.genre))
                            {
                                var allCols = parentRow.SelectNodes(".//div[contains(@class, 'col-lg') or contains(@class, 'col-md')]");
                                if (allCols != null)
                                {
                                    bool foundLabel = false;
                                    foreach (var col in allCols)
                                    {
                                        var colText = WebUtility.HtmlDecode(col.InnerText.Trim());
                                        var colClass = col.GetAttributeValue("class", "");
                                        
                                        // Tìm label "Thể loại" trong div có class chứa "col-lg-4"
                                        if ((colText.Contains("Thể loại") || colText.Contains("THỂ LOẠI")) && 
                                            colClass.Contains("col-lg-4"))
                                        {
                                            foundLabel = true;
                                            continue;
                                        }
                                        
                                        // Nếu đã tìm thấy label, lấy div tiếp theo có class chứa "col-lg-12"
                                        if (foundLabel && colClass.Contains("col-lg-12"))
                                        {
                                            var genreText = WebUtility.HtmlDecode(col.InnerText.Trim());
                                            // Loại bỏ các từ khóa không cần thiết
                                            genreText = genreText.Replace("THỂ LOẠI", "").Replace("Thể loại", "").Trim();
                                            
                                            if (!string.IsNullOrEmpty(genreText) && 
                                                genreText.Length > 0 && 
                                                genreText.Length < 200 &&
                                                !genreText.Contains("ĐẠO DIỄN") &&
                                                !genreText.Contains("DIỄN VIÊN") &&
                                                !genreText.Contains("THỜI LƯỢNG") &&
                                                !genreText.Contains("NGÔN NGỮ") &&
                                                !genreText.Contains("NGÀY KHỞI CHIẾU"))
                                            {
                                                movie.genre = genreText;
                                                Console.WriteLine($"Found genre (fallback): {movie.genre}");
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Extract price from detail page
                var priceNode = detailDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'price')] | //div[contains(@class, 'price')] | //div[contains(text(), 'GIÁ VÉ')]/following-sibling::*");
                if (priceNode != null)
                {
                    var priceText = priceNode.InnerText;
                    var priceMatch = System.Text.RegularExpressions.Regex.Match(priceText, @"(\d{1,3}(?:[.,]\d{3})*)");
                    if (priceMatch.Success)
                    {
                        var priceStr = priceMatch.Groups[1].Value.Replace(".", "").Replace(",", "");
                        if (double.TryParse(priceStr, out double price))
                        {
                            movie.basePrice = price;
                        }
                    }
                }

                // Get better poster image from detail page - ưu tiên files.betacorp.vn
                var allPosterNodes = detailDoc.DocumentNode.SelectNodes("//img[contains(@src, 'poster') or contains(@src, '.png') or contains(@src, '.jpg') or contains(@src, '.jpeg') or contains(@src, '.webp')] | //div[contains(@class, 'poster')]//img | //div[contains(@class, 'thumbnail')]//img | //div[contains(@class, 'image')]//img");
                
                if (allPosterNodes != null && allPosterNodes.Count > 0)
                {
                    string bestPosterUrl = "";
                    int priority = 0;
                    
                    foreach (var imgNode in allPosterNodes)
                    {
                        var posterUrl = imgNode.GetAttributeValue("src", "");
                        if (string.IsNullOrEmpty(posterUrl))
                        {
                            posterUrl = imgNode.GetAttributeValue("data-src", "");
                        }
                        if (string.IsNullOrEmpty(posterUrl))
                        {
                            posterUrl = imgNode.GetAttributeValue("data-lazy-src", "");
                        }
                        
                        if (!string.IsNullOrEmpty(posterUrl))
                        {
                            // Normalize URL
                            if (posterUrl.StartsWith("//"))
                                posterUrl = "https:" + posterUrl;
                            else if (posterUrl.StartsWith("/"))
                                posterUrl = "https://betacinemas.vn" + posterUrl;
                            else if (!posterUrl.StartsWith("http"))
                                posterUrl = "https://betacinemas.vn/" + posterUrl;
                            
                            var lowerUrl = posterUrl.ToLower();
                            
                            // Loại bỏ các ảnh icon, flag, logo - không phải poster
                            if (lowerUrl.Contains("/icons/") || 
                                lowerUrl.Contains("/icon") ||
                                lowerUrl.Contains("united-kingdom") ||
                                lowerUrl.Contains("flag") ||
                                lowerUrl.Contains("/assets/common/") ||
                                lowerUrl.Contains("/logo") ||
                                lowerUrl.Contains("common/icons"))
                            {
                                continue; // Bỏ qua ảnh này
                            }
                            
                            // Tính priority: ưu tiên files.betacorp.vn cao nhất
                            int currentPriority = 0;
                            
                            // Ưu tiên cao nhất cho files.betacorp.vn (nơi lưu poster thật)
                            if (lowerUrl.Contains("files.betacorp.vn"))
                                currentPriority = 50; // Rất cao
                            else if (lowerUrl.Contains("betacorp") || lowerUrl.Contains("betacinemas"))
                                currentPriority = 20;
                            else
                                currentPriority = 5; // Thấp cho các domain khác
                            
                            // Bonus cho format ảnh
                            if (lowerUrl.Contains(".png"))
                                currentPriority += 5;
                            else if (lowerUrl.Contains(".jpg") || lowerUrl.Contains(".jpeg"))
                                currentPriority += 5;
                            else if (lowerUrl.Contains(".webp"))
                                currentPriority += 3;
                            
                            // Bonus rất cao nếu có từ "poster" trong URL
                            if (lowerUrl.Contains("poster"))
                                currentPriority += 10;
                            
                            if (currentPriority > priority)
                            {
                                priority = currentPriority;
                                bestPosterUrl = posterUrl;
                            }
                        }
                    }
                    
                    // Cập nhật poster nếu tìm được ảnh tốt hơn
                    if (!string.IsNullOrEmpty(bestPosterUrl))
                    {
                        // Luôn cập nhật nếu tìm được ảnh từ files.betacorp.vn hoặc chưa có poster
                        if (string.IsNullOrEmpty(movie.poster) || 
                            bestPosterUrl.Contains("files.betacorp.vn") ||
                            (!movie.poster.Contains("files.betacorp.vn") && bestPosterUrl.Contains("files.betacorp.vn")))
                        {
                            movie.poster = bestPosterUrl;
                            Console.WriteLine($"Updated poster to: {bestPosterUrl} (Priority: {priority})");
                        }
                    }
                }
                else if (string.IsNullOrEmpty(movie.poster) || !movie.poster.Contains("files.betacorp.vn"))
                {
                    // Fallback: tìm bất kỳ ảnh nào từ files.betacorp.vn trong detail page
                    var anyImageNode = detailDoc.DocumentNode.SelectSingleNode("//img[contains(@src, 'files.betacorp.vn')]");
                    if (anyImageNode != null)
                    {
                        var posterUrl = anyImageNode.GetAttributeValue("src", "");
                        if (string.IsNullOrEmpty(posterUrl))
                        {
                            posterUrl = anyImageNode.GetAttributeValue("data-src", "");
                        }
                        if (string.IsNullOrEmpty(posterUrl))
                        {
                            posterUrl = anyImageNode.GetAttributeValue("data-lazy-src", "");
                        }
                        if (!string.IsNullOrEmpty(posterUrl))
                        {
                            // Loại bỏ icon/flags
                            var lowerUrl = posterUrl.ToLower();
                            if (!lowerUrl.Contains("/icons/") && 
                                !lowerUrl.Contains("united-kingdom") &&
                                !lowerUrl.Contains("flag") &&
                                !lowerUrl.Contains("/assets/common/"))
                            {
                                if (posterUrl.StartsWith("//"))
                                    movie.poster = "https:" + posterUrl;
                                else if (posterUrl.StartsWith("/"))
                                    movie.poster = "https://betacinemas.vn" + posterUrl;
                                else if (!posterUrl.StartsWith("http"))
                                    movie.poster = "https://betacinemas.vn/" + posterUrl;
                                else
                                    movie.poster = posterUrl;
                                Console.WriteLine($"Fallback poster from detail page: {movie.poster}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enhancing movie from detail page: {ex.Message}");
            }
        }

        private List<ScrapedMovieInfo> CreateSampleMovies()
        {
            return new List<ScrapedMovieInfo>
            {
                new ScrapedMovieInfo
                {
                    title = "Đào, phở và piano",
                    poster = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?auto=format&fit=crop&q=80&w=800",
                    description = "Bộ phim kể về cuộc sống của những người trẻ tại Hà Nội.",
                    duration = "2h 30m",
                    genre = "Drama",
                    basePrice = 45000
                },
                new ScrapedMovieInfo
                {
                    title = "Mai",
                    poster = "https://images.unsplash.com/photo-1626814026160-2237a95fc5a0?auto=format&fit=crop&q=80&w=800",
                    description = "Câu chuyện về tình yêu và gia đình.",
                    duration = "135 phút",
                    genre = "Romance",
                    basePrice = 100000,
                    detailUrl = "https://betacinemas.vn/phim.htm"
                },
                new ScrapedMovieInfo
                {
                    title = "Gặp lại chị bầu",
                    poster = "https://images.unsplash.com/photo-1478720568477-152d9b164e63?auto=format&fit=crop&q=80&w=800",
                    description = "Hài kịch về cuộc sống hiện đại.",
                    duration = "115 phút",
                    genre = "Comedy",
                    basePrice = 70000,
                    detailUrl = "https://betacinemas.vn/phim.htm"
                },
                new ScrapedMovieInfo
                {
                    title = "Tarot",
                    poster = "https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?auto=format&fit=crop&q=80&w=800",
                    description = "Bộ phim kinh dị về những lá bài bí ẩn.",
                    duration = "125 phút",
                    genre = "Horror",
                    basePrice = 90000,
                    detailUrl = "https://betacinemas.vn/phim.htm"
                }
            };
        }

        public async Task SaveMoviesToJsonAsync(List<ScrapedMovieInfo> movies)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(movies, options);
                await File.WriteAllTextAsync(MOVIES_JSON_FILE, json, Encoding.UTF8);
                Console.WriteLine($"Saved {movies.Count} movies to {MOVIES_JSON_FILE}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving movies to JSON: {ex.Message}");
            }
        }

        public List<ScrapedMovieInfo> LoadMoviesFromJson()
        {
            try
            {
                if (File.Exists(MOVIES_JSON_FILE))
                {
                    var json = File.ReadAllText(MOVIES_JSON_FILE, Encoding.UTF8);
                    var movies = JsonSerializer.Deserialize<List<ScrapedMovieInfo>>(json);
                    return movies ?? new List<ScrapedMovieInfo>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading movies from JSON: {ex.Message}");
            }
            return new List<ScrapedMovieInfo>();
        }
    }
}


