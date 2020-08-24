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

                    //add to Extra

                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                    xmlDatasetHelper.AddReferenceToXml(xmlDoc, AttributeNames.name.ToString(), "pub", AttributeType.parameter.ToString(), "extra/modules/module");

                    entity.Extra = xmlDoc;

                    entityManager.Create(entity);
                }
            }


            #endregion

            #region SECURITY

            using (FeatureManager featureManager = new FeatureManager())
            using (OperationManager operationManager = new OperationManager())
            {

                Feature PublicationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publications"));
                if (PublicationFeature == null) PublicationFeature = featureManager.Create("Publications", "Publications");

                Feature PublicationCreationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publication Creation"));
                if (PublicationCreationFeature == null) PublicationCreationFeature = featureManager.Create("Publication Creation", "Publication Creation", PublicationFeature);

                Feature PublicationShowFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publication Show"));
                if (PublicationShowFeature == null) PublicationShowFeature = featureManager.Create("Publication Show", "Publication Show", PublicationFeature);

                operationManager.Create("PUB", "CreatePublication", "*", PublicationCreationFeature);
                operationManager.Create("PUB", "UploadPublication", "*", PublicationCreationFeature);
                operationManager.Create("PUB", "ShowPublication", "*", PublicationShowFeature);


            }
            #endregion
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
