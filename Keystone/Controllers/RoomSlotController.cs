using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class RoomSlotController : BaseController
    {
        private IRoomProvider _roomProvider;

        public RoomSlotController(ApplicationDbContext db,
                                  IFlashMessage flashMessage,
                                  IRoomProvider roomProvider) : base (db, flashMessage)
        {
            _roomProvider = roomProvider;
        }

        [HttpGet]
        public string GetRoomSlotDetail(string startDate, string endDate)
        {
            DateTime start = DateTime.Parse(startDate);
            DateTime end = DateTime.Parse(endDate);
            return _roomProvider.GetRoomSlotDetail(start, end);
        }
    }
}