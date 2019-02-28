using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class InvitationsReserved
    {
        public InvitationsReserved(){}

        public InvitationsReserved(string NotExistingUserEmail, DateTime Invited, int Accepted, ApplicationUser ExistingUser, Team TeamSlotReserved)
        {
            this.NotExistingUserEmail = NotExistingUserEmail;
            this.Invited = Invited;
            this.Accepted = Accepted;
            this.ExistingUser = ExistingUser;
            this.TeamSlotReserved = TeamSlotReserved;
        }

        [Key]
        public int InvitationsReservedId {get;set;}
        // Email for the user if the user invited is not an existing one. An email should be sent to this address for him to make an account
        public string NotExistingUserEmail{get;set;}
        // DateTime for when the invitation was made
        public DateTime Invited {get;set;}
        // Tells if the invitation has been accepted or not. 0 = NO, 1 = YES
        public int Accepted {get;set;}
        // UserId of an already existing user
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ExistingUser {get;set;}
        // The team whose slot is gonna be reserved
        [ForeignKey("TeamId")]
        public Team TeamSlotReserved {get;set;}
        
        
    }
}