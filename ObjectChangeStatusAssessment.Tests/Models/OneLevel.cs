using System;
using ObjectChangeStatusAssessment;

namespace ObjectChangeStatusAssessment.Tests.Models
{

    //Trying to cover all the types you might store in the database common for ORMs and 
    //Crud apps, hopefully didnt miss any major ones for the multi layer I went less
    public class OneLevel : BaseClassToIgnore, IChangeTrackable<string>, InterfaceToIgnore
    {
        public string Id { get; set; }
        public ChangeType ChangeType { get; set; }


        public bool BooleanField { get; set; }
        public byte ByteField { get; set; }
        public sbyte SByteField { get; set; }
        public char CharField { get; set; }
        public decimal DecimalField { get; set; }
        public double DoubleField { get; set; }
        public float SingleField { get; set; }
        public int Int32Field { get; set; }
        public uint UInt32Field { get; set; }
        public long Int64Field { get; set; }
        public ulong UInt64Field { get; set; }
        public byte[] ByteArrayField { get; set; }
        public DateTime DateTimeField { get; set; }
        public TimeSpan TimeSpanField { get; set; }

    }
}
