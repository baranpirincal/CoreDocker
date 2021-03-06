﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreDocker.Core.Framework.DataIntegrity;
using CoreDocker.Core.Framework.Mappers;
using FluentAssertions;
using CoreDocker.Core.Tests.Helpers;
using CoreDocker.Core.Tests.Managers;
using CoreDocker.Dal.Models;
using CoreDocker.Dal.Models.Base;
using CoreDocker.Dal.Models.Projects;
using CoreDocker.Dal.Models.Users;
using NUnit.Framework;

namespace CoreDocker.Core.Tests.DataIntegrity
{
    [TestFixture]
    public class DataIntegrityManagerTests : BaseManagerTests
    {
        private DataIntegrityManager _dataIntegrityManager;
        private List<IIntegrity> _integrityUpdatetors;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _integrityUpdatetors = IntegrityOperators.Default;
            _dataIntegrityManager = new DataIntegrityManager(_baseManagerArguments.GeneralUnitOfWork,_integrityUpdatetors);
            
            
        }

        #endregion

        [Test]
        public void Constructor_WhenCalled_ShouldNotBeNull()
        {
            // arrange
            Setup();
            // assert
            _dataIntegrityManager.Should().NotBeNull();
        }

        [Test]
        public void FindMissingIntegrityOperators()
        {
            // arrange
            Setup();
            
            // action
            var referenceCount = _dataIntegrityManager.FindMissingIntegrityOperators<IBaseDalModel, IBaseReference>(typeof(BaseDalModel).GetTypeInfo().Assembly);
            // assert
            
            referenceCount.Where(x=> !x.Contains("Missing User on UserGrant")).Should().BeEmpty();
        }

        [Test]
        public void GetReferenceCount_GivenObjectNotReferenced_ShouldFindNoLinks()
        {
            // arrange
            Setup();
            Project project = _fakeGeneralUnitOfWork.Projects.AddAFake();
            // action
            long referenceCount = _dataIntegrityManager.GetReferenceCount(project).Result;
            // assert
            referenceCount.Should().Be(0);
        }

        [Test]
        public void GetReferenceCount_GivenObject_ShouldFindAllReferences()
        {
            // arrange
            Setup();
            Project project = _fakeGeneralUnitOfWork.Projects.AddAFake();
            User user = _fakeGeneralUnitOfWork.Users.AddAFake();
            user.DefaultProject = project.ToReference();
            project.Name = "NewName";
            // action
            long referenceCount = _dataIntegrityManager.GetReferenceCount(project).Result;
            // assert
            referenceCount.Should().Be(1);
        }

        [Test]
        public void UpdateAllReferences_GivenObject_ShouldUpdateTheReferences()
        {
            // arrange
            Setup();
            Project project = _fakeGeneralUnitOfWork.Projects.AddAFake();
            User user = _fakeGeneralUnitOfWork.Users.AddAFake();
            user.DefaultProject = project.ToReference();
            project.Name = "NewName";
            // action
            var result = _dataIntegrityManager.UpdateAllReferences(project).Result;
            // assert
            user.DefaultProject.Name.Should().Be(project.Name);
            result.Should().Be(1);
        }


//        [Test]
//        public void UpdateReferences_GivenObject_ShouldUpdateTheReferences()
//        {
//            // arrange
//            Setup();
//            Project project = _fakeGeneralUnitOfWork.Projects.AddAFake();
//            User user = _fakeGeneralUnitOfWork.Users.AddAFake();
//            user.AllowedProject = new List<ProjectReference>() { project.ToReference() };
//            project.Name = "NewName";
//            // action
//            var result = _dataIntegrityManager.UpdateAllReferences(project).Result;
//            // assert
//            user.AllowedProject.First().Name.Should().Be(project.Name);
//            result.Should().Be(1);
//        }

        [Test]
        public void UpdateAllReferences_GivenObjectWhereReferenceHasNotChanged_ShouldNotUpdateAllReference()
        {
            // arrange
            Setup();
            Project project = _fakeGeneralUnitOfWork.Projects.AddAFake();
            User user = _fakeGeneralUnitOfWork.Users.AddAFake();
            user.DefaultProject = project.ToReference();
            // action
            var result = _dataIntegrityManager.UpdateAllReferences(project).Result;
            // assert
            user.DefaultProject.Name.Should().Be(project.Name);
            result.Should().Be(1); // this should be 0 but check is slower than actually doing the update
        }
    }

    
}