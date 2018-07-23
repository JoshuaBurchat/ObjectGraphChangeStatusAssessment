using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Tests.Models
{
    class MultiLevelA : IChangeTrackable<string>
    {
        public string Id { get; set; }
        public ChangeType ChangeType { get; set; }

        public MultiLevelB B { get; set; }
        public List<MultiLevelC> Cs { get; set; }

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
        public string StringField { get; set; }
        public byte[] ByteArrayField { get; set; }
        public DateTime DateTimeField { get; set; }
        public TimeSpan TimeSpanField { get; set; }
    }
    class MultiLevelB : IChangeTrackable<string>
    {
        public MultiLevelA A { get; set; }
        public string Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public bool BooleanField { get; set; }
        public string StringField { get; set; }
        public int Int32Field { get; set; }
    }
    class MultiLevelC : BaseClassToIgnore, IChangeTrackable<string>
    {
        public MultiLevelA A { get; set; }
        public string Id { get; set; }
        public ChangeType ChangeType { get; set; }
    }
}
