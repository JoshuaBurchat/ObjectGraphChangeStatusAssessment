using ObjectChangeStatusAssessment;
using ObjectChangeStatusAssessment.Examples.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Dal
{
    public class MovieStorage
    {


        //This isnt a real DAL and not the way I would write one by any means, just used to 
        //be an example of how you can sort your records out for updating
        //

        private Dictionary<int, Genre> _genres = new Dictionary<int, Genre>
        {
            { 1, new Genre() { Id = 1, Name= "Action"} },
            { 2, new Genre() { Id = 2, Name= "Adventure"} },
            { 3, new Genre() { Id = 3, Name= "Comedy"} },
            { 4, new Genre() { Id = 4, Name= "Fantasy"} },
            { 5, new Genre() { Id = 5, Name= "Sci-Fi"} }
        };
        private Dictionary<int, Actor> _actors = new Dictionary<int, Actor>
        {
            { 1, new Actor() { Id = 1, Name = "Elijah Wood" } },
            { 2, new Actor() { Id = 2, Name = "Cate Blanchett" } },
            { 3, new Actor() { Id = 3, Name = "Viggo Mortensen" } },
            { 4, new Actor() { Id = 4, Name = "Jeff Goldblum" } }
        };
        private List<Movie> _movies = new List<Movie>();
        private Movie BuildLotr()
        {
            Movie lotr = new Movie() { Id = 1, Name = "The Lord of the Rings: ", Description = "The Lord of the Rings: The Fellowship of the Ring" };
            lotr.BoxOfficeDetails = new BoxOfficeDetails() { Amount = 315544750, Id = 1, Movie = lotr, MovieId = lotr.Id };
            lotr.BoxOfficeDetailsId = lotr.BoxOfficeDetails.Id;
            lotr.Director = new Director() { Name = "Peter Jackson", Id = 1 };
            lotr.Director.Movies.Add(lotr);
            lotr.DirectorId = lotr.Director.Id;
            lotr.Genres.Add(_genres[1]);
            _genres[1].Movies.Add(lotr);
            lotr.Genres.Add(_genres[2]);
            _genres[2].Movies.Add(lotr);
            lotr.Genres.Add(_genres[4]);
            _genres[4].Movies.Add(lotr);
            lotr.CastRoles.Add(new Role() { Id = 1, Actor = _actors[1], ActorId = _actors[1].Id, Movie = lotr, MovieId = lotr.Id, Name = "Frodo" });
            lotr.CastRoles.Add(new Role() { Id = 2, Actor = _actors[2], ActorId = _actors[2].Id, Movie = lotr, MovieId = lotr.Id, Name = "Galadriel" });
            lotr.CastRoles.Add(new Role() { Id = 3, Actor = _actors[3], ActorId = _actors[3].Id, Movie = lotr, MovieId = lotr.Id, Name = "Aragorn" });
            foreach (var role in lotr.CastRoles) role.Actor.Roles.Add(role);
            lotr.Reviews.Add(new Review() { MovieId = lotr.Id, Movie = lotr, Stars = 5, Content = "Review LOTR", Id = 1, WrittenBy = "Some Dude" });

            return lotr;
        }
        private Movie BuildThor()
        {
            Movie thor = new Movie() { Id = 2, Name = "Thor: Ragnarok", Description = "Thor: Ragnarok" };
            thor.BoxOfficeDetails = new BoxOfficeDetails() { Amount = 315058289, Id = 2, Movie = thor, MovieId = thor.Id };
            thor.BoxOfficeDetailsId = thor.BoxOfficeDetails.Id;
            thor.Director = new Director() { Name = "Taika Waititi", Id = 2 };
            thor.Director.Movies.Add(thor);
            thor.DirectorId = thor.Director.Id;
            thor.Genres.Add(_genres[1]);
            _genres[1].Movies.Add(thor);
            thor.Genres.Add(_genres[2]);
            _genres[2].Movies.Add(thor);
            thor.Genres.Add(_genres[3]);
            _genres[3].Movies.Add(thor);
            thor.CastRoles.Add(new Role() { Id = 4, Actor = _actors[2], ActorId = _actors[2].Id, Movie = thor, MovieId = thor.Id, Name = "Hela" });
            thor.CastRoles.Add(new Role() { Id = 5, Actor = _actors[4], ActorId = _actors[4].Id, Movie = thor, MovieId = thor.Id, Name = "Grandmaster" });
            foreach (var role in thor.CastRoles) role.Actor.Roles.Add(role);
            thor.Reviews.Add(new Review() { MovieId = thor.Id, Movie = thor, Stars = 4, Content = "Review Thor", Id = 2, WrittenBy = "Some Dude" });

            return thor;
        }
        private ChangeAssessor<int> _changeAssessor;
        private void InitializeAssessor()
        {
            _changeAssessor = new ChangeAssessor<int>()
                //These are all sub properties that are only related to one movie
                .AddOwnerMapping<Movie>(m => m.BoxOfficeDetails)
                .AddOwnerMapping<Movie>(m => m.CastRoles)
                //Role a can create an actor but not delete them
                .AddOwnerMapping<Role>(m => m.Actor, update: false, delete: false)
                .AddOwnerMapping<Movie>(m => m.Reviews)
                //You can add a director through a movie but not delete or update them
                .AddOwnerMapping<Movie>(m => m.Director, update: false, delete: false)
            ;
        }
        public MovieStorage()
        {
            _movies.Add(BuildLotr());
            _movies.Add(BuildThor());
            InitializeAssessor();
        }

        public Movie GetMovie(int id)
        {
            return _movies.FirstOrDefault(i => i.Id == id);
        }
        public Genre[] GetGenres()
        {
            return _genres.Values.ToArray();
        }
        public Actor[] GetActors()
        {
            return _actors.Values.ToArray();
        }


        public void SaveMovie(Movie movie)
        {

            //This is a long function that sgows how you can direct the flow of changes, 
            //This could be better suited to the business layer, and should have all paths
            //transactionalize.  It can be dried up as well for your real implementation
            //EF makes this a bit easier see Julie Lermans pattern here: https://msdn.microsoft.com/en-us/magazine/mt767693.aspx
            //This will save you a lot of effort.
            //Maybe I will come out with an extension to this library to help map this to EF

            var existingMovie = this.GetMovie(movie.Id);
            if (existingMovie == null)
            {
                Console.WriteLine("New Movie saved");
                //Steps/SQL/EF/ whatever to persist a new movie
            }
            else
            {
                var changeAssessment = _changeAssessor.SetChangeStatus(movie, existingMovie);

                if (movie.ChangeType == ChangeType.Updated)
                {
                    Console.WriteLine("Movie root updated");
                    //Steps/SQL/EF/ whatever to persist a new movie
                }

                if (movie.Director.ChangeType == ChangeType.Added)
                {
                    Console.WriteLine("Diretor {0} Created and assign it to movie", movie.Director.Name);
                    //Relationship removal may be present for this as well, but changing the actor Id will do
                }
                if (movie.BoxOfficeDetails.ChangeType == ChangeType.Updated || movie.BoxOfficeDetails.ChangeType == ChangeType.Added)
                {
                    //Note if its an add this means the ID has been replaced but that could be treated like an update as well
                    Console.WriteLine("Box Office details changed to {0}", movie.BoxOfficeDetails.Amount);
                }


                foreach (var genreMapped in changeAssessment.Relationships.Where(r => r.Value is Genre))
                {
                    if (genreMapped.ChangeType == ChangeType.Added)
                    {
                        Console.WriteLine("Genre Added {0}", _genres[genreMapped.Value.Id].Name);
                    }
                    else if (genreMapped.ChangeType == ChangeType.Deleted)
                    {
                        Console.WriteLine("Genre Deleted {0}", _genres[genreMapped.Value.Id].Name);
                    }
                }
                foreach (var review in changeAssessment.OwnedEntities.OfType<Review>())
                {
                    if (review.ChangeType == ChangeType.Added)
                    {
                        Console.WriteLine("Review Added {0} Stars", review.Stars);
                    }
                    else if (review.ChangeType == ChangeType.Deleted)
                    {
                        Console.WriteLine("Genre Deleted Id {0} ", review.Id);
                    }
                }

                foreach (var review in changeAssessment.OwnedEntities.OfType<Review>())
                {
                    if (review.ChangeType == ChangeType.Added)
                    {
                        Console.WriteLine("Review Added {0} Stars", review.Stars);
                    }
                    else if (review.ChangeType == ChangeType.Deleted)
                    {
                        Console.WriteLine("Genre Deleted Id {0} ", review.Id);
                    }
                }

                foreach (var role in changeAssessment.OwnedEntities.OfType<Role>())
                {
                    //Add actor first if flagged to add or if the its a new actor
                    if (role.Actor.ChangeType == ChangeType.Added || !_actors.ContainsKey(role.Actor.Id))
                    {
                        Console.WriteLine("Actor Added {0}", role.Actor.Name);
                    }

                    if (role.ChangeType == ChangeType.Added)
                    {
                        Console.WriteLine("Actor Added {0} for role {1}", role.Actor.Name, role.Name);
                      
                    }
                    else if (role.ChangeType == ChangeType.Deleted)
                    {
                        Console.WriteLine("Actor Remove {0} from role {1}", role.Actor.Name, role.Name);
                    }
                    else if (role.ChangeType == ChangeType.Updated)
                    {
                        Console.WriteLine("Role {1} Update for actor {0}", role.Actor.Name, role.Name);
                    }
                }

            }

        }

    }
}
