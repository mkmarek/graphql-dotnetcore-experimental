namespace GraphQLCoreExperimental.Execution
{
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class ReactiveExecutionManager : ExecutionManager
    {
        private ReactiveFieldCollector fieldCollector;

        public ReactiveExecutionManager(GraphQLSchema graphQLSchema, string expression, object variables = null,
            string clientId = null, int? subscriptionId = null) :
            base(graphQLSchema, expression, variables, clientId, subscriptionId)
        {
            
        }

        public IObservable<dynamic> Observe(string operationToExecute)
        {
            return this.Execute(operationToExecute).ToObservable(TaskPoolScheduler.Default);
        }

        public override async Task<ExpandoObject> ComposeResultForQuery(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition, object parent = null)
        {
            var context = this.CreateExecutionContext(operationDefinition);
            var scope = new FieldScope(context, type, parent);

            var fields = context.FieldCollector.CollectFields(type, operationDefinition.SelectionSet, scope);
            var resultObject = await scope.GetObject(fields);

            await this.AppendIntrospectionInfo(scope, fields, resultObject);

            var returnObject = new ExpandoObject();
            var returnObjectDictionary = (IDictionary<string, object>)returnObject;

            returnObjectDictionary.Add("data", resultObject);

            if (scope.Errors.Any())
                returnObjectDictionary.Add("errors", scope.Errors);

            return returnObject;
        }

        protected override ExecutionContext CreateExecutionContext(GraphQLOperationDefinition operationDefinition)
        {
            var variableResolver = this.CreateVariableResolver();

            this.fieldCollector = new ReactiveFieldCollector(
                this.Fragments,
                this.GraphQLSchema.SchemaRepository);

            return new ExecutionContext()
            {
                FieldCollector = this.fieldCollector,
                OperationType = operationDefinition.Operation,
                Schema = this.GraphQLSchema,
                SchemaRepository = this.GraphQLSchema.SchemaRepository,
                VariableResolver = variableResolver
            };
        }

        private IEnumerable<dynamic> Execute(string operationToExecute)
        {
            yield return this.ExecuteAsync(operationToExecute).Result;

            while (this.fieldCollector.DeferredFields.Any())
            {
                var tuple = this.fieldCollector.DeferredFields.Dequeue();
                var scope = tuple.Item1;
                var selection = tuple.Item2;
                var fields = new Dictionary<string, IList<GraphQLFieldSelection>>() { { selection.Name.Value, new[] { selection } } };

                yield return new
                {
                    path = scope.ReorderPath(scope.Path),
                    data = scope.GetObjectSynchronously(fields).Result
                };
            }
        }
    }
}
