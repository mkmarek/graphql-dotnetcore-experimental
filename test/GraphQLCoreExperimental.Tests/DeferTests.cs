namespace GraphQLCoreExperimental.Tests
{   
    using Directives;
    using GraphQLCore.Type;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class DeferTests
    {
        private GraphQLSchema schema;

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
            Assert.AreEqual("{\"path\":[\"nested\",\"foo2\"],\"data\":\"bar2\"}", result[1]);
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

            Assert.AreEqual(new []
            {
                "{\"path\":[\"nested\",\"foo2\"],\"data\":\"bar2\"}",
                "{\"path\":[\"nested\",\"nested\"],\"data\":{\"foo2\":\"bar2\"}}",
                "{\"path\":[\"nested\",\"nested\",\"foo\"],\"data\":\"bar\"}"
            }.OrderBy(e => e), result.Skip(1).OrderBy(e => e));
        }

        [Test]
        public void DeferWithDelays_DoesntWaitForDelayedFields()
        {
            var result = this.Observe(@"
            {
                delay1000: delay(delay: 1000) @defer
                noDelay: delay(delay: 0) @defer
                delay50: delay(delay: 50) @defer
                delay600: delay(delay: 600) @defer
            }");

            Assert.AreEqual(5, result.Count);

            Assert.AreEqual("{\"data\":{}}", result[0]);
            Assert.AreEqual("{\"path\":[\"noDelay\"],\"data\":\"foo\"}", result[1]);
            Assert.AreEqual("{\"path\":[\"delay50\"],\"data\":\"foo\"}", result[2]);
            Assert.AreEqual("{\"path\":[\"delay600\"],\"data\":\"foo\"}", result[3]);
            Assert.AreEqual("{\"path\":[\"delay1000\"],\"data\":\"foo\"}", result[4]);
        }

        [Test]
        public void NestedArrayDefer()
        {
            var result = this.Observe(@"
            {
                array {
                    foo @defer
                    nestedArray @defer {
                        nested {
                            foo
                            foo2 @defer
                        }
                    }
                }
            }");

            Assert.AreEqual(16, result.Count);
        }

        private List<string> Observe(string query)
        {
            var observable = this.schema.Subscribe(query, null, null);
            return observable.ToArray().GetAwaiter().GetResult().Select(JsonConvert.SerializeObject).ToList();
        }

        [SetUp]
        public void SetUp()
        {
            var query = new QueryType();
            this.schema = new GraphQLSchema();
            this.schema.AddOrReplaceDirective(new DeferDirective());
            this.schema.AddOrReplaceDirective(new StreamDirective());
            this.schema.AddOrReplaceDirective(new LiveDirective());
            this.schema.AddKnownType(query);
            this.schema.AddKnownType(new NestedType());
            this.schema.Query(query);
        }

        private class QueryType : GraphQLObjectType
        {
            public static bool IsRunning = true;

            public QueryType() : base("Query", null)
            {
                this.Field("nested", () => new Nested());
                this.Field("array", () => new[]
                {
                    new Nested(),
                    new Nested(),
                    new Nested()
                });
                this.Field("delay", (int delay) => this.Delay(delay));
                this.Field("stringArray", () => this.GetStringEnumerable());
            }

            private async Task<string> Delay(int delay)
            {
                await Task.Delay(delay);
                return "foo";
            }

            private IEnumerable<string> GetStringEnumerable()
            {
                IsRunning = true;

                yield return "a";
                Task.Delay(1000).GetAwaiter().GetResult();
                yield return "b";
                Task.Delay(1000).GetAwaiter().GetResult();
                yield return "c";

                IsRunning = false;
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
                this.Field("nestedArray", () => new[]
                {
                    new Nested(),
                    new Nested(),
                    new Nested()
                });
            }
        }
    }
}
