using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Linq;
using System.Xml;

namespace BExIS.Modules.PUB.UI.Helpers
{
    public class PUBSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            #region create entities

            using (var entityManager = new EntityManager())
            {
                // Entities
                Entity entity = entityManager.Entities.Where(e => e.Name.ToUpperInvariant() == "Publication".ToUpperInvariant()).FirstOrDefault();

                if (entity == null)
                {
                    entity = new Entity();
                    entity.Name = "Publication";
                    entity.EntityType = typeof(Dataset);
                    entity.EntityStoreType = typeof(PublicationStore);
                    entity.UseMetadata = true;
                    entity.Securable = true;

                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                    xmlDatasetHelper.AddReferenceToXml(xmlDoc, AttributeNames.name.ToString(), "pub", AttributeType.parameter.ToString(), "extra/modules/module");

                    entity.Extra = xmlDoc;

                    entityManager.Create(entity);
                }
            }


            #endregion

            #region SECURITY

            FeatureManager featureManager = new FeatureManager();
            OperationManager operationManager = new OperationManager();

            Feature DataCollectionFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Collection"));
            if (DataCollectionFeature == null) DataCollectionFeature = featureManager.Create("Data Collection", "Data Collection");

            Feature DataDiscovery = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Discovery"));
            if (DataDiscovery == null) DataDiscovery = featureManager.Create("Data Discovery", "Data Discovery");

            Feature PublicationCreationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publications Creation"));
            if (PublicationCreationFeature == null) PublicationCreationFeature = featureManager.Create("Publications Creation", "Publications Creation", DataCollectionFeature);

            Feature PublicationShowFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publications Show"));
            if (PublicationShowFeature == null) PublicationShowFeature = featureManager.Create("Publications Show", "Publications Show", DataDiscovery);

            operationManager.Create("PUB", "CreatePublication", "*", PublicationCreationFeature);
            operationManager.Create("PUB", "UploadPublication", "*", PublicationCreationFeature);
            operationManager.Create("PUB", "ShowPublication", "*", PublicationShowFeature);



            #endregion
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
