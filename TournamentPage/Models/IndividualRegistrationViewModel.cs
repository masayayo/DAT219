// Model is sent into a Razor view. It is used when there only is individual registration
// for a tournament. 
namespace TournamentPage.Models
{
    public class IndividualRegistrationViewModel
    {
        public IndividualRegistrationViewModel(){}
        public ApplicationUser User {get;set;}
        public Tournament Tournament {get; set;}
    }
}