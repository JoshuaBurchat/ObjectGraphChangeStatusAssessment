using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class BoxOfficeDetails : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public double Amount { get; set; }
        //... other details, couldnt think of a another example
    }
}
