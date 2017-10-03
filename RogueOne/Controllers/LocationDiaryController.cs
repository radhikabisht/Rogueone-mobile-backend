using Microsoft.AspNet.Identity;
using RogueOne.Models;
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
        public async Task<IHttpActionResult> Diary() {

            var username = User.Identity.GetUserId();
            ApplicationUser user = db.Users.FirstOrDefault(x=>x.Id == username);
            var userid = user.UserName;
            return null;
        }
        //GET api/User/GetAppUsers
        [Route("GetAppUsers")]
        [System.Web.Mvc.OutputCache(Duration = 1000, VaryByParam = "none")]
        public IHttpActionResult GetAppUsers() {
            return null;
        }
        //GET api/User/FriendList
        [Route("FriendList")]
        public async Task<IHttpActionResult> FriendList() {
            return null;
        }
        //POST api/User/AddFriend
        [Route("AddFriend")]
        public async Task<IHttpActionResult> AddFriend(string Username)
        {
            return null;
        }
        //GET api/User/PendingRequests
        [Route("PendingRequests")]
        public async Task<IHttpActionResult> PendingRequests()
        {
            return null;
        }
        //POST api/User/CreateTrip
        [Route("CreateTrip")]
        public async Task<IHttpActionResult> CreateTrip()
        {
            return null;
        }
        //GET api/User/DiaryEntries
        [Route("DiaryEntries")]
        public async Task<IHttpActionResult> DiaryEntries()
        {
            return null;
        }
        //GET api/User/GetTrips
        [Route("GetTrips")]
        public async Task<IHttpActionResult> GetTrips()
        {
            return null;
        }
        //GET api/User/TripInfo
        [Route("TripInfo")]
        public async Task<IHttpActionResult> TripInfo(long tripID)
        {
            return null;
        }
    }
}
