using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class Movie : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //Owned by Movie
        public List<Review> Reviews { get; set; } = new List<Review>();

        //Not owned by Movie
        public List<Genre> Genres { get; set; } = new List<Genre>();

        //Owned By Movie
        public int BoxOfficeDetailsId { get; set; }
        public BoxOfficeDetails BoxOfficeDetails { get; set; }

        //Not owned by Movie
        public int DirectorId { get; set; }
        public Director Director { get; set; }

        //Owned by Movie
        public List<Role> CastRoles { get; set; } = new List<Role>();

    }
}
