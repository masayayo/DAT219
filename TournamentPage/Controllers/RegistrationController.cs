using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentPage.Data;
using TournamentPage.Models;


// This controller registrates users and teams to the different tournaments listed in
// the tournament list. The actions is divided in three groups; individual 
// registrations, team registrations and both. 
// All the controls related to the registrations is separated from the normal 
// get and post requests and is done by Ajax calls. This methods is prefixed with 'Check'.
// Individual registration: 
// - add the user if (s)he is not added before and
// - (s)he has the right gender and
// - (s)he has the right age relativ to the tournament spesifications.
// Team registration:
// - add the team if it is not added before and
// - non of the team members is registrated for another team and
// - the team members has the right gender and
// - the right age relativ to the tournament spesifications.
// Individual or Team registration (use the individual and team methods):
// - See Individual registration and
// - See Team registration.


namespace TournamentPage.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        /* Declares the dependencies */
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;

        /* Global control flags and a string error message variable */
        bool TeamMemberExist = false;
        bool TeamExist = false;
        
        bool GenderDoNotMatchGirls = false;
        bool GenderDoNotMatchBoys = false;
        
        bool AgeToYoung = false;
        bool AgeToOld = false;

        bool RegistrationToEarly = false;
        bool RegistrationToLate = false; 

        string message = "";

        /* The service provider inspects the constructor arguments, and creates 
        and injects instances of this dependencies when it make the Registration controller object. */
        public RegistrationController(ApplicationDbContext db, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager)
        {
            this.db = db;
            this.userManager = usermanager;
            this.signInManager = signInManager;
        }

        // Creates a view model, sets its properties and return this model to the
        // individual registration view. 
        // url: Registration/IndividualRegistration
        [HttpGet]
        public async Task<IActionResult> IndividualRegistration(int id)
        {
            /* New instance */
            IndividualRegistrationViewModel IModel = new IndividualRegistrationViewModel();
            
            /* Get the tournament info */
            IModel.Tournament = await db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefaultAsync(t => t.TournamentId == id);

            /* Get the logged in user */
            ApplicationUser user = await userManager.GetUserAsync(User);
            if(user == null) /* something went wrong */
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/Registration/{nameof(RegistrationController.IndividualRegistration)}/{id}"});
            }
            /* Set the objects user */
            IModel.User = user;
            
            return View(IModel);
        }

        // This method create a new instance of a tournamentuser, sets its properties and save
        // it to the database, before we do a redirect to the users profilepage.
        // We only reach this method if the Ajax call is successful.
        // url: Registration/IndividualRegistration
        [HttpPost]
        public async Task<IActionResult> IndividualRegistration(int tournamentId, string applicationId)
        {
            /* Create a new instance of a TournamentUser */
            TournamentUser tu = new TournamentUser();
            
            /* Sets it's properties */
            tu.User = await userManager.FindByIdAsync(applicationId);
            
            tu.Tournament = await db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

            tu.Joined = DateTime.Now;

            /* Save the new instance to the database */
            db.TournamentUser.Add(tu);
            db.SaveChanges();

            /* Redirect to userprofile */
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {Message = MessageStatus.IndividualRegistrationSuccess});
        }

        // This method is called by AJAX. The AJAX call checks to see if the user have been registrated
        // before. It also checks the gender and age of the user against the tournament gender and age interval.
        // url: Registration/IndividualRegistrationCheck
        public string IndividualRegistrationCheck(int tournamentId, string applicationId)
        { 
            /* Get user */
            var user = userManager.FindByIdAsync(applicationId);
            
            /* User control */ 
            /* Check to see if user is already registrated */
            var tournamentUser = db.TournamentUser.Include(u => u.User).FirstOrDefault(t => t.Tournament.TournamentId == tournamentId && t.User.Id == applicationId);
            /* If (s)he is we tell the user so */
            if(tournamentUser != null)
            {
                TeamMemberExist = true;
            }
            /* Regardless of this we check the users gender and age */
            /* Get the tournament */
            var tournament = db.Tournament.Include(g => g.GenderType).FirstOrDefault(t => t.TournamentId == tournamentId);
            /* Make a list of the user object. We do this because the called method only takes a list of ApplicationUsers */
            var list = new List<ApplicationUser>();
            list.Add(user.Result);
            /* Gender control */
            this.GenderCheck(tournament, list);
            /* Age control */
            this.AgeCheck(tournament, list);
            /* Registration control */
            this.RegistrationDateCheck(tournament);
            
            /* Formulate the return error message according to which flag is set */
            this.MakeErrorMessage();
            
            return message; 
        }

        // Creates a view model, sets its properties and return this model to the
        // team registration view. 
        // url: Registration/TeamRegistration
        [HttpGet]
        public async Task<IActionResult> TeamRegistration(int id)
        {
            /* New instance */
            TeamRegistrationViewModel TModel = new TeamRegistrationViewModel();
            
            /* New instance of the IndividualRegistrationModel */
            IndividualRegistrationViewModel IModel = new IndividualRegistrationViewModel();
            TModel.IndividualRegistrationViewModel = IModel;

            /* Get the tournament info */
            Tournament tournament = await db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefaultAsync(t => t.TournamentId == id);

            /* Set the tournament property */
            TModel.IndividualRegistrationViewModel.Tournament = tournament;

            /* Get the logged in user */
            ApplicationUser user = await userManager.GetUserAsync(User);
            if(user == null) /* something went wrong */
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/Registration/{nameof(RegistrationController.TeamRegistration)}/{id}"});
            }
            /* Set the objects user */
            TModel.IndividualRegistrationViewModel.User = user;

            /* Get the teams the user is contactperson for */
            TModel.Teams = db.Team.Where(u => u.User.Id == user.Id).ToList();
            
            return View(TModel);
        }


        // This method is called by AJAX. The AJAX call checks to see if the team allready is registrated
        // for the spesific tournament.
        // It also checks the gender and age of the team users against the tournament gender and age interval.
        // url: Registration/IndividualRegistrationCheck
        public string TeamRegistrationCheck(int tournamentId, int teamId)
        {   
            /* Team control */
            this.TeamExists(tournamentId, teamId);

            /* Get the tournament and team members */
            var tournament = db.Tournament.Include(g => g.GenderType).FirstOrDefault(t => t.TournamentId == tournamentId);
            var TeamMembers = db.TeamUser.Where(i => i.Team.TeamId == teamId).Select(u => u.User).ToList();

            /* Team member control */
            /* Check if someone on the team allready is registrated for
            this tournament (is on a another team).
            We check each team member up against each of the tournament users.  */
            this.TeamMemberExists(tournamentId, TeamMembers);

            /* Gender control */
            /* Check that the team is not mixed by both girls and boys when the tournament demands only girls or only boys. */
            /* Get the tournament */
            this.GenderCheck(tournament, TeamMembers);

            /* Age control */
            this.AgeCheck(tournament, TeamMembers);

            /* Registration control */
            this.RegistrationDateCheck(tournament);
           
            /* Formulate the return error message according to which flag is set */
            this.MakeErrorMessage();

            return message;
        }

        // The user can change his tournament team to another team he is contactperson for if he allready has
        // registered a team for a special tournament. If not the team is added as a new tournament team. 
        // url: Registration/TeamRegistration
        [HttpPost]
        public async Task<IActionResult> TeamRegistration(int tournamentId, int teamId, string applicationId)
        {
            /* Create a new instance of a TournamentUser */
            TournamentTeam tt = new TournamentTeam();
            
            /* Sets it's properties */
            /* Set the team */
            tt.Team = await db.Team.Include(a => a.User).FirstOrDefaultAsync(t => t.TeamId==teamId);
            
            /* Set the tournament */
            tt.Tournament = await db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

            /* Set time when team joined tournament */
            tt.TeamJoined = DateTime.Now;

            /* If the user allready has a team registrated in the tournament we replace it with the new selected team */
            var result = db.TournamentTeam.Include(u => u.Tournament).Include(t => t.Team).ThenInclude(p => p.User).Where(t => t.Tournament.TournamentId == tournamentId && t.Team.User.Id == applicationId).FirstOrDefault(); 
            if(result != null)
            {
                /* We remove the earlier tournamentusers (because the number of members can be different on the different team) */
                /* Get the previous teamusers */
                var PreviousTeamId = result.Team.TeamId;
                var PreviousTeamUsers = db.TeamUser.Include(u => u.User).Where(t => t.Team.TeamId == PreviousTeamId).ToList();
                /* Remove the tournamentusers */
                for(int i=0; i<PreviousTeamUsers.Count; i++)
                {
                    /* Get a tournamentuser and remove it */
                    var PreviousTournamentUser = db.TournamentUser.Include(u => u.User).Include(u => u.Tournament).Where(t => t.User.Id == PreviousTeamUsers.ElementAt(i).User.Id).Where(t=> t.Tournament.TournamentId == tournamentId).FirstOrDefault();
                    if(PreviousTournamentUser != null)
                    {
                        db.TournamentUser.Remove(PreviousTournamentUser);
                        db.SaveChanges();
                    }
                }
                /* Change the existing tournament team properties in the tournament team table */
                result.Team = tt.Team;
                result.TeamJoined = DateTime.Now;
                result.Tournament = tt.Tournament;
                db.SaveChanges();
                /* Save the new team members to the tournament user table */
                var NewTeamUsers = db.TeamUser.Include(u => u.User).Where(t => t.Team.TeamId==teamId).ToList();
                /* Add each of the tournamentusers to the table */
                for(int i=0; i<NewTeamUsers.Count; i++)
                {
                    TournamentUser TournamentUser = new TournamentUser();
                    
                    var NewTeamUser = NewTeamUsers.ElementAt(i);
                    
                    TournamentUser.User = NewTeamUser.User;
                    TournamentUser.Tournament = tt.Tournament;
                    TournamentUser.Joined = DateTime.Now;
                    
                    db.TournamentUser.Add(TournamentUser);
                    db.SaveChanges();
                }                
            }
            else /* The contactperson/user has not registrated a team in the tournament yet */
            {
                /* Save the team members to the tournament user table */
                var NewTeamUsers = db.TeamUser.Include(u => u.User).Where(t => t.Team.TeamId==teamId).ToList();
                /* Add the new tournamentusers */
                for(int i=0; i<NewTeamUsers.Count; i++)
                {
                    TournamentUser TournamentUser = new TournamentUser();
                    
                    var NewTeamUser = NewTeamUsers.ElementAt(i);
                    
                    TournamentUser.User = NewTeamUser.User;
                    TournamentUser.Tournament = tt.Tournament;
                    TournamentUser.Joined = DateTime.Now;
                    
                    db.TournamentUser.Add(TournamentUser);
                    db.SaveChanges();
                }
                /* Add the new tournament team to the tournament team table */
                db.TournamentTeam.Add(tt);
                db.SaveChanges();
            }
            /* Redirect to userprofile */
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {Message = MessageStatus.TeamRegistrationSuccess});
        }

        // This method duplicates the TeamRegistration get action. It is done becuse it needs
        // its own view and becuse it should be clearly set. 
        // url: Registration/IndividualOrTeamRegistration
        [HttpGet]
        public async Task<IActionResult> IndividualOrTeamRegistration(int id)
        {
            /* New instance */
            TeamRegistrationViewModel TModel = new TeamRegistrationViewModel();
            
            /* New instance of the IndividualRegistrationModel */
            IndividualRegistrationViewModel IModel = new IndividualRegistrationViewModel();
            TModel.IndividualRegistrationViewModel = IModel;

            /* Get the tournament info */
            Tournament tournament = await db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefaultAsync(t => t.TournamentId == id);

            /* Set the tournament property */
            TModel.IndividualRegistrationViewModel.Tournament = tournament;

            /* Get the logged in user */
            ApplicationUser user = await userManager.GetUserAsync(User);
            if(user == null) /* something went wrong */
            {
                await signInManager.SignOutAsync();
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/Registration/{nameof(RegistrationController.IndividualOrTeamRegistration)}/{id}"});
            }
            /* Set the objects user */
            TModel.IndividualRegistrationViewModel.User = user;

            /* Get the teams the user is contactperson for */
            TModel.Teams = db.Team.Where(u => u.User.Id == user.Id).ToList();
            
            return View(TModel);
        }

        // Used by Ajax to control the following: 
        // -is the team allready registrated for the tournament?
        // -is one or more of the team members registrated individually or on another team?
        // -does the players have the right gendertype?
        // -does the players have the right age?
        // We do not allow for this events!
        // url: Registration/IndividualOrTeamRegistrationCheck
        public string IndividualOrTeamRegistrationCheck(int tournamentId, int teamId)
        {
            /* Team control */
            /* Check if the team allready exists as a tournamentteam for the tournament */
            this.TeamExists(tournamentId, teamId);
            
            /* Team member control */
            /* Check if some on the team allready is registrated for
            this tournament (either individually or is on a another team).
            We check each team member up against each of the tournament users.  */
            var TeamMembers = db.TeamUser.Where(i => i.Team.TeamId == teamId).Select(u => u.User).ToList();
            this.TeamMemberExists(tournamentId, TeamMembers);

            /* Gender control */
            /* Check that the team is not mixed by both girls and boys when the tournament is a pure girls or boys tournament */
            /* Get the tournament */
            var tournament = db.Tournament.Include(g => g.GenderType).FirstOrDefault(t => t.TournamentId == tournamentId);
            this.GenderCheck(tournament, TeamMembers);

            /* Age control */
            this.AgeCheck(tournament, TeamMembers);

            /* Registration control */
            this.RegistrationDateCheck(tournament);
           
            /* Set the return message after which global flags which is set */
            this.MakeErrorMessage();
            
            return message; 
        }

        // Removes a team from a tournament. 
        public IActionResult RemoveTournamentTeamRegistration(int teamId, int tournamentId)
        {
            /* Get the team users of the team */
            var PreviousTeamUsers = db.TeamUser.Include(u => u.User).Where(t => t.Team.TeamId == teamId).ToList();
            /* Remove the tournamentusers */
            for(int i=0; i<PreviousTeamUsers.Count; i++)
            {
                /* Get a tournamentuser and remove it */
                var PreviousTournamentUser = db.TournamentUser.Include(u => u.User).Include(u => u.Tournament).Where(t => t.User.Id == PreviousTeamUsers.ElementAt(i).User.Id).Where(t=> t.Tournament.TournamentId == tournamentId).FirstOrDefault();
                if(PreviousTournamentUser != null)
                {
                    db.TournamentUser.Remove(PreviousTournamentUser);
                    db.SaveChanges();
                }
            }
            /* Remove the tournament team */
            db.TournamentTeam.Remove(db.TournamentTeam.Include(t => t.Team).Include(t => t.Tournament).Where(t => t.Tournament.TournamentId == tournamentId && t.Team.TeamId == teamId).FirstOrDefault());
            db.SaveChanges();

            /* Redirect to userprofile */
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {Message = MessageStatus.RemoveTournamentTeamRegistrationSuccess});
               
        }
        // Removes a user from a tournament.
        public IActionResult RemoveIndividualRegistration(int tournamentId, string applicationId)
        {
            /* Remove user as a tournamentuser */
            db.TournamentUser.Remove(db.TournamentUser.Include(t => t.Tournament).Include(u => u.User).Where(u => u.User.Id == applicationId && u.Tournament.TournamentId == tournamentId).FirstOrDefault());
            db.SaveChanges();
            /* Redirect to userprofile */
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {Message = MessageStatus.RemoveIndividualRegistrationSuccess});

        }

        // Help method:
        // Team user control: Check to see if one or more of the team members allready is registrated for
        // the tournament. This can happen if the user participates with another team or if (s)he has 
        // registrated individually for a tournament which allow both team and individual registartion.
        private void TeamMemberExists(int tournamentId, IList<ApplicationUser> userList)
        {
            var TournamentUsers = db.TournamentUser.Where(t => t.Tournament.TournamentId == tournamentId).Select(u => u.User).ToList();
            /* Iterate through each teammember in userList and compare it against each of
            the tournamentusers */
            for(int i=0; i<userList.Count; i++)
            {
                for(int j=0; j<TournamentUsers.Count; j++)
                {
                    if(userList.ElementAt(i).Id == TournamentUsers.ElementAt(j).Id)
                    {
                        TeamMemberExist = true;
                        break;
                    }
                }
            }

        } 

        // Help method:
        // Team exist control: Check if the team allready is registrated for the tournament.
        private void TeamExists(int tournamentId, int teamId)
        {
            var result = db.TournamentTeam.Any(t => t.Tournament.TournamentId == tournamentId && t.Team.TeamId == teamId);
            if(result)
            {
                TeamExist = true;
            }
        }

        // Help method:
        // Gender control: Compares a users gender agaisnt the tournament gender.
        private void GenderCheck(Tournament tournament, IList<ApplicationUser> userList)
        {
            /* Gender control */
            var gendertype = tournament.GenderType.GenderTypeName;
            /* If the tournamnet only is for girls or boys */
            if(gendertype=="Jenter" || gendertype=="Gutter")
            {
                if(gendertype=="Jenter") /* tournament gender is girls */
                {
                    for(int i=0; i<userList.Count; i++)
                    {
                        if(userList.ElementAt(i).Gender != "Kvinne" )
                        {
                            GenderDoNotMatchGirls = true; /* gender mismatch */
                            break;
                        }

                    }
                }
                else /* tournament gender is boys */
                {
                    for(int i=0; i<userList.Count; i++)
                    {
                        if(userList.ElementAt(i).Gender != "Mann" )
                        {
                            GenderDoNotMatchBoys = true;  /* gender mismatch */
                            break;
                        }
                    }
                }
            }
        }
        // Help method:
        // Age control: Compares a users age agaisnt the tournament age interval.
        private void AgeCheck(Tournament tournament, IList<ApplicationUser> userList)
        {
            int tournamentAgeFrom = 0, tournamentAgeTo = 0;
            try
            {
                tournamentAgeFrom = Int32.Parse(tournament.AgeFrom);
                tournamentAgeTo = Int32.Parse(tournament.AgeTo);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
            for(int i=0; i<userList.Count; i++)
            {
                 /* Calculates the age */
                var Age = DateTime.Now.Date.Subtract( (DateTime)userList.ElementAt(i).BirthDate);
                int AgeInYears = (int)Age.TotalDays/365;
                /* Is there a age mismatch */
                if(AgeInYears < tournamentAgeFrom)
                {
                    AgeToYoung = true; /* We do not do a break here because we want to see if one of the other members is too old */
                }
                if(AgeInYears > tournamentAgeTo)
                {
                    AgeToOld = true;
                }
            }
        }

        // Help method:
        // This method checks if the the todays date lies in between the registartion interval for the tournament.
        private void RegistrationDateCheck(Tournament tournament)
        {
            // The registration has not started yet
            if( DateTime.Compare(DateTime.Now, tournament.RegisterDateStart) < 0)
            {
                RegistrationToEarly = true;
            }
            // The registration has ended
            else if(DateTime.Now.CompareTo(tournament.RegisterDateEnd) > 0)
            {
                RegistrationToLate = true;
            }
        } 

        // Help method:
        // This method formulate the error messag to the user:
        private void MakeErrorMessage()
        {
            /* The team exists */
            if(TeamExist)
            {
                message += "Laget er allerede registret for denne turneringa!<br> Du kan bytte lag ved å velge et annet lag i lista. <br> Du kan kun registrere et lag for denne turneringa!<br>";
            }
            /* One or more team memebers exsits */
            if(TeamMemberExist)
            {
                message += "Du, eller et eller flere medlemmer av laget hvis lagpåmelding <br> er allerede registrerte for denne turneringen. Velg et annet lag fra lista hvis det er lagpåmelding! <br>";
            }
            /* Gender of one or more team members is man/boy when the tournament demands only woman/girl  */
            if(GenderDoNotMatchGirls)
            {
                message += "Turneringen aksepterer kun jenter/kvinner. <br>";
            }
            /* Gender of one or more team members is man/boy when the tournament demands only woman/girl  */
            if(GenderDoNotMatchBoys)
            {
                message += "Turneringen aksepterer kun gutter/menn. <br>";
            }
            /* One or more team members is too young */
            if(AgeToYoung)
            {
                message += "Du eller en eller flere av lagets medlemmer er for unge. <br>";
            }
            /* One or more team members is too old */
            if(AgeToOld)
            {
                message += "Du eller en eller flere av lagets medlemmer er for gamle. <br>";
            }
            /* Registration has not started yet */
            if(RegistrationToEarly)
            {
                message += "Påmeldingen har ikke startet enda. <br>";
            }
            /* Registration is due */
            else if(RegistrationToLate)
            {
                message += "Påmeldingen er avsluttet. <br>";
            }
        }
    }
}