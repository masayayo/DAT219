using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentPage.Models;
using TournamentPage.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/* This is a CRUD controller for tournaments. 
It is used to create, read, update and delete tournaments. */
namespace TournamentPage.Controllers
{
    [Authorize]
    public class TournamentController : Controller
    {
        /* Declares the dependencies */
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private IHostingEnvironment _hostingEnv;
        
        private List<SelectListItem> AgeList;

        /* The service provider inspects the constructor arguments, and creates 
        and injects instances of this dependencies when it make the Tournament controller object. */
        public TournamentController(ApplicationDbContext db, UserManager<ApplicationUser> usermanager, IHostingEnvironment hostingEnv)
        {
            this.db = db;
            this.userManager = usermanager;
            _hostingEnv = hostingEnv;

            AgeList = new List<SelectListItem>();
        }

        /* Helpmethod: Used to make a list of ages */
        private void MakeTournamentSelectList()
        {
            /* Value must be a object (string). */
            for(int i=5; i<101; i++)
            {
                AgeList.Add(new SelectListItem{Text=i.ToString(), Value=i.ToString()});
            }
        }

        /* Gets the generated brackets for the specific tournament id */
        /* url: /Tournament/ShowTournamentMatchups */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ShowTournamentMatchups(int id){

            var tournament = db.Tournament.Include(u => u.User).Where(t => t.TournamentId == id).ToList(); 

            String bracketData = JsonConvert.SerializeObject(tournament[0].BracketsJSON);

            ViewBag.TournamentId = id;
            ViewBag.AdminUser = tournament[0].User;

            

            return View((object)bracketData);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditTournamentMatchups(int id){
            var tournament = db.Tournament.Include(u => u.User).Where(t => t.TournamentId == id).ToList(); 

            String bracketData = JsonConvert.SerializeObject(tournament[0].BracketsJSON);

            ViewBag.TournamentId = id;

            return View((object)bracketData);
        }

        /* Updated the tournament brackets each time a value gets edited in the showTournamentMatchups */
        [HttpPost]
        [Authorize]
        public void UpdateTournamentMatchups(string data){


            JObject dataToJSON = JObject.Parse(data);
            Tournament tournament = db.Tournament.FirstOrDefault(t => t.TournamentId == Int32.Parse(dataToJSON.GetValue("tournamentId").ToString()));

            // Delete the tournamentId-field from the json so that the tournament-data fits with the jQuery brackets library format
            dataToJSON.Remove("tournamentId");

            tournament.BracketsJSON = JsonConvert.SerializeObject(dataToJSON);
            db.SaveChanges();
        }

        /* url: /Tournament/CreateTournament */
        [HttpGet]
        public IActionResult CreateTournament()
        {
            Tournament t = new Tournament();

            /* Make the age list */
            this.MakeTournamentSelectList();

            /* Setter fornuftige startdatoer for datepickeren */
            t.RegisterDateStart = DateTime.Now.Date;
            t.RegisterDateEnd = t.RegisterDateStart.AddDays(3);
            t.TournamentDateStart = t.RegisterDateEnd.AddDays(1); 
            t.TournamentDateEnd = t.TournamentDateStart.AddDays(3);
            
            /* Selectlists */
            ViewBag.AgeList = AgeList;
            ViewBag.SportsType = new SelectList(db.SportType.Select(s => s.SportTypeName).Distinct());
            ViewBag.RegistrationType = new SelectList(db.RegisterType.Select(r => r.RegisterTypeName).Distinct());
            ViewBag.TournamentType = new SelectList(db.TournamentType.Select(e => e.TournamentTypeName).Distinct());
            ViewBag.GenderType = new SelectList(db.GenderType.Select(g => g.GenderTypeName).Distinct());

            return View(t);
        }

        /*public async Task<ActionResult> ConfirmTournamentRegistration(int Id)
        {
            Tournament tournament = await db.Tournament.FirstOrDefaultAsync(t => t.TournamentId == Id);
            return View(tournament);
        }*/

         /* url: /Tournament/CreateTournament */
        [HttpPost]
        public IActionResult CreateTournament(Tournament tournament)
        {
            /* The model data is valid and we save it to the database before we redirect the user
            to a confirm window. */
            if(ModelState.IsValid)
            {
                /* We get the logged in user. This person will be the creator of the tournament. */
                tournament.User = userManager.GetUserAsync(User).Result;
                /* Save the new tournament to the database */
                db.Tournament.Add(tournament);
                db.SaveChanges();

                /*return RedirectToAction(nameof(ConfirmTournamentRegistration), new{Id = tournament.TournamentId});*/
                ViewData["StatusMessage"] = MessageStatus.CreatedTournamentSuccess; /*  ViewData["Success"] is sat in UserProfile action */
                return View("ConfirmTournamentRegistration",tournament);
            }

            /* Something is wrong with the input data. The user must try again. */
            this.MakeTournamentSelectList();
            ViewBag.AgeList = AgeList;
            ViewBag.SportsType = new SelectList(db.SportType.Select(s => s.SportTypeName).Distinct());
            ViewBag.RegistrationType = new SelectList(db.RegisterType.Select(r => r.RegisterTypeName).Distinct());
            ViewBag.TournamentType = new SelectList(db.TournamentType.Select(e => e.TournamentTypeName).Distinct());
            ViewBag.GenderType = new SelectList(db.GenderType.Select(g => g.GenderTypeName).Distinct());
            ViewData["StatusMessage"] = MessageStatus.CreatedTournamentFailure; /*  ViewData["Success"] is sat in UserProfile action */
            
            return View(tournament);    
        }

         /* url: /Tournament/EditTournament */
        [HttpGet]
        public IActionResult EditTournament(int id)
        {
            /* Selectlists */
            this.MakeTournamentSelectList();
            ViewBag.AgeList = AgeList;
            ViewBag.SportsType = new SelectList(db.SportType.Select(s => s.SportTypeName).Distinct());
            ViewBag.RegistrationType = new SelectList(db.RegisterType.Select(r => r.RegisterTypeName).Distinct());
            ViewBag.TournamentType = new SelectList(db.TournamentType.Select(e => e.TournamentTypeName).Distinct());
            ViewBag.GenderType = new SelectList(db.GenderType.Select(g => g.GenderTypeName).Distinct());
            
            /* Get the tournament to edit */
            var tournamentToEdit = db.Tournament.
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefault(t => t.TournamentId == id);
            Console.WriteLine(tournamentToEdit.RegisterDateStart);

            return View(tournamentToEdit);
        }

        /* url: /Tournament/EditTournament */
        [HttpPost]
        public IActionResult EditTournament(Tournament tournamentToEdit)
        {   
            /* Get the id and compare it with existing id in the database. */
            var id = tournamentToEdit.TournamentId;
            if(ModelState.IsValid)
            {
                Tournament dbT = db.Tournament.FirstOrDefault(t => t.TournamentId == id);
                /* Updates the tournament if it exists. User details will not be updated. */
                if(dbT != null)
                {
                    dbT.TournamentName = tournamentToEdit.TournamentName;
                    dbT.SportType = tournamentToEdit.SportType;
                    dbT.RegisterType = tournamentToEdit.RegisterType;
                    dbT.TournamentType = tournamentToEdit.TournamentType;
                    dbT.Location = tournamentToEdit.Location;
                    dbT.RegisterFee = tournamentToEdit.RegisterFee;
                    dbT.GenderType = tournamentToEdit.GenderType;
                    dbT.AgeFrom = tournamentToEdit.AgeFrom;
                    dbT.AgeTo = tournamentToEdit.AgeTo;
                    dbT.RegisterDateStart = tournamentToEdit.RegisterDateStart;
                    dbT.RegisterDateEnd = tournamentToEdit.RegisterDateEnd;
                    dbT.TournamentDateStart = tournamentToEdit.TournamentDateStart;
                    dbT.TournamentDateEnd = tournamentToEdit.TournamentDateEnd;
                    dbT.TermsAccepted = tournamentToEdit.TermsAccepted;
                    db.SaveChanges();

                    ViewData["StatusMessage"] = MessageStatus.EditTournamentSuccess; /*  ViewData["Success"] is sat in UserProfile action */

                    return View("ConfirmTournamentRegistration",tournamentToEdit);
                }
            }
            
            /* Something is wrong with the input data. The user must try again. */
            this.MakeTournamentSelectList();
            ViewBag.AgeList = AgeList;
            ViewBag.SportsType = new SelectList(db.SportType.Select(s => s.SportTypeName).Distinct());
            ViewBag.RegistrationType = new SelectList(db.RegisterType.Select(r => r.RegisterTypeName).Distinct());
            ViewBag.TournamentType = new SelectList(db.TournamentType.Select(e => e.TournamentTypeName).Distinct());
            ViewBag.GenderType = new SelectList(db.GenderType.Select(g => g.GenderTypeName).Distinct());
            
            ViewData["StatusMessage"] = MessageStatus.EditTournamentSuccess; /*  ViewData["Success"] is sat in UserProfile action */

            return View(tournamentToEdit);
        }

        /* url: /Tournament/Delete */
        /* Finds and returns the tournament to the view */
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Tournament tournament = db.Tournament.FirstOrDefault(t => t.TournamentId == id);
            if(tournament == null) /* something went wrong */
            {
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = $"/Tournament/{nameof(TournamentController.Delete)}"});
            }
            return View(tournament);
        }

        /* url: /Tournament/Delete2 */
        [HttpPost]
        public IActionResult Delete2(int id)
        {
            /* Finds the tournament to delete */
            Tournament tournamentToDelete = db.Tournament.FirstOrDefault(t => t.TournamentId == id);
            MessageStatus message;
            if(tournamentToDelete != null)
            {
                db.TournamentUser.RemoveRange(db.TournamentUser.Where(t => t.Tournament.TournamentId==id));
                db.TournamentTeam.RemoveRange(db.TournamentTeam.Where(t => t.Tournament.TournamentId==id));
                db.Tournament.Remove(tournamentToDelete);
                db.SaveChanges();
                message = MessageStatus.DeletedTournamentSuccess;
            }
            else
            {
                message = MessageStatus.DeletedTournamentSuccess;
            }
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {message = message});
        }


        /* Method for changing the featured image for a tournament */
        [HttpPost]
        public async Task<IActionResult> EditFeaturedImage(ICollection<IFormFile> files, int Id)
        {      
            /* Get the tournament for reference */
            Tournament Tournament = db.Tournament.FirstOrDefault(t => t.TournamentId == Id);

            /* Get the images path */
             var uploads = Path.Combine(_hostingEnv.WebRootPath, "images");

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    /* Get the extension of the file and create a name unique to the logged in user */
                    string ext = file.FileName.Substring(file.FileName.IndexOf(".")).Trim();
                    string picName = "tournament"+Id+ext;

                    /* Create a filestream that will stream the file to the specific path with a new name for the picture */
                    using (var fileStream = new FileStream(Path.Combine(uploads, picName), FileMode.Create))
                    {

                        /* Copy the file to the ~/images/ folder. The await is necessary for the image to be fully uploaded */
                       await file.CopyToAsync(fileStream);

                       /* Update the LoggedInUser's profile picture path in the database */
                       Tournament.FeaturedImage = "/"+picName;
                       db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("SeeTournamentDetails", new { id = Id });
        }

        /* url: /Tournament/SeeTournamentDetails */
        /* The method finds the tournament with tournament id equal to the input parameter
        id and return this tournament together with it's objects. Number of teams and players
        participating in the tournament is also returned. */
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SeeTournamentDetails(int id, string returnurl=null)
        {
            /* Get the number of teams */
            var Teams = db.TournamentTeam.Where(t => t.Tournament.TournamentId == id).ToList();
            if(Teams==null)
            {
                ViewBag.NumberOfTeams = 0;
            }
            else
            {
                ViewBag.NumberOfTeams = Teams.Count;
            }
            /* Get the number of players */
            var Players = db.TournamentUser.Where(p => p.Tournament.TournamentId == id).ToList();
            if(Players==null)
            {
                ViewBag.NumberOfPlayers = 0;
            }
            else
            {
                ViewBag.NumberOfPlayers = Players.Count;
            }
            /* Gives a retrun possibility for the back button in the view */
            ViewData["returnurl"] = returnurl;

            /* Return the tournament.*/
            return View(db.Tournament.Include(u => u.User).
            Include(s => s.SportType).
            Include(r => r.RegisterType).
            Include(t => t.TournamentType).
            Include(g => g.GenderType).
            FirstOrDefault(t => t.TournamentId == id));
        }

        /* url: /Tournament/ShowTournamentTeam */
        /* Finds a list of tournament team, sort them after teamname and return them to the view */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ShowTournamentTeam(int id)
        {
            ViewBag.TournamentId = id;
            ViewBag.TournamentName = db.Tournament.FirstOrDefault(t => t.TournamentId ==id).TournamentName;

            var TournamentTeams = db.TournamentTeam.
            Include(t => t.Team).
            Where(t => t.Tournament.TournamentId == id).
            OrderBy(n => n.Team.TeamName).ToList();
            
            return View(TournamentTeams);
        }

        /* url: /Tournament/ShowTournamentPlayers */
        /* Find the tournament players, sort them by lastname and return them to the view */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ShowTournamentPlayers(int id)
        {
            ViewBag.TournamentId = id;
            ViewBag.TournamentName = db.Tournament.FirstOrDefault(t => t.TournamentId ==id).TournamentName;

            var TournamentPlayers = db.TournamentUser.
            Include(t => t.User).
            Where(t => t.Tournament.TournamentId == id).
            OrderBy(t => t.User.LastName).ToList();
            
            return View(TournamentPlayers);
        }

        /* url: /Tournament/SeeTournamentTeamPlayers */
        /* Find the players of the tournament team, sort them by lastname and send them to the view */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SeeTournamentTeamPlayers(int id)
        {
            ViewBag.TournamentId =  db.TournamentTeam.Include(t => t.Tournament).FirstOrDefault(o => o.TournamentTeamId == id).Tournament.TournamentId;
            ViewBag.TeamName =  db.TournamentTeam.Include(t => t.Team).FirstOrDefault(o => o.TournamentTeamId == id).Team.TeamName;

            /* Get the team id from the tournamnent team id */
            var TeamId = db.TournamentTeam.Include(t => t.Team).FirstOrDefault(t => t.TournamentTeamId == id).Team.TeamId;
            /* Use this team id to find the team users */
            var TeamPlayers = db.TeamUser.Include(u => u.User).Where(p => p.Team.TeamId == TeamId).OrderBy(n => n.User.LastName).ToList();
            return View(TeamPlayers);
        }

        /* url: /Tournament/SeeTeam */
        /* Finds the tournament team from a team id and returns the team to the view */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SeeTeamPlayers(int id)
        {
            ViewBag.TeamName = db.Team.FirstOrDefault(n => n.TeamId == id).TeamName;
            var team = db.TeamUser.
            Include(u => u.User).
            Include(t => t.Team).
            Where(x => x.Team.TeamId == id).OrderBy(u => u.User.LastName).ToList();

            return View(team);
        }

        
        //TournamentList
        //URL: /Tournament/TournamentList
        [AllowAnonymous]
        public ActionResult TournamentList(string returnurl = null)
        {   
            //return View(dbL.Tournament.ToList());
            var tournaments = db.Tournament.Include(p=>p.SportType).Include(pp=>pp.GenderType).Include(ppp=>ppp.TournamentType).Include(pppp=>pppp.RegisterType);
            
            /* Return url. */
            ViewData["returnurl"] = returnurl;
            
            return View(tournaments);
        }

        //URL: /Tournament/SortTournamentList
        // This method gets AJAX requestes, and sort the tournament table based on 
        // the column name (sortname) and the order (desc/asc) given by decreasingorder.
        // It return a partial view (html) to the ajax caller.
        [AllowAnonymous]
        public IActionResult SortTournamentList(string sortname, bool decreasingorder, string returnurl=null)
        {   
            IList<Tournament> tournament;
            switch(sortname)
            {
                case "SportType":
                {
                    /* The order is turned around if the current order is decreasing */
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.SportType.SportTypeName)
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderByDescending(x => x.SportType.SportTypeName)
                        .ToList();
                    }
                    break;
                }
                case "TournamentName":
                {
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.TournamentName).ToList()
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderByDescending(x => x.TournamentName).ToList()
                        .ToList();
                    }
                    break;
                }
                case "Location":
                {
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.Location).ToList()
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType)
                        .OrderByDescending(x => x.Location)
                        .ToList();
                    }
                    break;
                }
                case "GenderType":
                {
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.GenderType.GenderTypeName)
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderByDescending(x => x.GenderType.GenderTypeName)
                        .ToList();
                    }
                    break;
                }
                case "TournamentType":
                {
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.TournamentType.TournamentTypeName)
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderByDescending(x => x.TournamentType.TournamentTypeName)
                        .ToList();
                    }
                    break;
                }
                case "RegistrationType":
                {
                    if(decreasingorder)
                    {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderBy(x => x.RegisterType.RegisterTypeName)
                        .ToList();
                    }
                    else
                    {
                        tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .OrderByDescending(x => x.RegisterType.RegisterTypeName)
                        .ToList();
                    }
                    break;
                }
                /* No special sorting by default */
                default:
                {
                    tournament = db.Tournament
                        .Include(s => s.SportType)
                        .Include(g => g.GenderType)
                        .Include(t => t.TournamentType)
                        .Include(r => r.RegisterType).ToList()
                        .ToList();
                    break;
                }
            }
            ViewData["returnurl"] = returnurl;
            return PartialView("TournamentListPartial", tournament);
        }
    }
}