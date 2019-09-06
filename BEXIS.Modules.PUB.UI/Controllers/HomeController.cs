using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("PUB", this.Session.GetTenant());
            return View();

        }

        public ActionResult Index2()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("PUB", this.Session.GetTenant());
            return View();
        }
    }
}
