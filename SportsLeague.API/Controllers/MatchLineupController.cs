using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/match/{matchId}/lineup")]
    public class MatchLineupController : ControllerBase
    {
        private readonly IMatchLineupService _lineupService;
        private readonly IMapper _mapper;

        public MatchLineupController(
            IMatchLineupService lineupService,
            IMapper mapper)
        {
            _lineupService = lineupService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MatchLineupResponseDTO>> AddPlayer(
            int matchId, CreateMatchLineupDTO dto)
        {
            try
            {
                var lineup = _mapper.Map<MatchLineup>(dto);
                var created = await _lineupService.AddPlayerAsync(matchId, lineup);

                var fullLineup = await _lineupService.GetByMatchAsync(matchId);
                var createdEntry = fullLineup.First(l => l.Id == created.Id);
                var responseDto = _mapper.Map<MatchLineupResponseDTO>(createdEntry);

                return StatusCode(201, responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineup(
            int matchId)
        {
            try
            {
                var lineup = await _lineupService.GetByMatchAsync(matchId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByTeam(
            int matchId, int teamId)
        {
            try
            {
                var lineup = await _lineupService
                    .GetByMatchAndTeamAsync(matchId, teamId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemovePlayer(int matchId, int id)
        {
            try
            {
                await _lineupService.RemovePlayerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}