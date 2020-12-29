using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stexchange.Controllers;
using System;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Stexchange.Controllers.Exceptions;

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
        [ExpectedException(typeof(ArgumentNullException),
            "user tuple was null and inappropriately allowed")]
        public void CreateSession_TupleNull()
        {
            StexChangeController.CreateSession(null);
        }
        [TestMethod]
        public void CreateSession_DuplicateId()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(89, "5736TG");
            long existingSession = StexChangeController.CreateSession(user);
            //Act
            long createdSession = StexChangeController.CreateSession(user);
            //Assert
            Assert.IsFalse(StexChangeController.SessionExists(existingSession));
            Assert.IsTrue(StexChangeController.GetSessionData(createdSession, out Tuple<int, string> sessionData));
            Assert.AreEqual(user, sessionData);
        }
        [TestMethod]
        public void CreateSession_Normal()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(145, "9973RB");
            //Act
            long createdSession = StexChangeController.CreateSession(user);
            //Assert
            Assert.IsTrue(StexChangeController.GetSessionData(createdSession, out Tuple<int, string> sessionData));
            Assert.AreEqual(user, sessionData);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException),
            "Tuple that contains a null value was inappropriately allowed")]
        public void CreateSession_PostalCodeNull()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(586, null);
            //Act
            StexChangeController.CreateSession(user);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
            "Postal code that was too short was inapporpriately allowed")]
        public void CreateSession_PostalCodeInvalid_TooShort()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(934, "NT");
            //Act
            StexChangeController.CreateSession(user);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
            "Postal code that was too short was inapporpriately allowed")]
        public void CreateSession_PostalCodeInvalid_TooLong()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(934, "12345AB");
            //Act
            StexChangeController.CreateSession(user);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
            "Postal code that was too short was inapporpriately allowed")]
        public void CreateSession_PostalCodeInvalid_Format()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(934, "123ABC");
            //Act
            StexChangeController.CreateSession(user);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException),
            "UserId that was invalid was inapporpriately allowed")]
        public void CreateSession_UserIdInvalid()
        {
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(-1, "7384UY");
            //Act
            StexChangeController.CreateSession(user);
        }
        public void GetUserId_Normal()
        {
            //Arrange
            int expected = 47;
            Tuple<int, string> user = new Tuple<int, string>(expected, "4329TU");
            var mockedController = MockController(StexChangeController.CreateSession(user).ToString(), true);
            //Act
            int actual = mockedController.GetUserId();
            //Assert
            Assert.AreEqual(expected, actual);
        }
        public void GetUserId_Mismatch()
        {
            string message = "Session token should be invalid, but was found to be valid";
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(34, "7684IO");
            var mockedController = MockController("12345", true);
            //Act
            try
            {
                int actual = mockedController.GetUserId();
                Assert.Fail(message);
            } catch (InvalidSessionException except)
            {
                if (except.Message != "Session does not exist")
                {
                    Assert.Fail(message);
                }
            }
        }
        public void GetUserId_NoCookie()
        {
            string message = "Session token should be invalid, but was found to be valid";
            //Arrange
            Tuple<int, string> user = new Tuple<int, string>(908, "3342PE");
            var mockedController = MockController(null, false);
            //Act
            try
            {
                mockedController.GetUserId();
                Assert.Fail(message);
            } catch (InvalidSessionException except)
            {
                if(except.Message != "Cookie does not exist")
                {
                    Assert.Fail(message);
                }
            }
        }

        private StexChangeController MockController(string value, bool createCookie)
        {
            var responseMock = new Mock<HttpResponse>().Object;
            if (createCookie)
            {
                responseMock.Cookies.Append(StexChangeController.Cookies.SessionToken, value);
            }
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Response).Returns(responseMock);
            return new HomeController()
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = contextMock.Object
                }
            };
        }
    }
}
