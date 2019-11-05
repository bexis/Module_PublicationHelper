using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace BExIS.Modules.PUB.UI.Models
{
    public class SelectFileViewModel
    {
        public HttpPostedFileBase file;
        public List<string> serverFileList = new List<string>();
        public List<Error> ErrorList = new List<Error>();
        public String SelectedFileName = "";
        public String SelectedServerFileName = "";
        public Stream fileStream;

        public List<string> SupportedFileExtentions = new List<string>();

        public long EntityId;
    }
}