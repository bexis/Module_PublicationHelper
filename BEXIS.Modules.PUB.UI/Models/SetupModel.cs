using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Modules.PUB.UI.Models
{
    public class SetupModel
    {
        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a metadata structure.")]
        public long SelectedMetadataStructureId { get; set; }

        public List<ViewSelectItem> MetadataStructureList { get; set; }

        public SetupModel()
        {
            SelectedMetadataStructureId = -1;
            MetadataStructureList = new List<ViewSelectItem>();
        }


    }

    public class ViewSelectItem
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public ViewSelectItem(long id, string titel)
        {
            Id = id;
            Title = titel;
        }

    }
}