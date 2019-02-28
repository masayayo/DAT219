using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class Brackets
    {
        public Brackets(){}

        public Brackets(int Round, int Bracket, string TeamName, string Score)
        {
            this.Round = Round;
            this.Bracket = Bracket;
            this.TeamName = TeamName;
            this.Score = Score;
        }

        [Key]
        public int BracketId {get;set;}
        public int Round {get;set;}
        public int Bracket {get;set;}
        public string TeamName {get;set;}
        public string Score {get;set;}

    }
}