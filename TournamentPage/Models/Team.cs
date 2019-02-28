using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class Team
    {
        public Team(){}

        public Team(string TeamName, DateTime TeamRegisterDate, DateTime TeamModifiedDate, ApplicationUser ContactPerson)
        {
            this.TeamName = TeamName;
            this.TeamRegisterDate = TeamRegisterDate;
            this.TeamModifiedDate = TeamModifiedDate;
            this.User = ContactPerson;
        }

        [Key]
        public int TeamId {get;set;}

        public string TeamName {get;set;}
        public DateTime TeamRegisterDate {get;set;}
        public DateTime TeamModifiedDate {get;set;}

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser User {get;set;}
        
    }
}