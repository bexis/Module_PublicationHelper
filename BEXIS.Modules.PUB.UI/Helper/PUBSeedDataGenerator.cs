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
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
