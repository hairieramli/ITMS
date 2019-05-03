using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;



namespace ITMS.Models
{
    public class TechReportModel
    {
        public int id
        {
            get; set;
        }
        public int IDrep
        {
            get; set;
        }

        public int IDTech
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public string rep_desc
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public string rep_title
        {
            get; set;
        }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime submittedDate
        {
            get; set;
        }

    }
}