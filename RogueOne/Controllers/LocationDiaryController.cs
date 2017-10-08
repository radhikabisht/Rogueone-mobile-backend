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
        [Route("declineRequest")]
        public IHttpActionResult declineRequest([FromBody]string Username)
        {
            var userId = User.Identity.GetUserId();
            var Acceptor = db.Users.FirstOrDefault(x => x.UserName == Username);
            var Requestor = db.Users.Find(userId);
            var pen =  Acceptor.PendingRequests.Where(x => x.Requestor == Requestor).FirstOrDefault();
            var req = Requestor.ConnectRequests.Where(x => x.Acceptor == Acceptor).FirstOrDefault();
            Requestor.ConnectRequests.Remove(req);
            Acceptor.PendingRequests.Remove(pen);
            return Ok();
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
                if (location.tripName == null || location.tripName == "" || location.tripName == "My Diary")
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
                        .Where(x => x.TripName == location.tripName)
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
                    Comments = location.Comments
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
                badges.Add(new Badge() { BadgeName = badge });
            }
            if (entry == null)
            {
                entry = new LocationEntry()
                {
                    DateCreated = DateTime.Now,
                    Location = loc,
                    LocationBadge = badges,
                    Visits = visits,
                    Comments = location.Comments
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
        [HttpGet]
        //GET api/User/DiaryEntries
        [Route("UserDiaryEntries")]
        public List<LocationEntryViewModel> UserDiaryEntries(string Username)
        {
            string userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            if (Username != null && Username != "")
            {
                if (!db.Users.Where(x => x.UserName == Username).FirstOrDefault().UserSettings.Safemode)
                {
                    AppUser = db.Users.Where(x => x.UserName == Username).FirstOrDefault();
                }
                else {
                    return null;
                }
            }
            List<LocationEntryViewModel> diary = new List<LocationEntryViewModel>();
            if (AppUser.Diary != null && AppUser.Diary.Count != 0)
            {
                foreach (LocationEntry entry in AppUser.Diary)
                {
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
        //GET api/User/GetTrips
        [Route("GetUserTrips")]
        public List<TripView> GetUserTrips(string Username)
        {
            string userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            if (Username != null && Username != "")
            {
                if (!db.Users.Where(x => x.UserName == Username).FirstOrDefault().UserSettings.Safemode)
                {
                    AppUser = db.Users.Where(x => x.UserName == Username).FirstOrDefault();
                }
                else {
                    return null;
                }
            }
            
            var trips = AppUser.Trips;
            List<TripView> mytrips = new List<TripView>();
            if (trips != null) {
                foreach (Trip trip in trips) {
                    List<LocationEntryViewModel> levms = mapLocationEntrytoViewModel(trip);
                    TripView tripView = new TripView()
                    {
                        Description = trip.Description,
                        PlannedDuration = trip.PlannedDuration,
                        Destination = trip.Destination,
                        StartDate = trip.StartDate.ToString(),
                        TripMates = trip.TripMates,
                        TripName = trip.TripName,
                        TripEntries = levms,
                        TripID = trip.TripID
                    };
                    mytrips.Add(tripView);
                }
            }
            return mytrips;
        }
        [HttpGet]
        [Route("goIncognito")]
        public IHttpActionResult goIncognito() {
            try {
                string userId = User.Identity.GetUserId();
                var AppUser = db.Users.Find(userId);
                if (!AppUser.UserSettings.Safemode)
                {
                    AppUser.UserSettings.Safemode = true;
                    db.SaveChanges();
                }
                else {
                    AppUser.UserSettings.Safemode = false;
                    db.SaveChanges();
                }
                return Ok();
            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [Route("getIncognito")]
        public IHttpActionResult getIncognito()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var AppUser = db.Users.Find(userId);
                if (AppUser.UserSettings.Safemode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //GET api/User/GetTrips
        [Route("GetTrips")]
        public List<TripView> GetTrips()
        {
            string userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            var trips = AppUser.Trips;
            List<TripView> mytrips = new List<TripView>();
            if (trips != null)
            {
                foreach (Trip trip in trips)
                {
                    List<LocationEntryViewModel> levms = mapLocationEntrytoViewModel(trip);
                    TripView tripView = new TripView()
                    {
                        Description = trip.Description,
                        PlannedDuration = trip.PlannedDuration,
                        Destination = trip.Destination,
                        StartDate = trip.StartDate.ToString(),
                        TripMates = trip.TripMates,
                        TripName = trip.TripName,
                        TripEntries = levms,
                        TripID = trip.TripID
                    };
                    mytrips.Add(tripView);
                }
            }
            return mytrips;
        }
        private List<LocationEntryViewModel> mapLocationEntrytoViewModel(Trip trip)
        {
            List<LocationEntryViewModel> levms = new List<LocationEntryViewModel>();
            foreach (LocationEntry entry in trip.TripEntries) {
                LocationEntryViewModel levm = new LocationEntryViewModel()
                {
                    Address = entry.Location.DisplayFriendlyName,
                    Comments = entry.Comments,
                    DateCreated = entry.DateCreated.ToString(),
                    Latitude = entry.Location.Latitude,
                    Longitude = entry.Location.Longitude,
                    tripName = trip.TripName,
                    LocationEntryID = entry.DiaryEntryID,
                    BadgeNames = entry.LocationBadge.Select(x => x.BadgeName).ToList(),
                    CheckIns = entry.Visits.Select(x => x.DateCreated.ToString()).ToList()
                };
                levms.Add(levm);
            }
            return levms;
        }

        [HttpPost]
        [Route("emergencyCheckIn")]
        public IHttpActionResult emergencyCheckIn(LocationEntryViewModel entry) {
            var userId = User.Identity.GetUserId();
            var AppUser = db.Users.Find(userId);
            LocationEntry location = new LocationEntry();
            location.Comments = entry.Comments;
            location.DateCreated = DateTime.Now;
            location.Location = new Location() {
                DisplayFriendlyName = entry.Address,
                Latitude = entry.Latitude,
                Longitude = entry.Longitude             
            };
            AppUser.emergencyCheckIn = location;
            db.SaveChanges();
            return Ok();
        }
        [HttpGet]
        [Route("getEmergencyCheckIn")]
        public LocationEntryViewModel getEmergencyCheckIn(string Username)
        {
            var userId = User.Identity.GetUserId();
            var AppUser = db.Users.FirstOrDefault(x=>x.UserName == Username);
            LocationEntryViewModel loc = new LocationEntryViewModel();
            LocationEntry entry = AppUser.emergencyCheckIn;
            if (entry == null)
            {
                return null;
            }
            else {
                loc.Address = entry.Location.DisplayFriendlyName;
                loc.Comments = entry.Comments;
                loc.DateCreated = entry.DateCreated.ToString();
                loc.Latitude = entry.Location.Latitude;
                loc.Longitude = entry.Location.Longitude;
            }
            return loc;
        }
        [HttpPost]
        [Route("createTrip")]
        public IHttpActionResult createTrip(TripView tripView)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var AppUser = db.Users.Find(userId);
                Trip trip = new Trip()
                {
                    Description = tripView.Description,
                    PlannedDuration = tripView.PlannedDuration,
                    Destination = tripView.Destination,
                    StartDate = DateTime.Now,
                    TripName = tripView.TripName,
                    TripMates = tripView.TripMates
                };
                AppUser.Trips.Add(trip);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

    
}
