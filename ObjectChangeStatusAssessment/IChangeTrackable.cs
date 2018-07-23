using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment
{

    //TODO in the future make the key check more flexible. Maybe with reflection. I had done this to suit current needs
    //And assuming I/you will always use a single Id as a key, but thats not always the case, or naming could differ, therefore
    //this should be enhanced in the future. 
    //Another thought is to remove this interface and have it part of the ChangeAssessor as an abstract function to assess the keys of different types

    public interface IChangeTrackable<TKey>
    {
        TKey Id { get; set; }
        ChangeType ChangeType { get; set; }
    }
}
