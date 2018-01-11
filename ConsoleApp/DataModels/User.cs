using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConsoleApp.DataModels
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }

        public virtual List<ServerUser> ServerUsers { get; set; }
    }
}
