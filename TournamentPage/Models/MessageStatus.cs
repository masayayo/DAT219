/* Used to determine which message content we shoul send back to the view to
notify the user about the change went well or not */

namespace TournamentPage.Models
{
    public enum MessageStatus
    {
        CreatedTournamentSuccess,
        CreatedTournamentFailure,
        EditTournamentSuccess,
        EditTournamentFailure,
        DeletedTournamentSuccess,
        DeletedTournamentfailure,
        DeletedTeamSuccess,
        DeletedTeamFailure,

        ChangePasswordSuccess,
        ChangePasswordFailure,

        DeletedUserSuccess,
        DeletedUserFailure,

        IndividualRegistrationSuccess,
        TeamRegistrationSuccess,

        RemoveTournamentTeamRegistrationSuccess,
        RemoveIndividualRegistrationSuccess,
        
        Success, /* General ones used to determine the color of the alert box  */
        Failure
        /* expand the list here, e.g with RegisterSuccess etc */
    }
}