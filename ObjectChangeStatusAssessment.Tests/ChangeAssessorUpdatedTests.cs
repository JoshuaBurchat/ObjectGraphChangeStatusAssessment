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
    public class ChangeAssessorUpdatedTests
    {
        [TestMethod]
        public void ChangeAssessorTests_RootValuesChanged_RootSetAsUpdated()
        {
            //Note both should be randomly generated but have the same ID
            var source = MockDataHelper.BuildOneLevel("1");
            var destination = MockDataHelper.BuildOneLevel("1");
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities));
            Assert.AreEqual(source, assessment.OwnedEntities[0], "Source record returned as changed");
            Assert.AreEqual(source.ChangeType, ChangeType.Updated, "Change Type is set to updated");

        }

        [TestMethod]
        public void ChangeAssessorTests_SubPropertyValuesChanged_IsOwner_SubPropertySetAsUpdated()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.B = MockDataHelper.BuildMultiLevelB("2");
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.B = Mapper.Map<MultiLevelB>(source.B);

            source.B.StringField = "I am changed";

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note Ownership
                .AddOwnerMapping<MultiLevelA>(a => a.B);

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.B, assessment.OwnedEntities[0], "Source record returned as changed");
            Assert.AreEqual(source.B.ChangeType, ChangeType.Updated, "Change Type is set to updated");


        }
   
        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuesChanged_IsOwner_SubListPropertySetAsUpdated()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.Cs = source.Cs.Select(c => Mapper.Map<MultiLevelC>(c)).ToList();
            destination.Cs[0].StringField = "I am different1";
            destination.Cs[1].StringField = "I am different2";

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note ownership
                .AddOwnerMapping<MultiLevelA>(a => a.Cs);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 2, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.IsTrue(assessment.OwnedEntities.Contains(source.Cs[0]), "Cs 0 returned as changed");
            Assert.IsTrue(assessment.OwnedEntities.Contains(source.Cs[1]), "Cs 1 returned as changed");
            Assert.AreEqual(source.Cs[0].ChangeType, ChangeType.Updated, "Change Type is set to updated for C 0");
            Assert.AreEqual(source.Cs[1].ChangeType, ChangeType.Updated, "Change Type is set to updated for C 1");
        }
        [TestMethod]
        public void ChangeAssessorTests_List_RootValuesUpdatedAndOutOfOrder_RootSetAsAdded()
        {
            var source = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };
            //Note out of order to ensure test covers other scenarios
            var destination = new List<OneLevel>() { MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("561223"), MockDataHelper.BuildOneLevel("5212") };


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);


            var expectedIndices = new[] { 0, 1, 2, 3 };


            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(source[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Updated, source[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }
        }


    }
}
