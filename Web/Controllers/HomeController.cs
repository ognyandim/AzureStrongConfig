using System.Web.Mvc;
using Services.Interfaces;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMyService _myService;

        public HomeController(IMyService myService)
        {
            _myService = myService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ServiceWork()
        {
            ViewBag.Message = "My Service Work.";
            ViewBag.JobDone = _myService.DoWork();
            return View();
        }
    }
}