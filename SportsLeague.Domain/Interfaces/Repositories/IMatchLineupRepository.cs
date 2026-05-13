using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface IMatchLineupRepository : IGenericRepository<MatchLineup>
    {
        // Traer toda la alineación de un partido con datos del jugador
        Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId);

        // Traer la alineación de un equipo específico en un partido
        Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(
            int matchId, int teamId);

        // Verificar si un jugador ya está en la alineación de ese partido
        Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId);

        // Contar cuántos titulares tiene un equipo en un partido
        Task<int> CountStartersByMatchAndTeamAsync(int matchId, int teamId);
    }
}