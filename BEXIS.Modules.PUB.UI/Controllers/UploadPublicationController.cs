﻿using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.PUB.UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.PUB.UI.Controllers
{
    public class UploadPublicationController : Controller
    {

        // GET: UploadPublication
        public ActionResult Index(long entityId)
        {
            SelectFileViewModel model = new SelectFileViewModel();
            model.SupportedFileExtentions.Add(".pdf");
            model.EntityId = entityId;

            return View("UploadFile", model);
        }

        public ActionResult SkipUpload(long entityId)
        {
            return RedirectToAction("ShowData", "Data", new { area = "DDM", id = entityId });
        }

        public ActionResult SaveFile(long entityId)
        {
            Dataset ds = null;
            DatasetVersion workingCopy = new DatasetVersion();

            string status = DatasetStateInfo.NotValid.ToString();

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

                            //set StateInfo of the previus version
                            if (workingCopy.StateInfo == null)
                            {
                                workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                                {
                                    State = status
                                };
                            }
                            else
                            {
                                workingCopy.StateInfo.State = status;
                            }

                            unitOfWork.GetReadOnlyRepository<DatasetVersion>().Load(workingCopy.ContentDescriptors);

                            SaveFileInContentDiscriptor(workingCopy);
                        }
                        dm.EditDatasetVersion(workingCopy, null, null, null);

                        // ToDo: Get Comment from ui and users
                        dm.CheckInDataset(ds.Id, "upload unstructured data", GetUsernameOrDefault(), ViewCreationBehavior.None);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return RedirectToAction("ShowData", "Data", new { area = "DDM", id = entityId });

        }

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


      

        private string SaveFileInContentDiscriptor(DatasetVersion datasetVersion)
        {
            try
            {
                //XXX put function like GetStorePathOriginalFile or GetDynamicStorePathOriginalFile
                // the function is available in the abstract class datawriter
                ExcelWriter excelWriter = new ExcelWriter();
                // Move Original File to its permanent location
                TaskManager TaskManager = (TaskManager)Session["TaskManager"];
                String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
                string storePath = excelWriter.GetFullStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
                string dynamicStorePath = excelWriter.GetDynamicStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
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