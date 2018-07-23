using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class Genre : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public string Name { get; set; }
        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}
