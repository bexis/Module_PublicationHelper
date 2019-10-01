using BExIS.Dcm.UploadWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class UploadPublicationController : Controller
    {
        // GET: UploadPublication
        public ActionResult Index()
        {
            return RedirectToAction("UploadWizard", "Submit", new { area = "Dcm", type = DataStructureType.Unstructured});
        }
    }
}