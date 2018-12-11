using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.TEMPLATE.UI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("TEMPLATE", this.Session.GetTenant());
            return View();

        }

        public ActionResult Index2()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("TEMPLATE", this.Session.GetTenant());
            return View();
        }
    }
}