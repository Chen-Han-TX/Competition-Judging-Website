﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameTime.DAL;
using GameTime.Models;
using System.IO;
//using MimeKit; ->For file downloading

namespace GameTime.Controllers
{
    public class ViewCompetitionController : Controller
    {

        private ViewCompetitionDAL competitionContext = new ViewCompetitionDAL();
        private CompetitorSubmissionDAL competitorContext = new CompetitorSubmissionDAL();

        // View all the competitions
        public ActionResult Index()
        {
            List<CompetitionViewModel> competitionList = new List<CompetitionViewModel>();

            competitionList = competitionContext.GetAllCompetitions();
            ViewData["SuccessfulSearch"] = true;


            return View(competitionList);
        }

        //For the search bar
        [HttpPost]
        public ActionResult Index(IFormCollection formData)
        {
            string competitionName = formData["CompetitionName"];
            List<CompetitionViewModel> competitionList = new List<CompetitionViewModel>();
            competitionList = competitionContext.GetAllCompetitions();

            foreach (CompetitionViewModel competition in competitionList)
            {
                if (competition.CompetitionName == competitionName)
                {
                    return RedirectToAction("ViewCompetition", new { competitionId = (int)competition.CompetitionID });
                }
            }
            ViewData["SuccessfulSearch"] = false;
            return View(competitionList);
        }

        // View details of a competition
        public ActionResult ViewCompetition(int? competitionId)
        {

            //mapping competition details, competitor details and comment sections together
            CompetitionCommentViewModel competitionCommentVM = new CompetitionCommentViewModel();
            competitionCommentVM.competitorList = competitorContext.getAllCompetitor((int)competitionId);
            competitionCommentVM.commentList = competitionContext.getAllComments((int)competitionId);
            competitionCommentVM.competition = competitionContext.getCompetitionDetails((int)competitionId);

      
            //Get the status of the competition
            List<CompetitionViewModel> competitionList = new List<CompetitionViewModel>();
            competitionList = competitionContext.GetAllCompetitions();
            CompetitionViewModel competition = competitionList.Find(obj => obj.CompetitionID == (int)competitionId);
            ViewData["Status"] = competition.Status;

            if (competition.Status == "Over")
            {
                List<CompetitorSubmissionViewModel> sortedCompetitor = competitionCommentVM.competitorList.OrderBy(x => x.Ranking).ToList();
                competitionCommentVM.competitorList = sortedCompetitor;
            }


            //For voting checks
            string sessionCompetitionId = "competition" + competitionId.ToString();

            if (HttpContext.Session.GetString(sessionCompetitionId) == null)
            {
                TempData["hasVoted"] = "false";
                
            }
            else
            {
                TempData["hasVoted"] = "true";
                TempData["votedTo"] = HttpContext.Session.GetString(sessionCompetitionId);
            }

            return View(competitionCommentVM);

        }



        public ActionResult Vote(int? competitorId, string competitorName, int? competitionId)
        {
            List<CompetitorSubmissionViewModel> competitorList = competitorContext.getAllCompetitor((int)competitionId);

           
            string sessionCompetitionId = "competition" + competitionId.ToString();

            //If have not voted
            if (HttpContext.Session.GetString(sessionCompetitionId) == null)
            {
                HttpContext.Session.SetString(sessionCompetitionId, competitorName);

                CompetitorSubmissionViewModel competitor = competitorList.Find(obj => obj.CompetitorId == (int)competitorId);

                competitorContext.UpdateVoteCount(competitor);
            }


            
            return RedirectToAction("ViewCompetition", new { competitionId = (int)competitionId});
        }



        [HttpPost]
        public ActionResult AddComment(IFormCollection formData)
        {
            int competitionId = Convert.ToInt32(formData["CompetitionID"]);
            DateTime dateTimePosted = Convert.ToDateTime(formData["DateTimePosted"]);
            string description = formData["Description"];

            Comment newCommend = new Comment
            {
                CompetitionID = competitionId,
                Description = description,
                DateTimePosted = dateTimePosted
            };

            newCommend.CommentID = competitionContext.AddComment(newCommend);
            return RedirectToAction("ViewCompetition", new { competitionId = (int)competitionId});
        }


    }
}
