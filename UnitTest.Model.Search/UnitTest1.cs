using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Search;

using Moq;
using Logging;

namespace UnitTest.Model.Search
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetNextMatch_ReturnsMatches_DueThreeCalls()
        {
            // https://www.infragistics.com/community/blogs/dhananjay_kumar/archive/2015/07/16/how-to-unit-test-private-methods-in-ms-test.aspx
            //
            var str =
@"qweq";

            var m = new Mock<ILogger>();
            m.Setup(z => z.Log("zzz")).Returns(true);

            Searcher s = new Searcher(str, "q", m.Object);

            var objPrivate = new PrivateObject(s);

            //
            var result = objPrivate.Invoke("GetNextMatch");
            //
            Assert.AreEqual("q\n", result);

            //
            result = objPrivate.Invoke("GetNextMatch");
            //
            Assert.AreEqual("q\n", result);

            //
            result = objPrivate.Invoke("GetNextMatch");
            //
            Assert.AreEqual(null, result);

            ////
            //result = objPrivate.Invoke("GetNextMatch");
            ////
            //Assert.AreEqual("q\n", result);
        }
    }
}
