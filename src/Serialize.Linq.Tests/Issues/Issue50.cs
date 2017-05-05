using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serialize.Linq.Extensions;

namespace Serialize.Linq.Tests.Issues
{
    /// <summary>
    /// https://github.com/esskar/Serialize.Linq/issues/50
    /// </summary>
    [TestClass]
    public class Issue50
    {
        [TestMethod]
        public void SerializeArrayAsJson()
        {
            var list = new [] { "one", "two" };
            Expression<Func<Test, bool>> expression = test => list.Contains(test.Code);
            
            var value = expression.ToJson();

            Assert.IsNotNull(value);
        }
        
        [TestMethod]
        public void SerializeListAsJson()
        {
            var list = new List<string> { "one", "two" };
            Expression<Func<Test, bool>> expression = test => list.Contains(test.Code);

            var value = expression.ToJson();

            Assert.IsNotNull(value);
        }
        

        [TestMethod]
        public void SerializeDeserializeArrayAsJson()
        {
            var list = new[] { "one", "two" };
            Expression<Func<Test, bool>> expression = test => list.Contains(test.Code);

            var value = expression.ToJson();

            var actualExpression = (Expression<Func<Test, bool>>)value.ToExpression();
            var func = actualExpression.Compile();


            Assert.IsTrue(func(new Test { Code = "one" }), "one failed.");
            Assert.IsTrue(func(new Test { Code = "two" }), "two failed.");
            Assert.IsFalse(func(new Test { Code = "three" }), "three failed.");
        }

        [TestMethod]
        public void SerializeDeserializeArrayAsBinary()
        {
            var list = new[] { "one", "two" };
            Expression<Func<Test, bool>> expression = test => list.Contains(test.Code);

            var value = expression.ToJson();

            var actualExpression = (Expression<Func<Test, bool>>)value.ToExpression();
            var func = actualExpression.Compile();

            Assert.IsTrue(func(new Test { Code = "one" }), "one failed.");
            Assert.IsTrue(func(new Test { Code = "two" }), "two failed.");
            Assert.IsFalse(func(new Test { Code = "three" }), "three failed.");
        }

        public class Test
        {
            public string Code { get; set; }
        }
    }
}
