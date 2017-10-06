using Microsoft.AspNet.Identity;
using RogueOne.Models;
using RogueOne.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RogueOne.Controllers
{
    [Authorize]
    [RoutePrefix("api/User")]
    public class LocationDiaryController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        //GET api/User/Diary
        [Route("Diary")]
        public HomeScreenViewModel GetDiary() {
            var userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == userId);
            return new HomeScreenViewModel() {
                Diary = user.Diary,
                TripEntries = user.Trips,
                UserSettings = user.UserSettings,
                PendingRequests = user.PendingRequests == null ? 0 : user.PendingRequests.Count,
                NoOfFriends = user.Friends == null ? 0 : user.Friends.Count
            };
        }
        //GET api/User/AppUsers
        [Route("AppUsers")]
        [System.Web.Mvc.OutputCache(Duration = 1000, VaryByParam = "none")]
        public List<string> GetAppUsers() {
            List<string> users = new List<string>();
            var userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(userId);
            List<string> Friends = user.Friends.Select(x => x.Id).ToList();
            Friends.Add(userId);
            List<string> PotFriends;
            if (Friends != null)
                PotFriends = db.Users.Where(x => !Friends.Contains(x.Id)).Select(x => x.UserName).ToList();
            else
                PotFriends = db.Users.Where(x => x.Id != userId).Select(x => x.UserName).ToList();
            return PotFriends;
        }
        [HttpGet]
        [Route("getMyTrips")]
        public List<String> getMyTrips() {
            var userId = User.Identity.GetUserId();
            List<String> tripnames = db.Users.Find(userId).Trips.Select(x => x.TripName).ToList();
            return tripnames;
        }
        [HttpGet]
        //GET api/User/FriendList
        [Route("FriendList")]
        public List<String> FriendList() {
            var userId = User.Identity.GetUserId();
            List<String> friends = db.Users.Find(userId).Friends.Select(x => x.UserName).ToList();
            return friends;
        }
        [HttpPost]
        //POST api/User/AddFriend
        [Route("FriendRequest")]
        public IHttpActionResult AddFriend([FromBody]string Username)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var Acceptor = db.Users.FirstOrDefault(x => x.UserName == Username);
                var Requestor = db.Users.Find(userId);
                Request pendingRequest = new Request() {
                    Accepted = false,
                    Acceptor = Acceptor,
                    Requestor = Requestor
                };
                if (Acceptor.PendingRequests.Select(x => x.Requestor.UserName).Contains(Requestor.UserName))
                {
                    return BadRequest("friend request already sent");
                }
                Acceptor.PendingRequests.Add(pendingRequest);
                Requestor.ConnectRequests.Add(pendingRequest);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e) {
                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        //POST api/User/ConfirmRequest
        [Route("ConfirmRequest")]
        public IHttpActionResult ConfirmRequest([FromBody]string Username)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var Requestor = db.Users.FirstOrDefault(x => x.UserName == Username);
                var Acceptor = db.Users.Find(userId);
                var ConnectRequest = Requestor
                    .ConnectRequests
                    .Where(x => x.AcceptorID == Acceptor.Id)
                    .FirstOrDefault();
                if (ConnectRequest == null || ConnectRequest.Accepted) {
                    return BadRequest("Invalid Request");
                }
                else
                {
                    ConnectRequest.Accepted = true;
                }
                Acceptor.Friends.Add(Requestor);
                Requestor.Friends.Add(Acceptor);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        //GET api/User/PendingRequests
        [Route("PendingRequests")]
        public List<String> PendingRequests()
        {
            var userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            List<String> prs = AppUser
                .PendingRequests
                .Where(x => !x.Accepted)
                .Select(x => x.Requestor.UserName).ToList();
            return prs;
        }
        //POST api/User/CreateTrip
        [Route("CreateTrip")]
        public IHttpActionResult CreateTrip(Trip tripData)
        {
            return null;
        }
        [HttpGet]
        //GET api/User/DiaryEntries
        [Route("DiaryEntries")]
        public List<LocationEntryViewModel> DiaryEntries()
        {
            var userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            List<LocationEntryViewModel> diary = new List<LocationEntryViewModel>();
            if (AppUser.Diary != null && AppUser.Diary.Count != 0) {
                foreach (LocationEntry entry in AppUser.Diary) {
                    var diaryEntry = new LocationEntryViewModel()
                    {
                        Address = entry.Location.DisplayFriendlyName,
                        BadgeNames = entry.LocationBadge.Select(x => x.BadgeName).ToList(),
                        CheckIns = entry.Visits.Select(x => x.DateCreated.ToString()).ToList(),
                        Latitude = entry.Location.Latitude,
                        Longitude = entry.Location.Longitude,
                        DateCreated = entry.DateCreated.ToString(),
                        Comments = entry.Comments,
                        LocationEntryID = entry.DiaryEntryID
                    };
                    diary.Add(diaryEntry);
                }
            }
            return diary;
        }
        [HttpPost]
        [Route("createEntry")]
        public IHttpActionResult createEntry(LocationEntryViewModel location) {
            if (location == null) {
                return BadRequest();
            }
            try
            {
                var userId = User.Identity.GetUserId();
                var AppUser = db.Users.Find(userId);
                if (location.tripName == null || location.tripName == "")
                {
                    LocationEntry entry = AppUser.Diary
                        .Where(x => x.Location.DisplayFriendlyName == location.Address)
                        .FirstOrDefault();
                    if (!createLocationEntry(location, entry, AppUser))
                    {
                        return BadRequest();
                    };

                }
                else
                {
                    LocationEntry tripentry = AppUser
                        .Trips
                        .SelectMany(x => x.TripEntries)
                        .Where(x => x.Location.DisplayFriendlyName == location.Address)
                        .FirstOrDefault();
                    if (!createTripEntry(location, tripentry, AppUser))
                    {
                        return BadRequest();
                    };
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        private bool createTripEntry(LocationEntryViewModel location, LocationEntry entry, ApplicationUser user)
        {
            Location loc = new Location()
            {
                DisplayFriendlyName = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
            };
            CheckIn checkIn = new CheckIn()
            {
                DateCreated = DateTime.Now,
                Location = loc,
            };
            List<CheckIn> visits = new List<CheckIn>() { checkIn };
            List<Badge> badges = new List<Badge>();
            List<Badge> badgesInDb = db.Badges.ToList();
            Trip trip = user.Trips.Where(x => x.TripName == location.tripName).FirstOrDefault();
            if (trip == null) {
                return false;
            }
            foreach (string badge in location.BadgeNames)
            {
                if (badgesInDb.Where(x => x.BadgeName == badge) == null)
                {
                    badges.Add(new Badge() { BadgeName = badge });
                }
                else {
                    badges.Add(badgesInDb.Where(x => x.BadgeName == badge).FirstOrDefault());
                }
            }
            if (entry == null)
            {
                entry = new LocationEntry()
                {
                    DateCreated = DateTime.Now,
                    Location = loc,
                    LocationBadge = badges,
                    Visits = visits,
                };
                trip.TripEntries.Add(entry);
            }
            else
            {
                entry.Visits.Add(checkIn);
                entry.LocationBadge = badges;
            }
            db.SaveChanges();
            return true;
        }

        private bool createLocationEntry(LocationEntryViewModel location, LocationEntry entry,ApplicationUser user)
        {
            Location loc = new Location()
            {
                DisplayFriendlyName = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
            };
            CheckIn checkIn = new CheckIn()
            {
                DateCreated = DateTime.Now,
                Location = loc,
            };
            List<CheckIn> visits = new List<CheckIn>() { checkIn };
            List<Badge> badges = new List<Badge>();
            List<Badge> badgesInDb = db.Badges.ToList();
            foreach (string badge in location.BadgeNames)
            {
                if (badgesInDb.Where(x => x.BadgeName == badge) == null)
                {
                    badges.Add(new Badge() { BadgeName = badge });
                }
            }
            if (entry == null)
            {
                entry = new LocationEntry()
                {
                    DateCreated = DateTime.Now,
                    Location = loc,
                    LocationBadge = badges,
                    Visits = visits,
                };
                user.Diary.Add(entry);
            }
            else
            {
                entry.Visits.Add(checkIn);
                entry.LocationBadge = badges;
            }
            db.SaveChanges();
            return true;
        }

        //GET api/User/GetTrips
        [Route("Trips")]
        public List<TripView> GetTrips()
        {
            return null;
        }
        //GET api/User/TripInfo
        [Route("TripInfo")]
        public Trip TripInfo(long tripID)
        {
            return null;
        }
    }

    public class TripView
    {
    }
}
