using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ObjectChangeStatusAssessment.Tests.Models;
using ObjectChangeStatusAssessment.Tests.Mocking;
using System.Linq;
using AutoMapper;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    [TestClass]
    public class ChangeAssessorMixedStatusTests
    {
        [TestMethod]
        public void ChangeAssessorTests_List_RootValuesAddDeletedAndUpdated_RootSetAsMixedStatuses()
        {
            var source = new List<OneLevel>() { MockDataHelper.BuildOneLevel("-2124"), MockDataHelper.BuildOneLevel("1"), MockDataHelper.BuildOneLevel("5212") };
            //Note out of order to ensure test covers other scenarios
            var destination = new List<OneLevel>() {
                //Existing, unchanged
                Mapper.Map<OneLevel>(source[0]),
                //To Be updated
                MockDataHelper.BuildOneLevel("1"),

                //Note index 2 is not here and therefore added
                
                //Does not exist in source and therefore will be deleted
                 MockDataHelper.BuildOneLevel("14wdwedqweqe")
            };
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatusList(source, destination);


            Assert.AreEqual(3, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));


            Assert.AreEqual(ChangeType.None, source[0].ChangeType, string.Format("Record {0} should have the none change type", 0));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source[1]), string.Format("Record {0} should be in the list of changes", 1));
            Assert.AreEqual(ChangeType.Updated, source[1].ChangeType, string.Format("Record {0} should have the updated change type", 1));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source[2]), string.Format("Record {0} should be in the list of changes", 2));
            Assert.AreEqual(ChangeType.Added, source[2].ChangeType, string.Format("Record {0} should have the added change type", 2));

            Assert.IsTrue(assessment.OwnedEntities.Contains(destination[2]), string.Format("Record {0} should be in the list of changes", 2));
            Assert.AreEqual(ChangeType.Deleted, destination[2].ChangeType, string.Format("Record {0} should have the removed change type", 2));

        }

        [TestMethod]
        public void ChangeAssessorTests_List_SubPropertyValuesAddDeletedAndUpdatedAndRootUpdated_SubPropertySetAsMixedStatuses()
        {

            var source = MockDataHelper.BuildMultiLevelA("123");
            var destination = Mapper.Map<MultiLevelA>(source);
            source.Cs = new List<MultiLevelC>() { MockDataHelper.BuildMultiLevelC("-2124"), MockDataHelper.BuildMultiLevelC("1"), MockDataHelper.BuildMultiLevelC("5212") };
            destination.Cs = new List<MultiLevelC>() {
                //Existing, unchanged
                Mapper.Map<MultiLevelC>(source.Cs[0]),
                //To Be updated
                MockDataHelper.BuildMultiLevelC("1"),
                //Note index 2 is not here and therefore added
                
                //Does not exist in source and therefore will be deleted
                 MockDataHelper.BuildMultiLevelC("14wdwedqweqe")
            };

            //The root is changed as well
            destination.StringField = Guid.NewGuid().ToString();


            //Note out of order to ensure test covers other scenarios
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(1, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));
            Assert.AreEqual(2, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));
           

            Assert.IsTrue(assessment.OwnedEntities.Contains(source), "Root record should be in the list of changes");
            Assert.AreEqual(ChangeType.Updated, source.ChangeType, string.Format("Record {0} should have the update change type", 2));

            Assert.AreEqual(ChangeType.None, source.Cs[0].ChangeType, string.Format("Record {0} should have the none change type", 0));

            Assert.AreEqual(ChangeType.None, source.Cs[1].ChangeType, string.Format("Record {0} should have the none change type as it is not owned", 1));


            var relationshipS2 = assessment.Relationships.FirstOrDefault(r => r.Value == source.Cs[2]);

            Assert.IsNotNull(relationshipS2, "Record 2 should be in the list of changed relationships S2");
            Assert.AreEqual(relationshipS2.Parent, source, "Proper parent should be assigned in the relationship S2" );
            Assert.AreEqual(relationshipS2.ChangeType, ChangeType.Added, "Change Type is set to Added S2" );
            Assert.AreEqual(source.Cs[2].ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership S2");


            var relationshipD2 = assessment.Relationships.FirstOrDefault(r => r.Value == destination.Cs[2]);

            Assert.IsNotNull(relationshipD2, "Record 2 should be in the list of changed relationships D2");
            Assert.AreEqual(relationshipD2.Parent, source, "Proper parent should be assigned in the relationship D2");
            Assert.AreEqual(relationshipD2.ChangeType, ChangeType.Deleted, "Change Type is set to Deleted D2");
            Assert.AreEqual(destination.Cs[2].ChangeType, ChangeType.None, "Actual Entity type not changed due to lack of ownership D2");
          
        }



        [TestMethod]
        public void ChangeAssessorTests_List_SubPropertyValuesAddDeletedAndUpdatedAndRootUpdated_IsOwner_SubPropertySetAsMixedStatuses()
        {

            var source = MockDataHelper.BuildMultiLevelA("123");
            var destination = Mapper.Map<MultiLevelA>(source);
            source.Cs = new List<MultiLevelC>() { MockDataHelper.BuildMultiLevelC("-2124"), MockDataHelper.BuildMultiLevelC("1"), MockDataHelper.BuildMultiLevelC("5212") };
            destination.Cs = new List<MultiLevelC>() {
                //Existing, unchanged
                Mapper.Map<MultiLevelC>(source.Cs[0]),
                //To Be updated
                MockDataHelper.BuildMultiLevelC("1"),
                //Note index 2 is not here and therefore added
                
                //Does not exist in source and therefore will be deleted
                 MockDataHelper.BuildMultiLevelC("14wdwedqweqe")
            };

            //The root is changed as well
            destination.StringField = Guid.NewGuid().ToString();


            //Note out of order to ensure test covers other scenarios
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                        //Adding Ownershio
                        .AddOwnerMapping<MultiLevelA>(c => c.Cs);

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(4, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source), "Root record should be in the list of changes");
            Assert.AreEqual(ChangeType.Updated, source.ChangeType, string.Format("Record {0} should have the update change type", 2));

            Assert.AreEqual(ChangeType.None, source.Cs[0].ChangeType, string.Format("Record {0} should have the none change type", 0));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source.Cs[1]), string.Format("Record {0} should be in the list of changes", 1));
            Assert.AreEqual(ChangeType.Updated, source.Cs[1].ChangeType, string.Format("Record {0} should have the updated change type", 1));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source.Cs[2]), string.Format("Record {0} should be in the list of changes", 2));
            Assert.AreEqual(ChangeType.Added, source.Cs[2].ChangeType, string.Format("Record {0} should have the added change type", 2));

            Assert.IsTrue(assessment.OwnedEntities.Contains(destination.Cs[2]), string.Format("Record {0} should be in the list of changes", 2));
            Assert.AreEqual(ChangeType.Deleted, destination.Cs[2].ChangeType, string.Format("Record {0} should have the removed change type", 2));

        }


        

        [TestMethod]
        public void ChangeAssessorTests_SubListPropertyValuesAddDeletedAndUpdated_IsOwner_SubListPropertySetAsMixedStatuses()
        {
            var source = MockDataHelper.BuildMultiLevelA("123");
            var destination = Mapper.Map<MultiLevelA>(source);
            source.B = MockDataHelper.BuildMultiLevelB("5523");
            destination.B = MockDataHelper.BuildMultiLevelB("5523");
            //The root is changed as well
            destination.StringField = Guid.NewGuid().ToString();


            //Note out of order to ensure test covers other scenarios
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                    //Note ownership
                    .AddOwnerMapping<MultiLevelA>(a => a.B);

            var assessment = changeAssessor.SetChangeStatus(source, destination);


            Assert.AreEqual(2, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source), "Root record should be in the list of changes");
            Assert.AreEqual(ChangeType.Updated, source.ChangeType, string.Format("Record {0} should have the update change type", 2));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source.B), "Sub record should be in the list of changes");
            Assert.AreEqual(ChangeType.Updated, source.B.ChangeType, string.Format("Record {0} should have the update change type", 2));

        }
        [TestMethod]
        public void ChangeAssessorTests_CirclerReferenceSubListProperty_NoStackOverflow()
        {

            var source = MockDataHelper.BuildMultiLevelA("5213123");
            source.Cs = new List<MultiLevelC>()
            {
                MockDataHelper.BuildMultiLevelC("1"),
                MockDataHelper.BuildMultiLevelC("2"),
                MockDataHelper.BuildMultiLevelC("3"),
                MockDataHelper.BuildMultiLevelC("6")
            };
            foreach (var sourceC in source.Cs) { sourceC.A = source; };
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.Cs = new List<MultiLevelC>()
            {
                MockDataHelper.BuildMultiLevelC("1"),
                MockDataHelper.BuildMultiLevelC("2"),
                MockDataHelper.BuildMultiLevelC("3"),
                MockDataHelper.BuildMultiLevelC("4")
            };
            foreach (var destinationC in destination.Cs) { destinationC.A = destination; };

            //Note change results dont matter for this test, just ensure the results return. Amount checked just for
            //Some kind of assertion

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Circular ownership
                .AddOwnerMapping<MultiLevelA>(c => c.Cs)
                .AddOwnerMapping<MultiLevelC>(c => c.A);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            //3 updates, 1 add, 1 remove
            Assert.AreEqual(5, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

        }

        [TestMethod]
        public void ChangeAssessorTests_CirclerReferenceSubProperty_NoStackOverflow()
        {

            var source = MockDataHelper.BuildMultiLevelA("5213123");
            source.B = MockDataHelper.BuildMultiLevelB("1");
            source.B.A = source;
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.B = MockDataHelper.BuildMultiLevelB("1");
            destination.B.A = destination;

            //Note change results dont matter for this test, just ensure the results return. Amount checked just for
            //Some kind of assertion

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Circular ownership
                .AddOwnerMapping<MultiLevelA>(c => c.B)
                .AddOwnerMapping<MultiLevelB>(c => c.A);

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            //1 updates
            Assert.AreEqual(1, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

        }


        [TestMethod]
        public void ChangeAssessorTests_NullAndNullPassed_NoChangesReturned()
        {
            ChangeAssessor<int> changeAssessor = new ChangeAssessor<int>();

            var assessment = changeAssessor.SetChangeStatus((IChangeTrackable<int>)null, (IChangeTrackable<int>)null);

            Assert.IsFalse(assessment.OwnedEntities.Any(), "There should be no assessments");
        }
        [TestMethod]
        public void ChangeAssessorTests_List_NullAndNullPassed_NoChangesReturned()
        {
            ChangeAssessor<int> changeAssessor = new ChangeAssessor<int>();

            var assessment = changeAssessor.SetChangeStatusList((List<IChangeTrackable<int>>)null, (List<IChangeTrackable<int>>)null);

            Assert.IsFalse(assessment.OwnedEntities.Any(), "There should be no assessments");
        }

        [TestMethod]
        public void ChangeAssessorTests_SubPropertyValuesIdDifferent_IsOwner_SubPropertySetWithAnAddAndARemove()
        {
            var source = MockDataHelper.BuildMultiLevelA("123");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Different ids
            source.B = MockDataHelper.BuildMultiLevelB("1");
            destination.B = Mapper.Map<MultiLevelB>(source.B);
            destination.B.Id = "SOmething different";

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note Ownership
                .AddOwnerMapping<MultiLevelA>(c => c.B)
                ;

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(2, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            Assert.IsTrue(assessment.OwnedEntities.Contains(destination.B), "Root record should be in the list of changes");
            Assert.AreEqual(ChangeType.Deleted, destination.B.ChangeType, string.Format("Record {0} should have the update change type", 2));

            Assert.IsTrue(assessment.OwnedEntities.Contains(source.B), "Sub record should be in the list of changes");
            Assert.AreEqual(ChangeType.Added, source.B.ChangeType, string.Format("Record {0} should have the update change type", 2));

            Assert.AreEqual(2, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            var relationshipDeleted = assessment.Relationships.FirstOrDefault(f => f.Value == destination.B);

            Assert.IsNotNull(relationshipDeleted, "Destination should be in the list of changed relationships");
            Assert.AreEqual(relationshipDeleted.Parent, source, "Proper parent should be assigned in the relationship ");
            Assert.AreEqual(relationshipDeleted.ChangeType, ChangeType.Deleted, "Change Type is set to Added ");


            var relationshipAdded = assessment.Relationships.FirstOrDefault(f => f.Value == source.B);

            Assert.IsNotNull(relationshipAdded, "Destination should be in the list of changed relationships");
            Assert.AreEqual(relationshipAdded.Parent, source, "Proper parent should be assigned in the relationship ");
            Assert.AreEqual(relationshipAdded.ChangeType, ChangeType.Added, "Change Type is set to Added ");


        }


        [TestMethod]
        public void ChangeAssessorTests_SubPropertyValuesIdDifferent_SubPropertySetWithAnAddAndARemove()
        {
            var source = MockDataHelper.BuildMultiLevelA("123");
            var destination = Mapper.Map<MultiLevelA>(source);

            //Different ids
            source.B = MockDataHelper.BuildMultiLevelB("1");
            destination.B = Mapper.Map<MultiLevelB>(source.B);
            destination.B.Id = "SOmething different";

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(0, assessment.OwnedEntities.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            Assert.AreEqual(2, assessment.Relationships.Count, string.Format("Invalid change count, received {0}", assessment.OwnedEntities.Count));

            var relationshipDeleted = assessment.Relationships.FirstOrDefault(f => f.Value == destination.B);

            Assert.IsNotNull(relationshipDeleted, "Destination should be in the list of changed relationships");
            Assert.AreEqual(relationshipDeleted.Parent, source, "Proper parent should be assigned in the relationship ");
            Assert.AreEqual(relationshipDeleted.ChangeType, ChangeType.Deleted, "Change Type is set to Added ");


            var relationshipAdded = assessment.Relationships.FirstOrDefault(f => f.Value == source.B);

            Assert.IsNotNull(relationshipAdded, "Destination should be in the list of changed relationships");
            Assert.AreEqual(relationshipAdded.Parent, source, "Proper parent should be assigned in the relationship ");
            Assert.AreEqual(relationshipAdded.ChangeType, ChangeType.Added, "Change Type is set to Added ");


        }

    }
}
