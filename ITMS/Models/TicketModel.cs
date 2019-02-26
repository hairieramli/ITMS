using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ITMS.Models;
using System.ComponentModel.DataAnnotations;

namespace ITMS.Models
{
    public class TicketModel
    {
        QueryCode app = new QueryCode();

        public int IDticket
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public string task_type
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public int priority
        {
            get; set;
        }

        public int IDrep
        {
            get; set;
        }

        public string rep
        {
            get; set;
        }

        public int IDtechnician
        {
            get; set;
        }

        public string technician
        {
            get; set;
        }

        public string ticketStatus
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

        public DateTime TicketDate
        {
            get; set;
        }

        public DateTime submittedDate
        {
            get; set;
        }
    }
}