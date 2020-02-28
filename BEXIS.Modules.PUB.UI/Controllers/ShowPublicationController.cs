using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.PUB.UI.Controllers
{
    
    public class ShowPublicationController : Controller
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: ShowPublication
        //[BExISEntityAuthorize("Publication", typeof(Dataset), "id", RightType.Grant)]
        public ActionResult Index(long id, int version = 0)
        {
            DatasetManager dm = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                DatasetVersion dsv;
                ShowDataModel model = new ShowDataModel();

                string title = "";
                long metadataStructureId = -1;
                long dataStructureId = -1;
                long researchPlanId = 1;
                long versionId = 0;
                string dataStructureType = DataStructureType.Structured.ToString();
                bool downloadAccess = false;
                bool requestExist = false;
                bool requestAble = false;
                bool latestVersion = false;
                string isValid = "no";

                XmlDocument metadata = new XmlDocument();

                if (dm.IsDatasetCheckedIn(id))
                {
                    //get latest version
                    if (version == 0)
                    {
                        versionId = dm.GetDatasetLatestVersionId(id); // check for zero value
                        //get current version number
                        version = dm.GetDatasetVersions(id).OrderBy(d => d.Timestamp).Count();

                        latestVersion = true;
                    }
                    // get specific version
                    else
                    {
                        versionId = dm.GetDatasetVersions(id).OrderBy(d => d.Timestamp).Skip(version - 1).Take(1).Select(d => d.Id).FirstOrDefault();
                        latestVersion = versionId == dm.GetDatasetLatestVersionId(id);
                    }

                    dsv = dm.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo

                    if (dsv.StateInfo != null)
                    {
                        isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                    }

                    metadataStructureId = dsv.Dataset.MetadataStructure.Id;

                    //MetadataStructureManager msm = new MetadataStructureManager();
                    //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                    title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title); // this function only needs metadata and extra fields, there is no need to pass the version to it.
                    dataStructureId = dsv.Dataset.DataStructure.Id;
                    researchPlanId = dsv.Dataset.ResearchPlan.Id;
                    metadata = dsv.Metadata;

                    // check if the user has download rights
                    downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                        "Publication", typeof(Dataset), id, RightType.Read);

                    //// check if a reuqest of this dataset exist
                    //if (!downloadAccess)
                    //{
                    //    requestExist = HasOpenRequest(id);

                    //    if (UserExist() && HasRequestMapping(id)) requestAble = true;
                    //}

                  dataStructureType = DataStructureType.Unstructured.ToString();
                    

                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Show Data : " + title, this.Session.GetTenant());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Publication is just in processing.");
                }

                model = new ShowDataModel()
                {
                    Id = id,
                    Version = version,
                    VersionSelect = version,
                    VersionId = versionId,
                    LatestVersion = latestVersion,
                    Title = title,
                    MetadataStructureId = metadataStructureId,
                    DataStructureId = dataStructureId,
                    ResearchPlanId = researchPlanId,
                    ViewAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, "Publication", typeof(Dataset), id, RightType.Read),
                    GrantAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, "Publication", typeof(Dataset), id, RightType.Grant),
                    DataStructureType = dataStructureType,
                    DownloadAccess = downloadAccess,
                    RequestExist = requestExist,
                    RequestAble = requestAble
                };

                //set metadata in session
                Session["ShowDataMetadata"] = metadata;
                ViewData["VersionSelect"] = getVersionsSelectList(id, dm);
                ViewData["isValid"] = isValid;

                return View(model);
            }
            finally
            {
                dm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        private SelectList getVersionsSelectList(long id, DatasetManager datasetManager)
        {
            List<SelectListItem> tmp = new List<SelectListItem>();

            List<DatasetVersion> dsvs = datasetManager.GetDatasetVersions(id).OrderByDescending(d => d.Timestamp).ToList();

            dsvs.ForEach(d => tmp.Add(
                new SelectListItem()
                {
                    Text = (dsvs.IndexOf(d) + 1) + " " + getVersionInfo(d),
                    Value = "" + (dsvs.IndexOf(d) + 1)
                }
                ));

            return new SelectList(tmp, "Value", "Text");
        }

        private string getVersionInfo(DatasetVersion d)
        {
            StringBuilder sb = new StringBuilder();

            // modification, Performer and Comment exists (as indication for new version type tracking)
            if (d.ModificationInfo != null &&
                !string.IsNullOrEmpty(d.ModificationInfo.Performer) &&
                !string.IsNullOrEmpty(d.ModificationInfo.Comment))
            {

                // Metadata cration & edit
                if (d.ModificationInfo.Comment.Equals("Metadata") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Metadata creation (by {0}, {1})", d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Metadata") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Edit)
                {
                    sb.Append(String.Format("Metadata edited (by {0}, {1})", d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }

                //unstructured file upload & delete
                else if (d.ModificationInfo.Comment.Equals("File") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("File uploaded: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("File") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("File deleted: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }

                // structured data import & update & delete
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Data imported: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Edit)
                {
                    sb.Append(String.Format("Data added: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("Data deleted (by {0}, {1})", d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }

                // attachment 
                else if (d.ModificationInfo.Comment.Equals("Attachment") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Attachtment uploaded: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Attachment") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("Attachtment deleted: {0} (by {1}, {2})", Truncate(d.ChangeDescription, 30), d.ModificationInfo.Performer, d.Timestamp.ToString("dd.MM.yyyy")));
                }


                else
                {
                    sb.Append(d.ModificationInfo.Comment);
                    sb.Append(" - ");
                    sb.Append(d.ModificationInfo.ActionType);
                    sb.Append(" - ");
                    sb.Append(d.ModificationInfo.Performer);

                    // both exits - needs seperator
                    if (d.ModificationInfo != null &&
                        string.IsNullOrEmpty(d.ModificationInfo.Performer) &&
                        !string.IsNullOrEmpty(d.ModificationInfo.Comment) &&
                        !string.IsNullOrEmpty(d.ChangeDescription))
                    {
                        sb.Append(" : ");
                    }

                    //changedescription is not null or empty
                    if (!string.IsNullOrEmpty(d.ChangeDescription))
                    {
                        sb.Append(Truncate(d.ChangeDescription, 30));
                    }

                }

            }
            else
            {
                sb.Append(String.Format("{0} ({1})", Truncate(d.ChangeDescription, 30), d.Timestamp.ToString("dd.MM.yyyy")));
            }



            return sb.ToString();
        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        #region primary data

        //[MeasurePerformance
        [BExISEntityAuthorize("Publication", typeof(Dataset), "datasetID", RightType.Read)]
        public ActionResult ShowPrimaryData(long datasetID, int versionId)
        {
            Session["Filter"] = null;
            Session["Columns"] = null;
            Session["DownloadFullDataset"] = false;
            ViewData["DownloadOptions"] = null;
            IOUtility iOUtility = new IOUtility();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            //permission download
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                if (dm.IsDatasetCheckedIn(datasetID))
                {
                    // get latest or other datasetversion
                    DatasetVersion dsv = dm.GetDatasetVersion(versionId);
                    bool latestVersion = versionId == dm.GetDatasetLatestVersionId(datasetID);

                    
                    DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                    // TODO: refactor Download Right not existing, so i set it to read
                    bool downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                        "Publication", typeof(Dataset), datasetID, RightType.Read);

                    bool editRights = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                        "Publication", typeof(Dataset), datasetID, RightType.Write);

                    //TITLE
                    string title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title);

                    if (ds.Self.GetType() == typeof(UnStructuredDataStructure))
                    {
                        if (this.IsAccessible("MMM", "ShowMultimediaData", "multimediaData") && ConfigurationManager.AppSettings["useMultimediaModule"].ToLower().Equals("true"))
                            return RedirectToAction("multimediaData", "ShowMultimediaData", new RouteValueDictionary { { "area", "MMM" }, { "datasetID", datasetID }, { "versionId", versionId }, { "entityType" , "Publication"} });
                        else
                            return
                                PartialView(ShowPrimaryDataModel.Convert(datasetID,
                                versionId,
                                title,
                                ds,
                                SearchUIHelper.GetContantDescriptorFromKey(dsv, "unstructuredData"),
                                downloadAccess,
                                iOUtility.GetSupportedAsciiFiles(),
                                latestVersion, 
                                editRights));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
                }

                return PartialView(null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        #endregion

        #region entity references

        [BExISEntityAuthorize("Publication", typeof(Dataset), "id", RightType.Read)]
        public ActionResult ShowReferences(long id, int version)
        {
            var sourceTypeId = 0;

            //get the researchobject (cuurently called dataset) to get the id of a metadata structure
            Dataset researcobject = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
            long metadataStrutcureId = researcobject.MetadataStructure.Id;

            string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, new Dlm.Services.MetadataStructure.MetadataStructureManager());
            string entityType = xmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStrutcureId, new Dlm.Services.MetadataStructure.MetadataStructureManager());

            //ToDo in the entity table there must be the information
            EntityManager entityManager = new EntityManager();

            var entity = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

            var view = this.Render("DCM", "EntityReference", "Show", new RouteValueDictionary()
            {
                { "sourceId", id },
                { "sourceTypeId", entity.Id },
                { "sourceVersion", version }
            });

            return Content(view.ToHtmlString(), "text/html");
        }

        #endregion entity references

    
    }
}