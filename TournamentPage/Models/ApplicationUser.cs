using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TournamentPage.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string NickName {get;set;}
        
        /*[RegularExpression(@"^([0-9a-zA-Z_\-~ :\\])+(.jpg|.JPG|.jpeg|.JPEG|.png|.PNG)$", ErrorMessage = "Denne filtypen er ikke tillatt")]*/
        public string ProfilePicture {get;set;}
        public DateTime? BirthDate {get;set;}
        [RegularExpression ("^[0-9]{4,4}$", ErrorMessage = "Postnummer skal være et firesifret tall")]
        public string PostalCode {get;set;}
        public string Town {get;set;}
        public string Address {get;set;}
        public DateTime? RegisterDate {get;set;}
        public string Gender {get;set;}
        
        public ApplicationUser()
        {
            this.ProfilePicture = "/default.jpg";
        }

        // UserId, Phonenumber, Email and Password (hash) are already included in the default database.

        

    }
}
