using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectChangeStatusAssessment.Tests.Mocking;
using AutoMapper;
using ObjectChangeStatusAssessment.Tests.Models;

namespace ObjectChangeStatusAssessment.Tests.NonOwnerTests
{
    [TestClass]
    public class ChangeAssessorPropertyIgnoreTests
    {
        [TestMethod]
        public void ChangeAssessorTests_IgnoreInterfaceSet_ChangesAreNotDetectedForInterfaceFields()
        {
            var source = MockDataHelper.BuildOneLevel("1");
            var destination = Mapper.Map<OneLevel>(source);
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note ignoring the whole interface
                .IgnoreFields<InterfaceToIgnore>();

            //Only change fields from interface cast for test safety
            var sourceCastToInterface = (InterfaceToIgnore)source;
            sourceCastToInterface.BooleanField = !sourceCastToInterface.BooleanField;
            sourceCastToInterface.Int32Field += 100;


            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "No changes should be found as the changed fields should be ignored");
        }
        [TestMethod]
        public void ChangeAssessorTests_IgnoreBaseClassSet_ChangesAreNotDetectedForBaseClassFields()
        {
            var source = MockDataHelper.BuildMultiLevelC("1");
            var destination = Mapper.Map<MultiLevelC>(source);
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note ignoring the whole class
                .IgnoreFields<BaseClassToIgnore>()
               ;
            //Only change fields from ignored class cast for test safety
            var sourceCastToBaseClass = (BaseClassToIgnore)source;
            sourceCastToBaseClass.StringField = Guid.NewGuid().ToString();


            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "No changes should be found as the changed fields should be ignored");

        }

        [TestMethod]
        public void ChangeAssessorTests_IgnoreBaseClassAndInterfaceSet_ChangesAreNotDetectedForBaseClassAndInterfaceFields()
        {
            var source = MockDataHelper.BuildOneLevel("1");
            var destination = Mapper.Map<OneLevel>(source);
            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Note ignoring the whole interface and base class
                .IgnoreFields<InterfaceToIgnore>()
                .IgnoreFields<BaseClassToIgnore>();

            //Only change fields from interface cast for test safety
            var sourceCastToInterface = (InterfaceToIgnore)source;
            sourceCastToInterface.BooleanField = !sourceCastToInterface.BooleanField;
            sourceCastToInterface.Int32Field += 100;
            //Only change fields from ignored class cast for test safety
            var sourceCastToBaseClass = (BaseClassToIgnore)source;
            sourceCastToBaseClass.StringField = Guid.NewGuid().ToString();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "No changes should be found as the changed fields should be ignored");
        }

        [TestMethod]
        public void ChangeAssessorTests_IgnoreFieldNameSet_ChangesAreNotDetectedForFieldsSet()
        {
            //Note I am using sub properties to ensure the name is ignored in both levels of different types
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.B = MockDataHelper.BuildMultiLevelB("23145");
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.B = Mapper.Map<MultiLevelB>(source.B);


            //Change  StringField, and Int32
            source.StringField = Guid.NewGuid().ToString();
            source.Int32Field += 2000;
            source.B.StringField = Guid.NewGuid().ToString();
            source.B.Int32Field += 1000;

            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>()
                //Ignoring both fields
                .IgnoreFields("StringField", "Int32Field");

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "No changes should be found as the changed fields should be ignored");
        }

        [TestMethod]
        public void ChangeAssessorTests_ChangeTypeChanged_NoChangesShouldBePresentedAsThisFieldIsIgnored()
        {
            //Note I am using sub properties to ensure the name is ignored in both levels of different types
            var source = MockDataHelper.BuildMultiLevelA("1");
            source.B = MockDataHelper.BuildMultiLevelB("23145");
            var destination = Mapper.Map<MultiLevelA>(source);
            destination.B = Mapper.Map<MultiLevelB>(source.B);


            //Ensure statuses are different
            source.ChangeType = ChangeType.Updated;
            destination.ChangeType = ChangeType.Added;

            source.B.ChangeType = ChangeType.None;
            destination.B.ChangeType = ChangeType.Deleted;


            ChangeAssessor<string> changeAssessor = new ChangeAssessor<string>();

            var assessment = changeAssessor.SetChangeStatus(source, destination);

            Assert.AreEqual(assessment.OwnedEntities.Count, 0, "No changes should be found as the changed fields should be ignored");

        }
    }
}
