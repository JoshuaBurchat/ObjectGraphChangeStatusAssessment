using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class Actor : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public string Name { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
    }

}
