using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Controllers;
using BExIS.Modules.PUB.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Logging;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class CreatePublicationController : BaseController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: CreatePublication
        public ActionResult Index()
        {
            string mulitbleMetadataStructure = Helper.Settings.get("MultibleMetadataStructures").ToString();

            if (mulitbleMetadataStructure == "true")
            {
                SetupModel model = new SetupModel();

                using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
                {
                    IEnumerable<MetadataStructure> metadataStructureList = metadataStructureManager.Repo.Get();

                    foreach (MetadataStructure metadataStructure in metadataStructureList)
                    {
                        if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                            HasEntityTypeName(metadataStructure, "Publication"))
                        {
                            string title = metadataStructure.Name;

                            model.MetadataStructureList.Add(new ViewSelectItem(metadataStructure.Id, title));
                        }
                    }

                    model.MetadataStructureList.OrderBy(p => p.Title);
                }

               return View("ChooseMetadataStructure", model);
            }
            else
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                using (var entityManager = new EntityManager())
                using (var researchPlanManager = new ResearchPlanManager())
                {
                    //get metadatastruture
                    var metadataStructureName = Helper.Settings.get("DefaultMetadataStructure");

                    var md = metadataStructureManager.Repo.Get(a => a.Name == metadataStructureName.ToString()).FirstOrDefault();

                    //get entitytype
                    var entityType = entityManager.FindByName("Publication");

                    if (md != null)
                    {
                        CreateTaskmanager taskManager = new CreateTaskmanager();
                        taskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, md.Id);

                        taskManager.AddToBus(CreateTaskmanager.SAVE_WITH_ERRORS, true);
                        taskManager.AddToBus(CreateTaskmanager.NO_IMPORT_ACTION, true);

                        taskManager.AddToBus(CreateTaskmanager.INFO_ON_TOP_TITLE, "Create Publication");
                        taskManager.AddToBus(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION, "<p>Here you can enter metadata for your new dataset. The form varies according to the metadata structure you selected in the first step. Mandatory fields are indicated with an red asterisk. You can add, remove, or re - order elements(e.g.multiple Creators) using the buttons at the right.</p>");


                        // get existing researchPlan
                        string researchPlanName = "Research plan";
                        ResearchPlan researchPlan = researchPlanManager.Repo.Get(r => researchPlanName.Equals(r.Title)).FirstOrDefault();
                        taskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, researchPlan.Id);

                        //create unstructured datastructure
                        DataStructure dataStructure = CreateDataStructure("Publication") as UnStructuredDataStructure;
                        taskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, dataStructure.Id);

                        HttpContext.Session["CreateDatasetTaskmanager"] = taskManager;
                        setAdditionalFunctions();

                        var view = this.Render("DCM", "Form", "StartMetadataEditor", new RouteValueDictionary()
                        { });

                        return Content(view.ToHtmlString(), "text/html");
                    }

                    else
                    {
                        return Content("Metadata structure not exsits");
                    }
                }
            }
        }

        public ActionResult LoadMetaDataForm(SetupModel model)
        {
            if (model.SelectedMetadataStructureId == -1)
                ModelState.AddModelError("SelectedMetadataStructureId", "Please select a metadata structure.");

            if (ModelState.IsValid)
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                using (var entityManager = new EntityManager())
                using (var researchPlanManager = new ResearchPlanManager())
                {
                    var md = metadataStructureManager.Repo.Get(a => a.Id == model.SelectedMetadataStructureId).FirstOrDefault();

                    //get entitytype
                    var entityType = entityManager.FindByName("Publication");

                    if (md != null)
                    {
                        CreateTaskmanager taskManager = new CreateTaskmanager();
                        taskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, md.Id);

                        taskManager.AddToBus(CreateTaskmanager.SAVE_WITH_ERRORS, true);
                        taskManager.AddToBus(CreateTaskmanager.NO_IMPORT_ACTION, true);

                        taskManager.AddToBus(CreateTaskmanager.INFO_ON_TOP_TITLE, "Edit Publication");
                        taskManager.AddToBus(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION, "<p>Here you can enter metadata for your new dataset. The form varies according to the metadata structure you selected in the first step. Mandatory fields are indicated with an red asterisk. You can add, remove, or re - order elements(e.g.multiple Creators) using the buttons at the right.</p>");

                        // get existing researchPlan
                        string researchPlanName = "Research plan";
                        ResearchPlan researchPlan = researchPlanManager.Repo.Get(r => researchPlanName.Equals(r.Title)).FirstOrDefault();
                        taskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, researchPlan.Id);

                        //create unstructured datastructure
                        DataStructure dataStructure = CreateDataStructure("Publication") as UnStructuredDataStructure;
                        taskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, dataStructure.Id);

                        HttpContext.Session["CreateDatasetTaskmanager"] = taskManager;
                        setAdditionalFunctions();

                        var view = this.Render("DCM", "Form", "StartMetadataEditor", new RouteValueDictionary() { });

                        return Content(view.ToHtmlString(), "text/html");
                    }

                    else
                        return Content("Metadata structure not exsits");
                }

            }
            else
                return Content("Metadata structure not exsits");

        }

        public JsonResult Submit(bool valid)
        {
            // create and submit Dataset
            using (var createDatasetController = new CreateDatasetController())
            {
                try
                {
                    // how to hold the seesion: https://stackoverflow.com/questions/31388357/session-is-null-when-calling-method-from-one-controller-to-another-mvc
                    createDatasetController.ControllerContext = new ControllerContext(this.Request.RequestContext, createDatasetController);
                    long datasetId = createDatasetController.SubmitDataset(valid, "Publication");

                    //get groups from setting file
                    var adminGroup = Helper.Settings.get("adminGroup").ToString();
                    var pubAdminGroup = Helper.Settings.get("pubAdminGroup").ToString();
                    var pubInternGroup = Helper.Settings.get("pubInternGroup").ToString();


                    using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                    {
                        if(!String.IsNullOrEmpty(adminGroup))
                            entityPermissionManager.Create<Group>("administrator", "Publication", typeof(Dataset), datasetId, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                        if (!String.IsNullOrEmpty(pubAdminGroup))
                            entityPermissionManager.Create<Group>("publicationAdmin", "Publication", typeof(Dataset), datasetId, new List<RightType>() { RightType.Read, RightType.Write, RightType.Delete});
                        if (!String.IsNullOrEmpty(pubInternGroup))
                            entityPermissionManager.Create<Group>("1_publicationIntern", "Publication", typeof(Dataset), datasetId, new List<RightType>() { RightType.Read});
                    }

                    return Json(new { result = "redirect", url = Url.Action("Index", "UploadPublication", new { area = "Pub", entityId = datasetId }) }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //copy of BExIS.Modules.Dcm.UI.Controllers.CreateDatasetController. setAdditionalFunctions (adapted)
        private void setAdditionalFunctions()
        {
            CreateTaskmanager taskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            //set function actions of COPY, RESET,CANCEL,SUBMIT
            //ActionInfo copyAction = new ActionInfo();
            //copyAction.ActionName = "Index";
            //copyAction.ControllerName = "CreateDataset";
            //copyAction.AreaName = "DCM";

            //ActionInfo resetAction = new ActionInfo();
            //resetAction.ActionName = "Reset";
            //resetAction.ControllerName = "Form";
            //resetAction.AreaName = "DCM";

            //ActionInfo cancelAction = new ActionInfo();
            //cancelAction.ActionName = "Cancel";
            //cancelAction.ControllerName = "Form";
            //cancelAction.AreaName = "DCM";

            ActionInfo submitAction = new ActionInfo();
            submitAction.ActionName = "Submit";
            submitAction.ControllerName = "CreatePublication";
            submitAction.AreaName = "PUB";



            //taskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            //taskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            //taskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            taskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);
        }

       

        // create unstructured DataStructures
        private DataStructure CreateDataStructure(string fileType)
        {
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {
                UnStructuredDataStructure dataStructure = new UnStructuredDataStructure();

                // values of DataStructure
                string name = fileType;
                string description = "" + fileType;

                UnStructuredDataStructure existDS = dataStructureManager.UnStructuredDataStructureRepo.Get(s => name.Equals(s.Name)).FirstOrDefault();
                if (existDS == null)
                {
                    // create dataStructure
                    return dataStructureManager.CreateUnStructuredDataStructure(name, description);
                }
                else
                {
                    return existDS;
                }
            }
        }

        #region Helper

        private bool HasEntityTypeName(MetadataStructure metadataStructure, string entityName)
        {

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                {
                    foreach (var entity in tmp)
                    {
                        string tmpEntityClassPath = "";
                        if (entity.HasAttributes && entity.Attribute("name") != null)
                            tmpEntityClassPath = entity.Attribute("name").Value.ToLower();

                        if (tmpEntityClassPath.Equals(entityName.ToLower())) return true;
                    }
                }
            }
            return false;
        }

        #endregion

    }
}