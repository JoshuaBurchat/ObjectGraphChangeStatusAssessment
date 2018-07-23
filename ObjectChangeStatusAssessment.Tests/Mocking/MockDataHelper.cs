using AutoMapper;
using ObjectChangeStatusAssessment.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment.Tests.Mocking
{
    static class MockDataHelper
    {
        private static Random _rnd = new Random();

        static MockDataHelper()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<OneLevel, OneLevel>();
                cfg.CreateMap<MultiLevelA, MultiLevelA>();
                cfg.CreateMap<MultiLevelB, MultiLevelB>();
                cfg.CreateMap<MultiLevelC, MultiLevelC>();
            });
        }

        private static Byte[] RandomByteArray()
        {
            byte[] results = new byte[_rnd.Next(1, 21)];
            _rnd.NextBytes(results);

            return results;
        }
        public static OneLevel BuildOneLevel(string id)
        {
            return new OneLevel()
            {
                BooleanField = _rnd.Next(0, 2) == 1,
                ByteArrayField = RandomByteArray(),
                ByteField = (byte)_rnd.Next(1, Byte.MaxValue),
                ChangeType = ChangeType.None,
                CharField = (char)_rnd.Next(1, char.MaxValue),
                DateTimeField = new DateTime(_rnd.Next(1, int.MaxValue)),
                DecimalField = (decimal)_rnd.NextDouble(),
                DoubleField = _rnd.NextDouble(),
                Id = id,
                Int32Field = _rnd.Next(1, 1000),
                Int64Field = _rnd.Next(1, 11100),
                SByteField = (sbyte)_rnd.Next(1, sbyte.MaxValue),
                SingleField = (Single)_rnd.NextDouble(),
                StringField = Guid.NewGuid().ToString(),
                TimeSpanField = new TimeSpan((long)_rnd.Next(1, int.MaxValue)),
                UInt32Field = (uint)_rnd.Next(1, 122),
                UInt64Field = (UInt64)_rnd.Next(1, int.MaxValue)
            };

        }



        public static MultiLevelB BuildMultiLevelB(string id)
        {
            return new MultiLevelB()
            {
                BooleanField = _rnd.Next(0, 2) == 1,
                ChangeType = ChangeType.None,
                Id = id,
                Int32Field = _rnd.Next(1, 1000),
                StringField = Guid.NewGuid().ToString()
            };
        }
        public static MultiLevelC BuildMultiLevelC(string id)
        {
            return new MultiLevelC()
            {
                ChangeType = ChangeType.None,
                Id = id,
                StringField = Guid.NewGuid().ToString()
            };
        }
        public static MultiLevelA BuildMultiLevelA(string id)
        {
            return new MultiLevelA()
            {
                BooleanField = _rnd.Next(0, 2) == 1,
                ByteArrayField = RandomByteArray(),
                ByteField = (byte)_rnd.Next(1, Byte.MaxValue),
                ChangeType = ChangeType.None,
                CharField = (char)_rnd.Next(1, char.MaxValue),
                DateTimeField = new DateTime(_rnd.Next(1, int.MaxValue)),
                DecimalField = (decimal)_rnd.NextDouble(),
                DoubleField = _rnd.NextDouble(),
                Id = id,
                Int32Field = _rnd.Next(1, 1000),
                Int64Field = _rnd.Next(1, 11100),
                SByteField = (sbyte)_rnd.Next(1, sbyte.MaxValue),
                SingleField = (Single)_rnd.NextDouble(),
                StringField = Guid.NewGuid().ToString(),
                TimeSpanField = new TimeSpan((long)_rnd.Next(1, int.MaxValue)),
                UInt32Field = (uint)_rnd.Next(1, 122),
                UInt64Field = (UInt64)_rnd.Next(1, int.MaxValue)
            };

        }
    }
}
