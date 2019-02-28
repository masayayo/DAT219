using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentPage.Models.AccountViewModels
{
    public class RegisterViewModel
    {

        
        [Required(ErrorMessage = "Obligatorisk felt")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Obligatorisk felt")]
        [StringLength(100, ErrorMessage = " {0} må ha minst {2} og max {1} antall karakterer.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bekreft Passord")]
        [Compare("Password", ErrorMessage = "Passordene er ikke like.")]
        [Required(ErrorMessage = "Obligatorisk felt")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Obligatorisk felt")]
         public string FirstName { get; set; }

        [Required(ErrorMessage = "Obligatorisk felt")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Obligatorisk felt")]
        public string NickName {get;set;}

        [Required(ErrorMessage = "Obligatorisk felt")]
        public DateTime BirthDate {get;set;}

        [Required(ErrorMessage = "Obligatorisk felt")]
        [RegularExpression ("^[0-9]{4,4}$", ErrorMessage = "Postnummer skal være et firesifret tall")]  
        public string PostalCode {get;set;}

        [Required(ErrorMessage = "Obligatorisk felt")]
        public string Town {get;set;}

        [Required(ErrorMessage = "Obligatorisk felt")]
        public string Address {get;set;}
        
        [Required(ErrorMessage = "Obligatorisk felt")]
        public string PhoneNumber {get;set;}
        
        [Required(ErrorMessage = "Obligatorisk felt")]
        public string Gender {get;set;}
        
        [Range(typeof(bool), "true", "true", ErrorMessage="Du må akseptere vilkårene!")]
        public bool TermsAccepted {get; set;}

        public DateTime? RegisterDate {get;set;}

    }
}
