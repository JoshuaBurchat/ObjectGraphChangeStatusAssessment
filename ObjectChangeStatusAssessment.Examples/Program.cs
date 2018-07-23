using AutoMapper;
using ObjectChangeStatusAssessment.Examples.Dal;
using ObjectChangeStatusAssessment.Examples.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Using Automap to simulate disconnected environment
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Movie, Movie>();
                cfg.CreateMap<BoxOfficeDetails, BoxOfficeDetails>();
            });
            MovieStorage storage = new MovieStorage();

            var dbVersionOfLotr = storage.GetMovie(1);
            //this is crude i know
            var lotr = Mapper.Map<Movie>(dbVersionOfLotr);
            lotr.BoxOfficeDetails = Mapper.Map<BoxOfficeDetails>(lotr.BoxOfficeDetails);

            //Add an actor
            var ian = new Actor() { Name = "Ian McKellen", Id = 0 };
            var gandalf = new Role() { Actor = ian, ActorId = ian.Id, Name = "Gandalf", MovieId = lotr.Id, Movie = lotr };
            ian.Roles.Add(gandalf);
            lotr.CastRoles.Add(gandalf);


            //Add/Remove Genre Comedy
            lotr.Genres.Add(storage.GetGenres().FirstOrDefault(c => c.Id == 3));
            lotr.Genres.RemoveAt(0);

            //Change Name
            lotr.Name += " The Fellowship of the Ring";

            lotr.BoxOfficeDetails.Amount = 100;

            storage.SaveMovie(lotr);


            Console.Read();
        }
    }
}
