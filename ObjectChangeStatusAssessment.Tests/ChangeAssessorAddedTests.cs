using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ObjectChangeStatusAssessment.Tests.Mocking;
using System.Collections.Generic;
using ObjectChangeStatusAssessment.Tests.Models;
using AutoMapper;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    [TestClass]
    public class ChangeAssessorAddedTests
    {
        [TestMethod]
        public void ChangeAssessorTests_List_RootValuesAdded_RootSetAsAdded()
        {

            var source = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };
            //Note out of order to ensure test covers other scenarios
            var destination = new List<OneLevel>() { source[2] };
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);


            var expectedIndices = new[] { 0, 1, 3 };


            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(source[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Added, source[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }
        }
        [TestMethod]
        public void ChangeAssessorTests_SubPropertyAdded_SubPropertySetAsAdded()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Note source will now have a sub single property
            //This is new and not in the destiation
            source.B = MockDataHelper.BuildMultiLevelB("23145");


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "There should be no entity changes only relationships");

            Assert.AreEqual(assessment.Relationships.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.B, assessment.Relationships[0].Value, "Source record returned as in relationship changes");
            Assert.AreEqual(assessment.Relationships[0].Parent, source, "Proper parent should be assigned in the relationship");
            Assert.AreEqual(assessment.Relationships[0].ChangeType, ChangeType.Added, "Change Type is set to Added");
            Assert.AreEqual(source.B.ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership");
        }
        [TestMethod]
        public void ChangeAssessorTests_SubPropertyAdded_IsOwner_SubPropertySetAsAdded()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Note source will now have a sub single property
            //This is new and not in the destiation
            source.B = MockDataHelper.BuildMultiLevelB("23145");


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                    //Note owner addition
                    .AddOwnerMapping<MultiLevelA>(a => a.B)
                    ;

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(assessment.OwnedEntities.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.B, assessment.OwnedEntities[0], "Source record returned as Added");
            Assert.AreEqual(source.B.ChangeType, ChangeType.Added, "Change Type is set to Added");
           
            //relationships
            Assert.AreEqual(assessment.Relationships.Count, 1, string.Format("One and only one change is expected, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(source.B, assessment.Relationships[0].Value, "Source record returned as in relationship changes");
            Assert.AreEqual(assessment.Relationships[0].Parent, source, "Proper parent should be assigned in the relationship");
            Assert.AreEqual(assessment.Relationships[0].ChangeType, ChangeType.Added, "Change Type is set to Added");

        }
        [TestMethod]
        public void ChangeAssessorTests_ListDestinationNull_RootValuesAdded_RootSetAsAdded()
        {
            var source = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212"), MockDataHelper.BuildOneLevel("561223") };

            //note null
            List<OneLevel> destination = null;
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);


            var expectedIndices = Enumerable.Range(0, source.Count);


            Assert.AreEqual(expectedIndices.Count(), assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(source[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Added, source[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }
        }

        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuessAdded_SubListPropertySetAsAdded()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("00"),
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var destination = Mapper.Map<MultiLevelA>(source);
            //Note only one exists therefore 2 should be added
            destination.Cs = new List<MultiLevelC>() { Mapper.Map<MultiLevelC>(source.Cs[1]) };

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            var expectedIndices = new[] { 0, 2 };
            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "There should be no entity changes only relationships");

            Assert.AreEqual(expectedIndices.Length, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == source.Cs[expectedIndex]);

                Assert.IsNotNull(relationship, string.Format("Record {0} should be in the list of changed relationships", expectedIndex));
                Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship " + expectedIndex);
                Assert.AreEqual(relationship.ChangeType, ChangeType.Added, "Change Type is set to Added " + expectedIndex);
                Assert.AreEqual(source.Cs[expectedIndex].ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership " + expectedIndex);

            }

        }

        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuessAdded_IsOwner_SubListPropertySetAsAdded()
        {
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.Cs = new System.Collections.Generic.List<MultiLevelC>()
            {
                 MockDataHelper.BuildMultiLevelC("00"),
                 MockDataHelper.BuildMultiLevelC("11"),
                 MockDataHelper.BuildMultiLevelC("22")
            };
            var destination = Mapper.Map<MultiLevelA>(source);
            //Note only one exists therefore 2 should be added
            destination.Cs = new List<MultiLevelC>() { Mapper.Map<MultiLevelC>(source.Cs[1]) };

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                    //Note ownership
                    .AddOwnerMapping<MultiLevelA>(a => a.Cs);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            var expectedIndices = new[] { 0, 2 };

            Assert.AreEqual(expectedIndices.Length, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                Assert.IsTrue(assessment.OwnedEntities.Contains(source.Cs[expectedIndex]), string.Format("Record {0} should be in the list of changes", expectedIndex));
                Assert.AreEqual(ChangeType.Added, source.Cs[expectedIndex].ChangeType, string.Format("Record {0} should have the added change type", expectedIndex));
            }

            //Relationships
            Assert.AreEqual(expectedIndices.Length, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            foreach (var expectedIndex in expectedIndices)
            {
                var relationship = assessment.Relationships.FirstOrDefault(r => r.Value == source.Cs[expectedIndex]);

                Assert.IsNotNull(relationship, string.Format("Record {0} should be in the list of changed relationships", expectedIndex));
                Assert.AreEqual(relationship.Parent, source, "Proper parent should be assigned in the relationship " + expectedIndex);
                Assert.AreEqual(relationship.ChangeType, ChangeType.Added, "Change Type is set to Added " + expectedIndex);
            }

        }

    }
}
