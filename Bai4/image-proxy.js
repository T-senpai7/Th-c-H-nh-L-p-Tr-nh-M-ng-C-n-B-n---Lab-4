// Image Proxy Server - Express.js
// Chạy server này để proxy images từ betacorp.vn
// Usage: node image-proxy.js

const express = require('express');
const fetch = require('node-fetch');
const app = express();
const PORT = 3001; // Port khác với server C# (8888)

// CORS middleware
app.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET, OPTIONS');
    res.header('Access-Control-Allow-Headers', 'Content-Type');
    
    if (req.method === 'OPTIONS') {
        return res.sendStatus(200);
    }
    next();
});

// Proxy image endpoint
app.get('/api/proxy-image', async (req, res) => {
    try {
        const imageUrl = req.query.url;
        
        if (!imageUrl) {
            return res.status(400).json({ error: 'Missing url parameter' });
        }

        // Decode URL nếu đã bị encode
        let decodedUrl = imageUrl;
        try {
            decodedUrl = decodeURIComponent(imageUrl);
        } catch (e) {
            // Nếu decode thất bại, thử decode nhiều lần
            let temp = imageUrl;
            for (let i = 0; i < 5; i++) {
                try {
                    temp = decodeURIComponent(temp);
                    if (!temp.includes('%2f') && !temp.includes('%2F')) {
                        break;
                    }
                } catch (err) {
                    break;
                }
            }
            decodedUrl = temp;
        }

        console.log(`Proxying image: ${decodedUrl}`);

        // Fetch image với headers phù hợp
        const response = await fetch(decodedUrl, {
            headers: {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
                'Referer': 'https://www.betacinemas.vn/',
                'Accept': 'image/webp,image/apng,image/*,*/*;q=0.8',
                'Accept-Language': 'vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7'
            },
            timeout: 10000 // 10 seconds timeout
        });

        if (!response.ok) {
            console.error(`Failed to fetch image: ${response.status} ${response.statusText}`);
            return res.status(response.status).json({ 
                error: `Failed to fetch image: ${response.statusText}` 
            });
        }

        // Detect content type từ URL extension (ưu tiên) hoặc response headers
        let contentType = 'image/jpeg'; // default
        
        // Ưu tiên detect từ URL extension để giữ nguyên format gốc
        const lowerUrl = decodedUrl.toLowerCase();
        if (lowerUrl.includes('.png')) {
            contentType = 'image/png';
        } else if (lowerUrl.includes('.jpg') || lowerUrl.includes('.jpeg')) {
            contentType = 'image/jpeg';
        } else if (lowerUrl.includes('.webp')) {
            contentType = 'image/webp';
        } else if (lowerUrl.includes('.gif')) {
            contentType = 'image/gif';
        } else {
            // Fallback: lấy từ response headers nếu không detect được từ URL
            const responseContentType = response.headers.get('content-type');
            if (responseContentType && responseContentType.startsWith('image/')) {
                contentType = responseContentType;
            }
        }
        
        // Get image buffer
        const buffer = Buffer.from(await response.arrayBuffer());

        // Set headers và send image - giữ nguyên format gốc
        res.set('Content-Type', contentType);
        res.set('Cache-Control', 'public, max-age=86400'); // Cache 1 ngày
        res.set('Content-Length', buffer.length);
        res.send(buffer);

    } catch (error) {
        console.error('Error proxying image:', error);
        res.status(500).json({ 
            error: 'Internal server error',
            message: error.message 
        });
    }
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'ok', service: 'image-proxy' });
});

// Start server
app.listen(PORT, () => {
    console.log(`Image Proxy Server running on http://localhost:${PORT}`);
    console.log(`Proxy endpoint: http://localhost:${PORT}/api/proxy-image?url=<encoded-image-url>`);
});

