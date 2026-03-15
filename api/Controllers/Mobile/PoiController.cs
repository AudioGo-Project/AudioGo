using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories.Interfaces;
using Shared;

namespace Server.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/pois")]
    public class PoiController : ControllerBase
    {
        private readonly IPoiRepository _repo;
        public PoiController(IPoiRepository repo) => _repo = repo;

        // GET /api/mobile/pois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll() =>
            Ok((await _repo.GetAllAsync()).Select(ToDto));

        // GET /api/mobile/pois/nearby?lat=10.72&lon=106.70&radius=500
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<POI>>> GetNearby(
            [FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radius = 500)
        {
            var pois = await _repo.GetNearbyAsync(lat, lon, radius);
            return Ok(pois.Select(ToDto));
        }

        // GET /api/mobile/pois/{poiId}
        [HttpGet("{poiId}")]
        public async Task<ActionResult<POI>> GetById(string poiId)
        {
            var poi = await _repo.GetByIdAsync(poiId);
            return poi is null ? NotFound() : Ok(ToDto(poi));
        }

        private static POI ToDto(Poi p) => new()
        {
            PoiId            = p.PoiId,
            Latitude         = p.Latitude,
            Longitude        = p.Longitude,
            ActivationRadius = p.ActivationRadius,
            Priority         = p.Priority,
            Status           = p.Status ?? string.Empty,
            LogoUrl          = p.LogoUrl ?? string.Empty
        };
    }
}
