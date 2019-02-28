using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentPage.Models.ManageViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Fyll inn det nåværende passordet")]
        [DataType(DataType.Password)]
        [Display(Name = "Nåværende passord")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Fyll inn et nytt passord")]
        [StringLength(100, ErrorMessage = "Det nye passordet må være minst {2} og maks {1} tegn langt.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nytt passord")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bekreft nytt passord")]
        [Compare("NewPassword", ErrorMessage = "Det bekreftede passordet samstemmer ikke med det nye")]
        public string ConfirmPassword { get; set; }
    }
}
