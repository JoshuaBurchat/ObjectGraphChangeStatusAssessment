using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    public class ChangeAssessorSubPropertyOwnershipTests
    {

        /*The owner ship of a sub property may differ based on how your persistence layer works
         Here are a few cases
         Sub Property Shared by other Entities 
            [  { id : 1, b : { id : 11 } : bType }: atype, { id : 2, b : { id : 11 } : bType }: atype ]
            In this case bType can be shared between many aType
           

         Sub Property Owned by single entity
            [  { id : 1, b : { id : 11 } : bType }: atype, { id : 2, b : { id : 22 } : bType }: atype ]
            In this case bType WONT be  shared between many aType, and its a one to one relationship (even if type b has its own id in persistence)
       
         //Sub Property Owned
         //Sub Property RelationShip
        */

    }
}
