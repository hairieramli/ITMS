using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITMS.Models
{
    public class ReportModel
    {
        public int IDrep
        {
            get; set;
        }

        public string UserName
        {
            get; set;
        }

        public int IDUser
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
        [DisplayFormat(DataFormatString ="{0:dd MMM yyy hh:mm}", ApplyFormatInEditMode =true)]
        public DateTime submittedDate
        {
            get; set;
        }

        public string ticketStatus
        {
            get;set;
        }

        public int IDtechnician
        {
            get; set;
        }

        public string technician
        {
            get; set;
        }

        public int priority
        {
            get;set;
        }

        public string task_type
        {
            get;set;
        }

        public int IDTicket
        {
            get;set;
        }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime TicketDate
        {
            get;set;
        }

        public string attendanceStatus
        {
            get;set;
        }

    }


}