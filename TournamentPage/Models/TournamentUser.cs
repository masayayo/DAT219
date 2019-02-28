using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class TournamentUser
    {
        public TournamentUser(){}

       public TournamentUser(ApplicationUser User, Tournament Tournament, DateTime Joined)
        {  
            this.User = User;
            this.Tournament = Tournament;
            this.Joined = Joined;
        }

        [Key]
        public int TournamentUserId {get;set;}

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser User {get;set;}

        [ForeignKey("TournamentId")]
        public Tournament Tournament {get;set;}

        public DateTime Joined {get;set;}      
    }
}