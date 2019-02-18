using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ITMS.Models;

namespace ITMS.Models
{
    public class InventoryModel
    {
        public int IDinv
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public string InvName
        {
            get; set;
        }

        [Required(ErrorMessage = "Please fill all the details")]
        public int InvQty
        {
            get;set;
        }

    }
}