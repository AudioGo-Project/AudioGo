using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Repositories.Interfaces;
using Shared.DTOs;

namespace Server.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IListenHistoryRepository _history;
        private readonly ILocationLogRepository   _location;
        private readonly AppDbContext _db;

        public AnalyticsController(
            IListenHistoryRepository history,
            ILocationLogRepository location,
            AppDbContext db)
        {
            _history  = history;
            _location = location;
            _db       = db;
        }

        /// <summary>Top N POI được nghe nhiều nhất.</summary>
        [HttpGet("top-pois")]
        public async Task<ActionResult<List<TopPoiDto>>> GetTopPois(
            [FromQuery] int top = 10)
        {
            var topPois = await _history.GetTopPoisAsync(top);

            // Enrich với title từ PoiContent (ngôn ngữ mặc định vi)
            var result = new List<TopPoiDto>();
            foreach (var (poiId, count) in topPois)
            {
                var title = await _db.PoiContents.AsNoTracking()
                    .Where(c => c.PoiId == poiId && (c.IsMaster || c.LanguageCode == "vi"))
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync() ?? poiId;

                result.Add(new TopPoiDto(poiId, title, count));
            }
            return Ok(result);
        }

        /// <summary>Heatmap vị trí người dùng (nhóm theo ô ~100m).</summary>
        [HttpGet("heatmap")]
        public async Task<ActionResult<List<HeatmapPointDto>>> GetHeatmap()
        {
            var points = await _location.GetHeatmapAsync();
            return Ok(points.Select(p => new HeatmapPointDto(p.Lat, p.Lon, p.Count)));
        }
    }
}
