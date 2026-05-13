using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services
{
    public interface IMatchLineupService
    {
        // Agregar un jugador a la alineación
        Task<MatchLineup> AddPlayerAsync(int matchId, MatchLineup lineup);

        // Obtener alineación completa del partido
        Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId);

        // Obtener alineación de un equipo específico
        Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(
            int matchId, int teamId);

        // Eliminar un jugador de la alineación
        Task RemovePlayerAsync(int lineupId);
    }
}