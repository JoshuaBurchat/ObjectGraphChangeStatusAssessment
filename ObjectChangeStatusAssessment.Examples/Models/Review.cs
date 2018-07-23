using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class Review : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string Content { get; set; }
        public string WrittenBy { get; set; }

        public int Stars { get; set; }
    }
}
