using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stexchange.Controllers;
using System;

namespace Tests
{
    [TestClass]
    public class StexChangeControllerTest
    {
        [TestMethod]
        public void SessionExists_True()
        {
            //Arrange
            var testData = new Tuple<int, string>(29, "2846DF");
            long token = StexChangeController.CreateSession(testData);
            //Act
            bool outcome = StexChangeController.SessionExists(token);
            //Assert
            Assert.IsTrue(outcome);
            //Cleanup
            StexChangeController.TerminateSession(token);
        }
        [TestMethod]
        public void SessionExists_False()
        {
            Assert.IsFalse(StexChangeController.SessionExists(57));
        }
        [TestMethod]
        public void GetSessionData_True()
        {
            //Arrange
            var expected = new Tuple<int, string>(34, "7519YY");
            long token = StexChangeController.CreateSession(expected);
            //Act
            bool outcome = StexChangeController.GetSessionData(token, out Tuple<int, string> actual);
            //Assert
            Assert.IsTrue(outcome);
            Assert.AreEqual(expected, actual);
            //Cleanup
            StexChangeController.TerminateSession(token);
        }
        [TestMethod]
        public void GetSessionData_False()
        {
            //Act
            bool outcome = StexChangeController.GetSessionData(42, out Tuple<int, string> output);
            //Assert
            Assert.IsFalse(outcome);
            Assert.IsNull(output);
        }
        [TestMethod]
        public void TerminateSession_True()
        {
            //Arrange
            var testData = new Tuple<int, string>(17, "1145BV");
            long token = StexChangeController.CreateSession(testData);
            //Act
            bool outcome = StexChangeController.TerminateSession(token);
            bool checkRemoved = !StexChangeController.SessionExists(token);
            //Assert
            Assert.IsTrue(outcome);
            Assert.IsTrue(checkRemoved);
        }
        [TestMethod]
        public void TerminateSession_False()
        {
            Assert.IsFalse(StexChangeController.TerminateSession(98));
        }
        [TestMethod]
        public void CreateSession_TupleNull()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void CreateSession_DuplicateId()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void CreateSession_Normal()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void CreateSession_PostalCodeNull()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void CreateSession_PostalCodeInvalid()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void CreateSession_UserIdInvalid()
        {
            throw new NotImplementedException();
        }
    }
}
