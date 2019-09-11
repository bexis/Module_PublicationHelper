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
    }
}