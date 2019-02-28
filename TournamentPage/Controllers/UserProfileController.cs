using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TournamentPage.Data;
using TournamentPage.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http.Headers;

namespace TournamentPage.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        /* The dependencies */
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;

        private IHostingEnvironment _hostingEnv;
    
        /* Constructor */
        public UserProfileController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHostingEnvironment hostingEnv)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
             _hostingEnv = hostingEnv;
        }

        /* url: /UserProfile/ShowProfile  */
        /* This action finds the user data. The data includes 
        which teams and tournaments the user has participated on. */
        [HttpGet]
        public async Task<IActionResult> ShowUserProfile(MessageStatus? message = null)
        {
            /* A instance of the model */
            UserProfile up = new UserProfile();
            /* Gets the logged in user */
            ApplicationUser LoggedInUser = userManager.GetUserAsync(User).Result;
            /* If something is wrong we logout the user and redirect him to the login action again in the account
            controller together with the url to this action. */
            if(LoggedInUser == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/UserProfile/{nameof(UserProfileController.ShowUserProfile)}"});
            }
            /* Get the logged in user own made tournaments */
            up.UsersMadeTournaments = db.Tournament.Where(u => u.User.Id == LoggedInUser.Id).ToList();
             /* Get the team the user participate on */
            up.UsersTeams = db.TeamUser.Include(x => x.Team).ThenInclude(u => u.User).Where(u => u.User.Id == LoggedInUser.Id).ToList();
            /* We want to have a list of TournamentTeam for this user s.t we can map the users tournament registrations with his tournamnet registration type, i.e his team registration name or individual registration. 
            If the team component in the list is null the user has performed an individual registration,
            and if not (s)he is part of a team registration and hence a team. We use the TournamentTeam object because it has all the information of both the tournament and the team if it is a team registration.
            This is important to have if the user wants to delete his/her tournament registration.*/
            // 1. Get all the tournaments the user participate on:
            IList<TournamentUser> TournamentsUserParticipateOn = db.TournamentUser.Include(t => t.Tournament).Where(u => u.User.Id == LoggedInUser.Id).ToList();
            // 2. Get all the tournament team the user participate on:
            IList<TournamentTeam> TournamentTeamsUserParticipateOn = new List<TournamentTeam>(); 
            IList<TournamentTeam> AllTournamentTeams = db.TournamentTeam.Include(t => t.Team).Include(t => t.Tournament).ToList();
            // Iterate through all of the users teams
            for(int i=0; i<up.UsersTeams.Count;i++)
            {
                // and for each user team compare it against each of the tournament teams.
                for(int j=0; j<AllTournamentTeams.Count;j++)
                {
                    // if the team id is equal - the team of the user is also a tournament team.
                    if(up.UsersTeams.ElementAt(i).Team.TeamId == AllTournamentTeams.ElementAt(j).Team.TeamId)
                    {
                        TournamentTeamsUserParticipateOn.Add(AllTournamentTeams.ElementAt(j));
                    }
                }
            }
            // 3. We want to map the users tournamnet registration to the users tournament team or to an individual tournament registration.
            // To get this differensiation correct we compare each of the tournaments the user participate on 
            // to each of the tournament teams. If they are equal we add the tournamentteam to a new list, and if not we set the team 
            // component in tournament team to null. We test on this component in the view. 
            up.UsersParticipatedTournaments =  new List<TournamentTeam>();
            for(int i=0; i<TournamentsUserParticipateOn.Count; i++)
            {
                int flag = 0;
                for(int j=0; j<TournamentTeamsUserParticipateOn.Count; j++)
                {
                    if(TournamentsUserParticipateOn.ElementAt(i).Tournament.TournamentId == TournamentTeamsUserParticipateOn.ElementAt(j).Tournament.TournamentId)
                    {
                        // We include the TournamentTeam object
                        up.UsersParticipatedTournaments.Add(TournamentTeamsUserParticipateOn.ElementAt(j));
                        flag = 1;
                        break;
                    }
                }
                // The user is not participating on any team (been no break), hence (s)he is registrated individually for the tournament.
                if(flag==0)
                {
                    TournamentTeam tt = new TournamentTeam();
                    tt.Team = null; /* We test on this attribute in the view. If it is null it is a individual tournament registration  */
                    tt.Tournament = TournamentsUserParticipateOn.ElementAt(i).Tournament;
                    up.UsersParticipatedTournaments.Add(tt);
                }
            } 
            /* Get the name of the user */
            ViewBag.UserName = $"{LoggedInUser.FirstName} {LoggedInUser.LastName}";
            /* Get the user id */
            ViewBag.UserId = LoggedInUser.Id;
            /* Get the user profile picture */
            ViewBag.ProfilePicture = LoggedInUser.ProfilePicture;
            /* We want to notify the user if the password change has been succesfully in the Manage/ChangPassword action */
            if(message!=null)
            {
                if(message == MessageStatus.ChangePasswordSuccess)
                {
                    ViewData["StatusMessage"] = "Suksessfullt passordbytte";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.ChangePasswordFailure)
                {
                    ViewData["StatusMessage"] = "Passordbyttet feilet";
                    ViewData["Success"] = MessageStatus.Failure;
                }
                else if(message == MessageStatus.DeletedTournamentSuccess)
                {
                    ViewData["StatusMessage"] = "Turneringen ble slettet";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.DeletedTournamentfailure)
                {
                    ViewData["StatusMessage"] = "Turneringen ble ikke slettet";
                    ViewData["Success"] = MessageStatus.Failure;
                }
                else if(message == MessageStatus.DeletedUserFailure) /* En suksessfull brukersletting kommer i et annet view (Account/Login) */
                {
                    ViewData["StatusMessage"] = "Brukeren din ble ikke slettet";
                    ViewData["Success"] = MessageStatus.Failure;
                }
                else if(message == MessageStatus.CreatedTournamentSuccess) 
                {
                    ViewData["StatusMessage"] = "Turneringen ble opprettet";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.EditTournamentSuccess)
                {
                    ViewData["StatusMessage"] = "Turneringen ble redigert suksessfult";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.IndividualRegistrationSuccess)
                {
                    ViewData["StatusMessage"] = "Du ble suksessfult påmeldt turneringen";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.TeamRegistrationSuccess)
                {
                    ViewData["StatusMessage"] = "Laget ditt ble suksessfult påmeldt turneringen";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.RemoveTournamentTeamRegistrationSuccess)
                {
                    ViewData["StatusMessage"] = "Laget ditt ble suksessfult avmeldt turneringen";
                    ViewData["Success"] = MessageStatus.Success;
                }
                else if(message == MessageStatus.RemoveIndividualRegistrationSuccess)
                {
                    ViewData["StatusMessage"] = "Du ble suksessfult avmeldt turneringen";
                    ViewData["Success"] = MessageStatus.Success;
                }
            }
            /* Return url. */
            ViewData["returnurl"] = $"/UserProfile/ShowUserProfile";
            return View(up);
        }

        /* url: /UserProfile/SeeUserProfileDetails */
        /* This action finds the user, calculate the age of the user based on his
        birthdate and return all of it to the corresponding view. */
        [HttpGet]
        public async Task<IActionResult> SeeUserProfileDetails(string id)
        {
            /* Finds the user to delete */
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if(user == null) /* something went wrong */
            {
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/UserProfile/{nameof(UserProfileController.ShowUserProfile)}"});
            }
            /*  Calculates the age */
            var Age = DateTime.Now.Date.Subtract( (DateTime)user.BirthDate);
            ViewBag.Age = (int)Age.TotalDays/365;

            return View(user);
        }

        /* url: /UserProfile/DeleteUserProfile/ */
        /* Finds and returns the user to the view */
        [HttpGet]
        public async Task<IActionResult> DeleteUserProfile(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if(user == null) /* something went wrong */
            {
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/UserProfile/{nameof(UserProfileController.ShowUserProfile)}"});
            }
            return View(user);
        }

        /* url: /UserProfile/DeleteUserProfile2/ */
        /* Get the user, remove him from his role before he get deleted. When the user is deleted he also get
        deletd from all the other tables in the database because of the [Required] attributte force cascade deleting. */
        [HttpPost]
        public async Task<IActionResult> DeleteUserProfile2(string id)
        {  
            /* Finds the user */
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if(user!=null)
            {
                /* Delete the user from each of its role */
                /*var roles = await userManager.GetRolesAsync(user);
                foreach(var r in roles)
                {
                    await userManager.RemoveFromRoleAsync(user, r);
                }
                var result = await userManager.DeleteAsync(user);
                /* Delete the user */
                /*
                if(result.Succeeded)
                    return RedirectToAction(nameof(AccountController.Login),"Account"); /* success */
                //else /* something went wrong */
                    //return RedirectToAction(nameof(UserProfileController.ShowUserProfile),"UserProfile");
                
                /* We have to delete sensitive user information and still 
                keep the anonymized user in the database to keep the other database tables 
                consistent. */
                user.AccessFailedCount = 0;
                user.ConcurrencyStamp = null; /* prevents concurrecy conflicts. Will be updated when somebody change the user data. Actually no point in setting it to null. */
                user.Email = null;
                user.EmailConfirmed = false;
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
                user.NormalizedEmail = null;
                user.NormalizedUserName = null;
                user.PasswordHash = null;
                user.PhoneNumber = null;
                user.PhoneNumberConfirmed = false;
                user.SecurityStamp = null;
                user.TwoFactorEnabled = false;
                user.UserName = "SlettetBruker"; /* can only contain letters or digits */
                user.FirstName = null;
                user.LastName = null;
                user.NickName = null;
                user.BirthDate = null;
                user.PostalCode = null;
                user.Town = null;
                user.Address = null;
                user.RegisterDate = null;
                /* Since we change indiviudual attributtes of the user this method has be called to save the changes */
                var result = await userManager.UpdateAsync(user);   
                /* Success */
                if(result.Succeeded)
                {
                    await signInManager.SignOutAsync();
                    return RedirectToAction(nameof(AccountController.Login),"Account", new {message = MessageStatus.DeletedUserSuccess});
                }
                /* Something went wrong */
                else
                    return RedirectToAction(nameof(UserProfileController.ShowUserProfile),"UserProfile", new {message = MessageStatus.DeletedUserFailure});
            }
            else /* Something went wrong */
            {
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/UserProfile/{nameof(UserProfileController.ShowUserProfile)}", message = MessageStatus.DeletedUserFailure});
            }
        }

        /* url: /UserProfile/EditUserProfile/ */
        /* Get the user data and populate some of it's attributtes in a view.  
        We do not give the user a chance to change every value in the user model. */
        [HttpGet]
        public async Task<IActionResult> EditUserProfile(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if(user == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(UserProfileController.ShowUserProfile),"UserProfile");
            }
            return View(user);
        }

        /* url: /UserProfile/EditUserProfile/ */
        /* Recives the new or unchnaged values from the populated form.  */
        [HttpPost]
        public async Task<IActionResult> EditUserProfile
        (
            /* We want to set the required attribute here and not in the ApplicationUser
            model because then we cannot null them out in the delete action! */
            /*[Required]int PostalCode,
            [Required]string Address,
            [Required]string Town,
            [Required]string NickName,
            [Required]string Email,
            [Required]string PhoneNumber*/
            [Bind("PostalCode", "Address", "Town", "NickName", "Email", "PhoneNumber")] ApplicationUser u
        )
        {
            if(u.PostalCode == null)
            {
                ModelState.AddModelError(nameof(u.PostalCode),"Må ha et gyldig postnummer");
            }
            if(string.IsNullOrEmpty(u.Address))
            {
                ModelState.AddModelError(nameof(u.Address),"Adresse mangler");
            }
            if(string.IsNullOrEmpty(u.Town))
            {
                ModelState.AddModelError(nameof(u.Town),"Byen du bor i eller byen du bor nærmest");
            }
            if(string.IsNullOrEmpty(u.NickName))
            {
                ModelState.AddModelError(nameof(u.NickName),"Du må gi deg selv et kult kallenavn");
            }
            if(string.IsNullOrEmpty(u.Email))
            {
                ModelState.AddModelError(nameof(u.Email),"Mangler epostadresse");
            }
            if(string.IsNullOrEmpty(u.PhoneNumber))
            {
                ModelState.AddModelError(nameof(u.PhoneNumber),"Mangler telefnnummer");
            }
            if(ModelState.IsValid)
            {
                /* Gets the logged in user and updates the attributtes of the user */
                ApplicationUser user = await userManager.GetUserAsync(User);
                /* Set the new attributte values */
                user.PostalCode = u.PostalCode;
                user.Address = u.Address;
                user.Town = u.Town;
                user.NickName = u.NickName;
                user.Email = u.Email;
                user.PhoneNumber = u.PhoneNumber;
                /* We must call the UpdateAsync method since we only update some of the attributtes. */
                await userManager.UpdateAsync(user);
                return RedirectToAction(nameof(UserProfileController.SeeUserProfileDetails), "UserProfile", new {id= user.Id });
            }
            else
            {
                return View(u);
            }      
        }
        
      /* Uploads a selected profile picture to the server images folder  */
      [HttpPost]
      public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            /* Get the currently logged in user */
            ApplicationUser LoggedInUser = userManager.GetUserAsync(User).Result;
            
            /* Get the images path */
             var uploads = Path.Combine(_hostingEnv.WebRootPath, "images");

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    /* Get the extension of the file and create a name unique to the logged in user */
                    string ext = file.FileName.Substring(file.FileName.IndexOf(".")).Trim();
                    string picName = "pic"+LoggedInUser.Id+ext;

                    /* Create a filestream that will stream the file to the specific path with a new name for the picture */
                    using (var fileStream = new FileStream(Path.Combine(uploads, picName), FileMode.Create))
                    {

                        /* Copy the file to the ~/images/ folder. The await is necessary for the image to be fully uploaded */
                       await file.CopyToAsync(fileStream);

                       /* Update the LoggedInUser's profile picture path in the database */
                       LoggedInUser.ProfilePicture = "/"+picName;
                       db.SaveChanges();
                    }
                }
            }
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile),"UserProfile");
        }
    }
}