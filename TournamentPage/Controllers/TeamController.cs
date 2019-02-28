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
using TournamentPage.Models.TeamViewModels;
using Newtonsoft.Json.Linq;
using System.Data;

/* This is a CRUD controller for tournaments. 
It is used to create, read, update and delete tournaments. */
namespace TournamentPage.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        /* Declares the dependencies */
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;

        /* The service provider inspects the constructor arguments, and creates 
        and injects instances of this dependencies when it make the Tournament controller object. */
        public TeamController(ApplicationDbContext db, UserManager<ApplicationUser> usermanager)
        {
            this.db = db;
            this.userManager = usermanager;

        }


        /* url: /Team/CreateTeam */
        [HttpGet]
        public IActionResult CreateTeam()
        {
           return View();
        }    

        /* This is called when the user confirms the team creation */
        [HttpPost]
        public IActionResult CreateTeam(Team team)
        {
            
            team.TeamRegisterDate = DateTime.Now;
            team.TeamModifiedDate = DateTime.Now;
            team.User = userManager.GetUserAsync(User).Result;
            db.Team.Add(team);
            db.SaveChanges();

            TeamUser TU = new TeamUser();
            TU.Joined = DateTime.Now;
            TU.Team = team;
            TU.User = team.User;
            db.TeamUser.Add(TU);
            db.SaveChanges();

            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile");
        }
        

         /* url: /Tournament/EditTournament */
        [HttpGet]
        public IActionResult EditTeam(int id)
        {
            /* Get the tournament to edit */
            Team teamToEdit = db.Team.FirstOrDefault(t => t.TeamId == id);
            return View(teamToEdit);
        }

        /* url: /Tournament/EditTournament */
        [HttpPost]
        public IActionResult EditTeam(Team teamToEdit)
        {   
            /* Get the id and compare it with existing id in the database. */
            var id = teamToEdit.TeamId;
            if(ModelState.IsValid)
            {
                Team dbT = db.Team.FirstOrDefault(t => t.TeamId == id);
                /* Updates the team if it exists. User details will not be updated. */
                if(dbT != null)
                {
                    dbT.TeamName = teamToEdit.TeamName;
                    //dbT.ContactPerson = teamToEdit.ContactPerson;
                    dbT.TeamRegisterDate = teamToEdit.TeamRegisterDate;
                    dbT.TeamModifiedDate = DateTime.Now;
                  
                    db.SaveChanges();
                   return RedirectToAction("SeeTeamPlayers", new { Id = id });
                }
            }
            
            /* Something is wrong with the input data. The user must try again. */
            
            return View(teamToEdit);
        }

        [HttpGet]
        public IActionResult EditTeamPlayers(int id){
            // Set the necessary viewbag items we need
            ViewBag.TeamId = id;
            ViewBag.TeamName = db.Team.FirstOrDefault(n => n.TeamId == id).TeamName;
            ViewBag.User = userManager.GetUserAsync(User).Result;

            var teamPlayers = db.TeamUser.
            Include(u => u.User).
            Include(t => t.Team).
            Where(x => x.Team.TeamId == id).OrderBy(u => u.User.LastName).ToList();

            TeamViewModel tvm = new TeamViewModel();
            tvm.Players = teamPlayers;

            var users = db.Users.ToList();

            // Remove all players that already exists in the team.
            users.RemoveAll(u => u.Id == userManager.GetUserAsync(User).Result.Id);
            for(int i = 0; i < teamPlayers.Count(); i++){
                users.RemoveAll(u => u.Id == teamPlayers.ElementAt(i).User.Id);
            }
            tvm.Users = users;

            tvm.TeamId = id;

            return View(tvm);

        }

        [HttpPost]
        public IActionResult AddTeamPlayers(TeamViewModel model)
        {
            
            
            if(model != null){

                List<ApplicationUser> PlayersToAdd = new List<ApplicationUser>();

                for(int i = 0; i < model.NewPlayers.Count(); i++){
                    var user = userManager.FindByEmailAsync(model.NewPlayers[i]).Result;
                    PlayersToAdd.Add(user);
                }

                for(int i = 0; i < PlayersToAdd.Count(); i++){
                    TeamUser newPlayer = new TeamUser();
                    var team = db.Team.FirstOrDefault(t => t.TeamId == model.TeamId);
                    newPlayer.Team = team;
                    newPlayer.Joined = DateTime.Now;
                    newPlayer.User = PlayersToAdd.ElementAt(i);
                    db.TeamUser.Add(newPlayer);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("SeeTeamPlayers", new { Id = model.TeamId });  
        }

        [HttpGet]
        public IActionResult RemoveTeamPlayer(int teamuserid)
        {
            Console.WriteLine("TeamUser id is: "+teamuserid);

            var tu = db.TeamUser.Include(x => x.Team).FirstOrDefault(t => t.TeamUserId == teamuserid);

            var teamId = tu.Team.TeamId;

            db.TeamUser.Remove(tu);
            db.SaveChanges();

             return RedirectToAction("SeeTeamPlayers", new { Id = teamId });            
        }

        /* Deletes the team plus all associations */
        /* url: /Team/Delete */
        [HttpGet]
        [Authorize]
        public IActionResult Delete(int id)
        {
            Team team = db.Team.FirstOrDefault(t => t.TeamId == id);

            return View(team);
        }


       [HttpPost]
       [Authorize]
        public IActionResult DeleteConfirmed(int id)
        {
            Team team = db.Team.FirstOrDefault(t => t.TeamId == id);
            MessageStatus message;

            if(team == null) /* something went wrong */
            {
                message = MessageStatus.DeletedTeamSuccess;
            }else{
                db.TournamentTeam.RemoveRange(db.TournamentTeam.Where(t => t.Team.TeamId==id));
                db.TeamUser.RemoveRange(db.TeamUser.Where(t => t.Team.TeamId==id));
                db.Team.Remove(team);
                db.SaveChanges();
                message = MessageStatus.DeletedTeamSuccess;
            }
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {message = message});
        }
      

        /* url: /Tournament/Delete2 */
       /* [HttpPost]
        public IActionResult Delete2(int id)
        {
            */
            /* Finds the tournament to delete */
            /*Team teamToDelete = db.Team.FirstOrDefault(t => t.TeamId == id);
            MessageStatus message;
            if(teamToDelete != null)
            {
                db.TeamUser.RemoveRange(db.TeamUser.Where(t => t.Team.TeamId==id));
               
                db.Team.Remove(teamToDelete);
                db.SaveChanges();
                message = MessageStatus.DeletedTeamSuccess;
            }
            else
            {
                message = MessageStatus.DeletedTeamSuccess;
            }
            return RedirectToAction(nameof(UserProfileController.ShowUserProfile), "UserProfile", new {message = message});
        }*/
     

           /* url: /Tournament/SeeTeam */
        /* Finds the tournament team from a team id and returns the team to the view */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SeeTeamPlayers(int id)
        {
            ViewBag.TeamId = id;
            ViewBag.TeamName = db.Team.FirstOrDefault(n => n.TeamId == id).TeamName;
            var team = db.TeamUser.
            Include(u => u.User).
            Include(t => t.Team).
            Where(x => x.Team.TeamId == id).OrderBy(u => u.User.LastName).ToList();

            ViewBag.User = db.Team.Include(u => u.User).FirstOrDefault(t => t.TeamId == id).User;

            return View(team);
        }

        
    }
}