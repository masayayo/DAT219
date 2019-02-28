using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TournamentPage.Models.TeamViewModels
{

    public class TeamViewModel
    {
        public int TeamId {get;set;}
        public List<TeamUser> Players {get;set;}
        public List<ApplicationUser> Users {get;set;}
        public string[] NewPlayers {get;set;}
        
    }

}