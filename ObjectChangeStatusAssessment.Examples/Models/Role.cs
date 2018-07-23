﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Examples.Models
{
    public class Role : IChangeTrackable<int>
    {
        public int Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public int ActorId { get; set; }
        public Actor Actor { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string Name { get; set; }
    }
}
