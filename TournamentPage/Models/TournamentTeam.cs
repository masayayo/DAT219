using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class TournamentTeam
    {
        public TournamentTeam(){}

        public TournamentTeam(Team Team, Tournament Tournament, DateTime TeamJoined)
        {
            this.Team = Team;
            this.Tournament = Tournament;
            this.TeamJoined = TeamJoined;
        }

        [Key]
        public int TournamentTeamId {get;set;}

        [ForeignKey("TeamId")]
        [Required]
        public Team Team {get;set;}

        [ForeignKey("TournamentId")]
        [Required]
        public Tournament Tournament {get;set;}

        public DateTime TeamJoined {get;set;}

        
    }
}