using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectChangeStatusAssessment.Tests.Mocking;
using ObjectChangeStatusAssessment.Tests.Models;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    [TestClass]
    public class ChangeAssessorNoChangeTests
    {
        [TestMethod]
        public void ChangeAssessorTests_RootValuesChanged_RootSetAsNoChange()
        {
            //Note both should be randomly generated but have the same ID
            var source = MockDataHelper.BuildOneLevel("1");
            var destination = Mapper.Map<OneLevel>(source);
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 0, string.Format("No changes expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.ChangeType, ChangeType.None, "Change Type is set to NoChange");

        }

        [TestMethod]
        public void ChangeAssessorTests_SubPropertyValuesChanged_SubPropertySetAsNoChange()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.B = MockDataHelper.BuildMultiLevelB("2");
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.B = Mapper.Map<MultiLevelB>(source.B);


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 0, string.Format("No changes expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.B.ChangeType, ChangeType.None, "Change Type is set to None");
        }
        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuesChanged_SubListPropertySetAsNoChange()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.Cs = source.Cs.Select(c => Mapper.Map<MultiLevelC>(c)).ToList();



            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 0, string.Format("No changes expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.Cs[0].ChangeType, ChangeType.None, "Change Type is set to None for C 0");
            Assert.AreEqual(source.Cs[1].ChangeType, ChangeType.None, "Change Type is set to None for C 1");
        }
        [TestMethod]
        public void ChangeAssessorTests_List_RootValuesNoChangeAndOutOfOrder_RootSetAsAdded()
        {
            var source = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };
            //Note out of order to ensure test covers other scenarios
            var destination = new List<OneLevel>() {
                Mapper.Map< OneLevel >(source[2]),
                Mapper.Map< OneLevel >(source[1]),
                Mapper.Map< OneLevel >(source[0]),
                Mapper.Map< OneLevel >(source[3])
            };


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);




            Assert.AreEqual(0, assessment.OwnedEntities.Count, string.Format("No changes expected, received {0}", assessment.OwnedEntities.Count));

            foreach (var sourceItem in source)
            {
                Assert.AreEqual(ChangeType.None, sourceItem.ChangeType, string.Format("Record {0} should have the none change type", sourceItem.Id));
            }
        }



    }
}
