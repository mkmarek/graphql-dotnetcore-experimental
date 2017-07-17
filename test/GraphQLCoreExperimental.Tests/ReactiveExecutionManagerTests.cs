namespace GraphQLCoreExperimental.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using Directives;
    using GraphQLCore.Type;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Type;

    [TestFixture]
    public class ReactiveExecutionManagerTests
    {
        private GraphQLObservableSchema schema;

        [Test]
        public void SimpleDefer_ReturnsTwoResults()
        {
            var result = this.Observe(@"
            {
                nested {
                    foo
                    foo2 @defer
                }
            }");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("{\"data\":{\"nested\":{\"foo\":\"bar\"}}}", result[0]);
            Assert.AreEqual("{\"path\":[\"nested\"],\"data\":{\"foo2\":\"bar2\"}}", result[1]);
        }

        [Test]
        public void NestedDefer_ReturnsResultForEveryDeferredField()
        {
            var result = this.Observe(@"
            {
                nested {
                    foo
                    foo2 @defer
                    nested @defer {
                        foo @defer
                        foo2
                    }
                }
            }");
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("{\"data\":{\"nested\":{\"foo\":\"bar\"}}}", result[0]);
            Assert.AreEqual("{\"path\":[\"nested\"],\"data\":{\"foo2\":\"bar2\"}}", result[1]);
            Assert.AreEqual("{\"path\":[\"nested\"],\"data\":{\"nested\":{\"foo2\":\"bar2\"}}}", result[2]);
            Assert.AreEqual("{\"path\":[\"nested\",\"nested\"],\"data\":{\"foo\":\"bar\"}}", result[3]);
        }

        private List<string> Observe(string query)
        {
            var isFinished = false;
            var queryLog = new List<string>();

            var observable = this.schema.Observe(query, null, null);

            observable.Subscribe(
                result => queryLog.Add(JsonConvert.SerializeObject(result)),
                ex => isFinished = true,
                () =>
                {
                    isFinished = true;
                });
            while (!isFinished)
            {
            }

            return queryLog;
        }

        [SetUp]
        public void SetUp()
        {
            var query = new QueryType();
            this.schema = new GraphQLObservableSchema();
            this.schema.AddOrReplaceDirective(new DeferDirective());
            this.schema.AddKnownType(query);
            this.schema.AddKnownType(new NestedType());
            this.schema.Query(query);
        }

        private class QueryType : GraphQLObjectType
        {
            public QueryType() : base("Query", null)
            {
                this.Field("nested", () => new Nested());
            }
        }

        private class Nested { }

        private class NestedType : GraphQLObjectType<Nested>
        {
            public NestedType() : base("Nested", null)
            {
                this.Field("foo", e => "bar");
                this.Field("foo2", e => "bar2");
                this.Field("nested", e => new Nested());
            }
        }
    }
}
