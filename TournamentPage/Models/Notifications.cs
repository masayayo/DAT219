using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class Notifications
    {
        public Notifications(){}

        public Notifications(string NotificationId, string NotificationTitle, string NotificationMessage, ApplicationUser User, DateTime Created)
        {
           this.NotificationTitle = NotificationTitle;
           this.NotificationMessage = NotificationMessage;
           this.User = User;
           this.Created = Created;
        }

        [Key]
        public int NotificationId {get;set;}

        public string NotificationTitle {get;set;}
        public string NotificationMessage {get;set;}
        public DateTime Created {get;set;}
        
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser User {get;set;}

    }
}