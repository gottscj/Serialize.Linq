﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serialize.Linq.Extensions;
using Serialize.Linq.Tests.Internals;

namespace Serialize.Linq.Tests.Issues
{
    /// <summary>
    /// https://github.com/esskar/Serialize.Linq/issues/69
    /// </summary>
    [TestClass]
    public class Issue69
    {
        [TestMethod]
        public void JsonSerialzeAndDeserialize1969Utc()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1969, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        [TestMethod]
        public void JsonSerialzeAndDeserialize1969Local()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1969, 1, 1, 0, 0, 0, DateTimeKind.Local));
        }

        [TestMethod]
        public void JsonSerialzeAndDeserialize1970Utc()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));            
        }

        [TestMethod]
        public void JsonSerialzeAndDeserialize1970Local()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local));
        }

        [TestMethod]
        public void JsonSerialzeAndDeserialize1971Utc()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        [TestMethod]
        public void JsonSerialzeAndDeserialize1971Local()
        {
            this.SerialzeAndDeserializeDateTimeJson(new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Local));
        }
        
        private void SerialzeAndDeserializeDateTimeJson(DateTime dt)
        {
            Expression<Func<DateTime>> actual = () => dt;
            actual = actual.Update(Expression.Constant(dt), new List<ParameterExpression>());

            var serialized = actual.ToJson();
            var expected = serialized.ToExpression();
            ExpressionAssert.AreEqual(expected, actual);
        }
    }
}