using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.PUB.UI.Helpers
{
    public class PublicationStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                DatasetManager dm = new DatasetManager();
                MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
                var md = metadataStructureManager.Repo.Get(a => a.Name == "BE-PublicationSchema").FirstOrDefault();

                try
                {
                    var datasetIds = dm.DatasetRepo.Get(a => a.MetadataStructure.Id == md.Id).Select(a=>a.Id);
                    var datasetHelper = new XmlDatasetHelper();

                    var entities = datasetIds.Select(id => new EntityStoreItem() { Id = id, Title = datasetHelper.GetInformation(id, NameAttributeValues.title) });
                    return entities.ToList();
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public string GetTitleById(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var dm = new DatasetManager();

                try
                {
                    var datasetHelper = new XmlDatasetHelper();

                    return datasetHelper.GetInformation(id, NameAttributeValues.title);
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public List<EntityStoreItem> GetVersionsById(long id)
        {
            DatasetManager dm = new DatasetManager();
            List<EntityStoreItem> tmp = new List<EntityStoreItem>();
            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
                var datasetHelper = new XmlDatasetHelper();
                var versions = dm.GetDataset(id).Versions.OrderBy(v => v.Timestamp).ToList();

                foreach (var v in versions)
                {
                    tmp.Add(new EntityStoreItem()
                    {
                        Id = v.Id,
                        Title = datasetHelper.GetInformationFromVersion(v.Id, NameAttributeValues.title),
                        Version = versions.IndexOf(v) + 1,
                        CommitComment = "(" + v.Timestamp.ToString("dd.MM.yyyy HH:mm") + "): " + v.ChangeDescription
                    });
                }

                return tmp;
            }
            catch (Exception ex)
            {
                return tmp;
            }
            finally
            {
                dm.Dispose();
            }
        }

        
        public bool HasVersions()
        {
            return true;
        }

        public int CountVersions(long id)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
                var datasetHelper = new XmlDatasetHelper();

                int version = dm.GetDataset(id).Versions.Count;

                return version;
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                dm.Dispose();
            }
        }
    }
}