using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class Matchups
    {
        public Matchups(){}

        public Matchups(int Phase, TournamentTeam Visitor, TournamentTeam Local, int ScoreVisitor, int ScoreLocal)
        {
            this.Phase = Phase;
            this.Visitor = Visitor;
            this.Local = Local;
            this.ScoreVisitor = 0;
            this.ScoreLocal = 0;
        }

        [Key]
        public int MatchupId {get;set;}
        [ForeignKey("TournamentTeamVisitorId")]
        public int Phase {get;set;}
        public TournamentTeam Visitor {get;set;}
        [ForeignKey("TournamentTeamLocalId")]
        public TournamentTeam Local {get;set;}
        public int ScoreVisitor {get;set;}
        public int ScoreLocal {get;set;}
    }
}