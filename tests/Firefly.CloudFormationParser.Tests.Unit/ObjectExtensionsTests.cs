using System;

namespace Firefly.CloudFormationParser.Tests.Unit
{
    using Firefly.CloudFormationParser.Utils;
    using FluentAssertions;
    using System.Collections;
    using System.Collections.Generic;

    using Xunit;

    public class ObjectExtensionsTests
    {
        [Fact]
        public void ShouldConvertSimpleAnonymousTypeToDictionary()
        {
            var anon = (object)new { Prop1 = "value1", Prop2 = "value2" };

            var dict = anon.ToResourceSchema();

            dict.Should().BeAssignableTo<IDictionary>();
        }

        [Fact]
        public void ShouldConvertComplexAnonymousTypeToDictionary()
        {
            var anon = (object)new { Prop1 = "value1", Prop2 = new { Prop3 = "value3" } };

            var dict = anon.ToResourceSchema();

            dict.Should().BeAssignableTo<IDictionary>();

            ((IDictionary)dict!)["Prop2"].Should().BeAssignableTo<IDictionary>();
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(long.MaxValue)]
        [InlineData(double.MaxValue)]
        public void ValueTypeShouldBeReturnedUnchanged(ValueType value)
        {
            var result = value.ToResourceSchema();

            result!.Equals(value).Should().BeTrue();
        }

        [Fact]
        public void StringShouldBeReturnedUnchanged()
        {
            const string Test = "test";

            Test.ToResourceSchema().Should().Be(Test);
        }

        [Fact]
        public void ShouldConverListContainingObjectsAndScalars()
        {
            var obj = new List<object> { 1, "hi", new { Prop1 = "value1", Prop2 = "value2" }, };

            var result = obj.ToResourceSchema();

            result.Should().BeAssignableTo<IList>();
            ((IList)result)[2].Should().BeAssignableTo<IDictionary>();
        }
    }
}
