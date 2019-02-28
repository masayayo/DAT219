
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace TournamentPage.Models
{
    public class UserProfile
    {
        public UserProfile(){}
        public IList<TournamentTeam> UsersParticipatedTournaments {get; set;}
        public IList<Tournament> UsersMadeTournaments {get; set;}
        public IList<TeamUser> UsersTeams {get; set;}
    }
}