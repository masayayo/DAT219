using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentPage.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage="Email må spesifiseres")]
        [EmailAddress(ErrorMessage = "Email adressen er ikke gyldig")]
        public string Email { get; set; }

        [Required(ErrorMessage="Passord må spesifiseres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Husk meg?")]
        public bool RememberMe { get; set; }
    }
}
