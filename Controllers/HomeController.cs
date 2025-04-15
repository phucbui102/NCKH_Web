using NCKH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using MySql.Data.MySqlClient;
using System.Web.Mvc;
using System.Data;

namespace NCKH.Controllers
{
	public class HomeController : Controller
	{
        private DatabaseHelper dbHelper = new DatabaseHelper();

        // Hiển thị danh sách users
        public ActionResult Index()
        {
            DataTable users = dbHelper.GetUsers();
            return View(users);
        }
        protected override void OnActionExecuting(ActionExecutingContext allprint)
        {
            // Ensure dbHelper is initialized before use
            ViewData["Reports"] = dbHelper.GetUnassignedReports();
            ViewBag.ReportCount = dbHelper.GetReportCount();

            base.OnActionExecuting(allprint);
        }


        public ActionResult main()
        {
            if (Session["name"] == null)
            {
                return RedirectToAction("Login", "Home"); // Chưa đăng nhập, quay lại trang đăng nhập
            }
            DataTable report    = dbHelper.getReport();
            ViewBag.DeviceCount = dbHelper.GetDeviceCount();
            ViewBag.ErrorDevice = dbHelper.GetErrorDevice();
            ViewBag.Username = Session["name"];
            return View(report);
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (dbHelper.Login(username, password))
            {
                Session["name"] = username;
                return RedirectToAction("main");
            }
            else
            {
                ViewBag.ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string username, string password)
        {
            if (dbHelper.Register(username, password))
            {
                ViewBag.Message = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        // Hiển thị trang phân công công việc
        public ActionResult Division()
        {
            ViewData["Reports"]     = dbHelper.GetUnassignedReports();
            ViewData["Groups"]      = dbHelper.GetUserGroups();
            return View();
        }

        // Xử lý phân công công việc
        [HttpPost]
        public ActionResult AssignReport(int reportId, int groupId)
        {
            try
            {
                bool isAssigned = dbHelper.AssignReportToGroup(reportId, groupId);

                if (isAssigned)
                {
                    dbHelper.UpdateStatus();
                    TempData["SuccessMessage"] = "Phân công thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Division");
        }

        public ActionResult Room()
        {
            ViewData["Areas"] = dbHelper.GetAreas();
            DataTable rooms = dbHelper.GetRooms();
            return View(rooms);
        }

        [HttpPost]
        public ActionResult AddRoom(int id_area, string room_name)
        {
            dbHelper.AddRoom(id_area, room_name);
            return RedirectToAction("Room");
        }
        // Thêm mới dãy phòng
        [HttpPost]
        public ActionResult AddArea(string name)
        {

            try
            {
                dbHelper.AddArea(name);
                TempData["Success"] = "Thêm dãy thành công!";
                return RedirectToAction("Area");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Thêm dãy thất bại: " + ex.Message;
                return RedirectToAction("Area");
            }
        }

        // Chỉnh sửa tên dãy phòng
        [HttpPost]
        public ActionResult EditArea(int id, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                dbHelper.EditArea(id, name);
                TempData["Success"] = "Sửa dãy thành công!";
            }
            return RedirectToAction("Area");
        }

        // Xóa dãy phòng
        [HttpPost]
        public ActionResult DeleteArea(int id)
        {
            dbHelper.DeleteArea(id);
            TempData["Success"] = "Xóa dãy thành công!";
            return RedirectToAction("Area");
        }

        [HttpPost]
        public ActionResult EditRoom(int room_id, string room_name)
        {
            dbHelper.UpdateRoom(room_id, room_name);
            return RedirectToAction("Room");
        }

        [HttpPost]
        public ActionResult DeleteRoom(int room_id)
        {
            dbHelper.DeleteRoom(room_id);
            return RedirectToAction("Room");
        }

        //      public ActionResult Device()
        //{
        //          DataTable divice = dbHelper.GetDevices();//
        //          ViewData["Areas"] = dbHelper.GetAreaRoom();
        //          return View(divice);
        //}

        public ActionResult testDevice()
        {
            ViewData["Devices"] = dbHelper.GetDevices();
            ViewData["Areas"] = dbHelper.GetAreas();
            return View();
        }

        public ActionResult Area()
		{
            DataTable area = dbHelper.GetAreas();
            return View(area);
		}

        // Lấy danh sách phòng theo dãy (AJAX)
        public JsonResult GetRooms(int areaId)
        {
            try
            {
                DataTable rooms = dbHelper.GetRoomsByArea(areaId);
                var roomList = new List<object>();

                if (rooms != null && rooms.Rows.Count > 0)
                {
                    foreach (DataRow row in rooms.Rows)
                    {
                        roomList.Add(new { id = row["id"], name = row["name"] });
                    }
                }

                return Json(roomList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi khi lấy danh sách phòng: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Thêm thiết bị
        [HttpPost]
        public ActionResult AddDevice(string name, string status, int roomId)
        {
            dbHelper.AddDevice(name, status, roomId);
            return RedirectToAction("testDevice");
        }

        // Xóa thiết bị
        public ActionResult DeleteDevice(int id)
        {
            dbHelper.DeleteDevice(id);
            return RedirectToAction("testDevice");
        }
    }
}