using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Tests.Models
{
    public interface InterfaceToIgnore
    {
        bool BooleanField { get; set; }
        int Int32Field { get; set; }
    }
}
