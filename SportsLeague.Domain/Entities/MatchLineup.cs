namespace SportsLeague.Domain.Entities
{
    public class MatchLineup : AuditBase
    {
        // Foreign Keys
        public int MatchId { get; set; }    // FK al partido
        public int PlayerId { get; set; }   // FK al jugador

        public bool IsStarter { get; set; } // true=Titular, false=Suplente
        public string Position { get; set; } = string.Empty; // "GK","CB","ST", etc.

        // Navigation Properties
        public Match Match { get; set; } = null!;
        public Player Player { get; set; } = null!;
    }
}