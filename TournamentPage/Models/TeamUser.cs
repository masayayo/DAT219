using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class TeamUser
    {
        public TeamUser(){}

        public TeamUser(ApplicationUser User, Team Team, DateTime Joined)
        {  
            this.User = User;
            this.Team = Team;
            this.Joined = Joined;
        }

        [Key]
        public int TeamUserId {get;set;}

        [ForeignKey("ApplicationUserId")]
        [Required]
        public ApplicationUser User {get;set;}
        
        [ForeignKey("TeamId")]
        [Required]
        public Team Team {get;set;}

        public DateTime Joined {get;set;}
    }
}