using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Modules.PUB.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Entities.Common;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class UploadPublicationController : Controller
    {

        // GET: UploadPublication
        public ActionResult Index(long entityId, DataStructureType type = DataStructureType.Unstructured)
        {
            var fileExtentions = Helper.Settings.get("FileExtentions").ToString().Split(',');

            SelectFileViewModel model = new SelectFileViewModel();
            foreach (string extention in fileExtentions)
            {
                model.SupportedFileExtentions.Add(extention);
            }

            model.EntityId = entityId;

            return View("UploadFile", model);
        }

        public ActionResult SkipUpload(long entityId)
        {
            return RedirectToAction("Index", "ShowPublication", new { area = "PUB", id = entityId });
        }

        //copy of Dcm.UI.Helpers.DataASyncUploadHelper.FinishUpload (adapted)
        public ActionResult SaveFile(long entityId)
        {
            Dataset ds = null;
            DatasetVersion workingCopy = new DatasetVersion();
            string storePath = "";
            using (var dm = new DatasetManager())
            {
                ds = dm.GetDataset(entityId);

                // checkout the dataset, apply the changes, and check it in.
                if (dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUsernameOrDefault()))
                {
                    try
                    {
                        workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                        using (var unitOfWork = this.GetUnitOfWork())
                        {
                            workingCopy = unitOfWork.GetReadOnlyRepository<DatasetVersion>().Get(workingCopy.Id);

                            unitOfWork.GetReadOnlyRepository<DatasetVersion>().Load(workingCopy.ContentDescriptors);

                            storePath = SaveFileInContentDiscriptor(workingCopy);
                        }

                        workingCopy.StateInfo = new EntityStateInfo();
                        workingCopy.StateInfo.State = DatasetStateInfo.Valid.ToString();

                        //set modification
                        workingCopy.ModificationInfo = new EntityAuditInfo()
                        {
                            Performer = GetUsernameOrDefault(),
                            Comment = "File",
                            ActionType = AuditActionType.Create
                        };

                        dm.EditDatasetVersion(workingCopy, null, null, null);

                        string filename = Path.GetFileName(storePath);

                        //TaskManager TaskManager = (TaskManager)Session["TaskManager"];
                        //if (TaskManager.Bus.ContainsKey(TaskManager.FILENAME))
                        //{
                        //    filename = TaskManager.Bus[TaskManager.FILENAME]?.ToString();
                        //}

                        // ToDo: Get Comment from ui and users
                        dm.CheckInDataset(ds.Id, filename, GetUsernameOrDefault(), ViewCreationBehavior.None);

                        using (var userManager = new UserManager())
                        {
                            string username = GetUsernameOrDefault();
                            if(username != "DEFAULT")
                            {
                                User user = userManager.FindByNameAsync(username).Result;
                                var es = new EmailService();
                                es.Send(MessageHelper.GeFileUpdatHeader(ds.Id),
                                    MessageHelper.GetFileUploaddMessage(ds.Id, user.Name, filename),
                                    new List<string> { user.Email }, null, new List<string> { ConfigurationManager.AppSettings["SystemEmail"] });
                            }

                            
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return RedirectToAction("Index", "ShowPublication", new { area = "PUB", id = entityId });

        }

        //copy of Dcm.UI.Helpers.DataASyncUploadHelper.SelectFileProcess (adapted)

        /// <summary>
        /// Selected File store in the BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {
            //var TaskManager = (TaskManager)Session["TaskManager"];
            TaskManager TaskManager = new TaskManager();
            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                var path = Path.Combine(storepath, SelectFileUploader.FileName);

                SelectFileUploader.SaveAs(path);
                TaskManager.AddToBus(TaskManager.FILEPATH, path);

                TaskManager.AddToBus(TaskManager.FILENAME, SelectFileUploader.FileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, SelectFileUploader.FileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }

        ////copy of Dcm.UI.Helpers.DataASyncUploadHelper.SaveFileInContentDiscriptor (adapted)
        private string SaveFileInContentDiscriptor(DatasetVersion datasetVersion)
        {
            try
            {
                //XXX put function like GetStorePathOriginalFile or GetDynamicStorePathOriginalFile
                // the function is available in the abstract class datawriter
                ExcelWriter excelWriter = new ExcelWriter();
                // Move Original File to its permanent location

                string path = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetVersion.Dataset.Id.ToString(), "DatasetVersions");

                // if folder not exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                TaskManager TaskManager = (TaskManager)Session["TaskManager"];
                String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
                string storePath = Path.Combine(path, originalFileName);
                string dynamicStorePath = Path.Combine("Datasets", datasetVersion.Dataset.Id.ToString(), "DatasetVersions", originalFileName);
                string extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                Debug.WriteLine("extention : " + extention);
                

                //Why using the excel writer, isn't any function available in System.IO.File/ Directory, etc. Javad
                FileHelper.MoveFile(tempPath, storePath);

                string mimeType = MimeMapping.GetMimeMapping(originalFileName);

                //Register the original data as a resource of the current dataset version
                ContentDescriptor originalDescriptor = new ContentDescriptor()
                {
                    OrderNo = 1,
                    Name = "unstructuredData",
                    MimeType = mimeType,
                    URI = dynamicStorePath,
                    DatasetVersion = datasetVersion,
                };

                // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);

                return storePath;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        //copy of Dcm.UI.Helpers.DataASyncUploadHelper.GetUsernameOrDefault (unchanged)

        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }


    }
}