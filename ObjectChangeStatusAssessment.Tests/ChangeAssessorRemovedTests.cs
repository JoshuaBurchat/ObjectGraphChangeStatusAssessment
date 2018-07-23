using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectChangeStatusAssessment.Tests.Mocking;
using AutoMapper;
using ObjectChangeStatusAssessment.Tests.Models;
using System.Collections.Generic;
using System.Linq;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    [TestClass]
    public class ChangeAssessorRemovedTests
    {
        [TestMethod]
        public void ChangeAssessorTests_List_ItemRemoved_RootSetAsRemoved()
        {
            var destination = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };

            var source = new List<OneLevel>() { Mapper.Map<OneLevel>(destination[2]) };


            //Note out of order to ensure test covers other scenarios
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);


            var expectedIndices = new[] { 0, 1, 3 };


            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(destination[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Deleted, destination[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }
        }

        [TestMethod]
        public void ChangeAssessorTests_ListSourceNull_RootValuesAdded_RootSetAsAdded()
        {
            var destination = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };

            List<OneLevel> source = null;

            //Note out of order to ensure test covers other scenarios
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);

            var expectedIndices = Enumerable.Range(0, destination.Count).ToArray();

            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(destination[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Deleted, destination[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }
        }
        [TestMethod]
        public void ChangeAssessorTests_SubPropertyRemoved_SubPropertySetAsRemoved()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Note source will now have a sub single property
            //This is removed and not in the source
            destination.B = MockDataHelper.BuildMultiLevelB("23145");

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.Relationships.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));

            var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == destination.B);

            Assert.IsNotNull(relationship, "Record {0} should be in the list of changed relationships");
            Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship");
            Assert.AreEqual(relationship.ChangeType, ChangeType.Deleted, "Change Type is set to Deleted");
            Assert.AreEqual(destination.B.ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership");

        }

        [TestMethod]
        public void ChangeAssessorTests_SubPropertyRemoved_IsOwner_SubPropertySetAsRemoved()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Note source will now have a sub single property
            //This is removed and not in the source
            destination.B = MockDataHelper.BuildMultiLevelB("23145");

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note ownership assignment
                .AddOwnerMapping<MultiLevelA>(a => a.B);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(destination.B, assessment.OwnedEntities[0], "Destination record returned as Deleted");
            Assert.AreEqual(ChangeType.Deleted, destination.B.ChangeType, "Change Type is set to Deleted");

            //Relationships
            Assert.AreEqual(assessment.Relationships.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));

            var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == destination.B);

            Assert.IsNotNull(relationship, "Record {0} should be in the list of changed relationships");
            Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship");
            Assert.AreEqual(relationship.ChangeType, ChangeType.Deleted, "Change Type is set to Deleted");


        }

        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuesRemoved_SubListPropertySetAsRemoved()
        {
            var destination = MockDataHelper.BuildMultiLevelA("1");
            destination.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("00"),
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var source = Mapper.Map<MultiLevelA>(destination);
            //Note only one exists therefore 2 should be added
            source.Cs = new List<MultiLevelC>() { Mapper.Map<MultiLevelC>(source.Cs[1]) };

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            var expectedIndices = new[] { 0, 2 };

            Assert.AreEqual(expectedIndices.Length, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));


            foreach (var expectedIndex in expectedIndices)
            {
                var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == destination.Cs[expectedIndex]);

                Assert.IsNotNull(relationship, string.Format("Record {0} should be in the list of changed relationships", expectedIndex));
                Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship " + expectedIndex);
                Assert.AreEqual(relationship.ChangeType, ChangeType.Deleted, "Change Type is set to Deleted " + expectedIndex);
                Assert.AreEqual(destination.Cs[expectedIndex].ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership " + expectedIndex);

            }

        }

        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuesRemoved_IsOwner_SubListPropertySetAsRemoved()
        {
            var destination = MockDataHelper.BuildMultiLevelA("1");
            destination.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("00"),
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var source = Mapper.Map<MultiLevelA>(destination);
            //Note only one exists therefore 2 should be added
            source.Cs = new List<MultiLevelC>() { Mapper.Map<MultiLevelC>(source.Cs[1]) };

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note Ownership
                .AddOwnerMapping<MultiLevelA>(a => a.Cs);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            var expectedIndices = new[] { 0, 2 };

            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(destination.Cs[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Deleted, destination.Cs[expectedIndex].ChangeType, string.Format("Record {0} should have the deleted change type", expectedIndex));
            }

            Assert.AreEqual(expectedIndices.Length, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            //Relationships
            foreach (var expectedIndex in expectedIndices)
            {
                var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == destination.Cs[expectedIndex]);

                Assert.IsNotNull(relationship, string.Format("Record {0} should be in the list of changed relationships", expectedIndex));
                Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship " + expectedIndex);
                Assert.AreEqual(relationship.ChangeType, ChangeType.Deleted, "Change Type is set to Deleted " + expectedIndex);
            }
        }

    }
}
