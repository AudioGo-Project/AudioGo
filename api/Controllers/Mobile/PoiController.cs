using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories.Interfaces;
using Server.Services.Interfaces;
using Shared;

namespace Server.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/pois")]
    public class PoiController : ControllerBase
    {
        private readonly IPoiRepository _repo;
        private readonly IContentPipelineService _pipeline;

        public PoiController(IPoiRepository repo, IContentPipelineService pipeline)
        {
            _repo = repo;
            _pipeline = pipeline;
        }

        // GET /api/mobile/pois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll(
            [FromQuery] string lang = "vi")
        {
            var pois = await _repo.GetAllAsync();
            var dtos = new List<POI>();
            foreach (var p in pois)
                dtos.Add(await ToDtoAsync(p, lang));
            return Ok(dtos);
        }

        // GET /api/mobile/pois/nearby?lat=10.72&lon=106.70&radius=500
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<POI>>> GetNearby(
            [FromQuery] double lat, [FromQuery] double lon,
            [FromQuery] double radius = 500, [FromQuery] string lang = "vi")
        {
            var pois = await _repo.GetNearbyAsync(lat, lon, radius);
            var dtos = new List<POI>();
            foreach (var p in pois)
                dtos.Add(await ToDtoAsync(p, lang));
            return Ok(dtos);
        }

        // GET /api/mobile/pois/{poiId}
        [HttpGet("{poiId}")]
        public async Task<ActionResult<POI>> GetById(
            string poiId, [FromQuery] string lang = "vi")
        {
            var poi = await _repo.GetByIdAsync(poiId);
            return poi is null ? NotFound() : Ok(await ToDtoAsync(poi, lang));
        }

        private async Task<POI> ToDtoAsync(Poi p, string lang)
        {
            var content = await _pipeline.EnsureContentAsync(p, lang);
            return new()
            {
                PoiId            = p.PoiId,
                Latitude         = p.Latitude,
                Longitude        = p.Longitude,
                ActivationRadius = p.ActivationRadius,
                Priority         = p.Priority,
                Status           = p.Status   ?? string.Empty,
                LogoUrl          = p.LogoUrl  ?? string.Empty,
                LanguageCode     = content.LanguageCode,
                Title            = content.Title,
                Description      = content.Description,
                AudioUrl         = content.AudioUrl,
            };
        }
    }
}
