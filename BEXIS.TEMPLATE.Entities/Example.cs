using Vaiona.Entities.Common;

namespace BExIS.TEMPLATE.Entities
{
    public class Example : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        public virtual long Id { get; set; }

        /// <summary>
        /// Name of the Publication
        /// </summary>
        public virtual string Name { get; set; }

        //public virtual string Mask { get; set; }


        #endregion

        #region Associations    

        //public virtual LinkElement Parent { get; set; }

        //public virtual List<LinkElement> Children { get; set; }

        #endregion
    }
}
