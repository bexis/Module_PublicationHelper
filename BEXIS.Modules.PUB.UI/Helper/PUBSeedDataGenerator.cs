using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Linq;

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
                    entity.Name = "Dataset";
                    entity.EntityType = typeof(Dataset);
                    entity.EntityStoreType = typeof(PublicationStore);
                    entity.UseMetadata = true;
                    entity.Securable = true;

                    entityManager.Create(entity);
                }
            }


            #endregion

            #region SECURITY

            FeatureManager featureManager = new FeatureManager();
            OperationManager operationManager = new OperationManager();

            Feature DataCollectionFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Collection"));
            if (DataCollectionFeature == null) DataCollectionFeature = featureManager.Create("Data Collection", "Data Collection");

            Feature PublicationCreationFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publication Creation"));
            if (PublicationCreationFeature == null) PublicationCreationFeature = featureManager.Create("Publication Creation", "Publication Creation", DataCollectionFeature);

            operationManager.Create("PUB", "CreatePublication", "*", PublicationCreationFeature);
            operationManager.Create("PUB", "UploadPublication", "*", PublicationCreationFeature);


            #endregion
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
