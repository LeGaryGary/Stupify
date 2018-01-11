using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp.DataModels
{
    public class Server
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ServerId { get; set; }

        public virtual List<User> Users { get; set; }
    }
}