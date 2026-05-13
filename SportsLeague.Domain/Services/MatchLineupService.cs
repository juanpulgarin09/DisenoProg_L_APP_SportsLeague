using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class MatchLineupService : IMatchLineupService
    {
        private readonly IMatchLineupRepository _lineupRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly MatchValidationHelper _matchValidationHelper;
        private readonly ILogger<MatchLineupService> _logger;

        public MatchLineupService(
            IMatchLineupRepository lineupRepository,
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            MatchValidationHelper matchValidationHelper,
            ILogger<MatchLineupService> logger)
        {
            _lineupRepository = lineupRepository;
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _matchValidationHelper = matchValidationHelper;
            _logger = logger;
        }

        public async Task<MatchLineup> AddPlayerAsync(int matchId, MatchLineup lineup)
        {
            // El partido existe
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");

            // El partido debe estar en estado Scheduled
            // (las alineaciones se registran ANTES de que empiece el partido)
            if (match.Status != MatchStatus.Scheduled)
                throw new InvalidOperationException(
                    "Solo se pueden registrar alineaciones en partidos Scheduled");

            // El jugador existe
            var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);
            if (player == null)
                throw new KeyNotFoundException(
                    $"No se encontró el jugador con ID {lineup.PlayerId}");

            // El jugador pertenece a uno de los dos equipos del partido
            if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
                throw new InvalidOperationException(
                    "El jugador no pertenece a ninguno de los equipos del partido");

            // El jugador no puede estar dos veces en la misma alineación
            var alreadyExists = await _lineupRepository
                .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);
            if (alreadyExists)
                throw new InvalidOperationException(
                    "El jugador ya está registrado en la alineación de este partido");

            // Máximo 11 titulares por equipo
            // Solo aplica si el jugador que se agrega es titular
            if (lineup.IsStarter)
            {
                var starterCount = await _lineupRepository
                    .CountStartersByMatchAndTeamAsync(matchId, player.TeamId);

                if (starterCount >= 11)
                    throw new InvalidOperationException(
                        "El equipo ya tiene 11 titulares registrados en este partido");
            }

            lineup.MatchId = matchId;

            _logger.LogInformation(
                "Adding player {PlayerId} to lineup of match {MatchId} as {Role}",
                lineup.PlayerId, matchId,
                lineup.IsStarter ? "Starter" : "Substitute");

            return await _lineupRepository.CreateAsync(lineup);
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
        {
            // Verificar que el partido existe
            var matchExists = await _matchRepository.ExistsAsync(matchId);
            if (!matchExists)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");

            return await _lineupRepository.GetByMatchAsync(matchId);
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(
            int matchId, int teamId)
        {
            var matchExists = await _matchRepository.ExistsAsync(matchId);
            if (!matchExists)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");

            return await _lineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
        }

        public async Task RemovePlayerAsync(int lineupId)
        {
            var exists = await _lineupRepository.ExistsAsync(lineupId);
            if (!exists)
                throw new KeyNotFoundException(
                    $"No se encontró la entrada de alineación con ID {lineupId}");

            _logger.LogInformation(
                "Removing lineup entry with ID: {LineupId}", lineupId);

            await _lineupRepository.DeleteAsync(lineupId);
        }
    }
}