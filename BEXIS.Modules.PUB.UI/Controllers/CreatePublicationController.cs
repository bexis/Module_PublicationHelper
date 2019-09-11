using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class CreatePublicationController : BaseController
    {
        // GET: CreatePublication
        public ActionResult Index()
        {
           
            using (var metadataStructureManager = new MetadataStructureManager())
            using (var entityManager = new EntityManager())
            using (var researchPlanManager = new ResearchPlanManager())
            {
                //get metadatastruture
                var md = metadataStructureManager.Repo.Get(a => a.Name == "BE-PublicationSchema").FirstOrDefault();

                //get entitytype
                var entityType = entityManager.FindByName("Publication");

                if (md != null)
                {
                    CreateTaskmanager taskManager = new CreateTaskmanager();
                    taskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, md.Id);
                  
                    taskManager.AddToBus(CreateTaskmanager.SAVE_WITH_ERRORS, false);
                    taskManager.AddToBus(CreateTaskmanager.NO_IMPORT_ACTION, true);

                   // get existing researchPlan
                   string researchPlanName = "Research plan";
                   ResearchPlan researchPlan = researchPlanManager.Repo.Get(r => researchPlanName.Equals(r.Title)).FirstOrDefault();
                   taskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, researchPlan.Id);

                    //create unstructured datastructure
                    DataStructure dataStructure = CreateDataStructure("Publication-File") as UnStructuredDataStructure;
                    taskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, dataStructure.Id);

                    Session["CreateDatasetTaskmanager"] = taskManager;
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
            submitAction.ControllerName = "CreateDataset";
            submitAction.AreaName = "DCM";



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
    }
}