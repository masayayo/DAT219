using System.Collections.Generic;

// Model is sent into a Razor view. It is used when there is team
// registration, and a team or individual registration for a tournament. 
namespace TournamentPage.Models
{
    public class TeamRegistrationViewModel
    {
        public TeamRegistrationViewModel(){}
        public IndividualRegistrationViewModel IndividualRegistrationViewModel {get;set;}
        public IList<Team> Teams {get; set;}
    }
}