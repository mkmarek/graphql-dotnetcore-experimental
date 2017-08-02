namespace GraphQLCoreExperimental.Directives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;

    public class LiveDirective : GraphQLDirectiveType
    {
        public LiveDirective()
            : base("live", null, DirectiveLocation.FIELD)
        {
        }

        public override bool PostponeNodeResolve(FieldScope scope, IWithDirectives node, out IEnumerable<Task<dynamic>> postponedNodes)
        {
            postponedNodes = new[] { scope.GetSingleField((GraphQLFieldSelection)node) };
            return true;
        }

        public override bool PreExecutionIncludeFieldIntoResult(GraphQLDirective directive, ISchemaRepository schemaRepository)
        {
            return true;
        }

        public override LambdaExpression GetResolver(Func<Task<object>> valueGetter, object parentValue)
        {
            Expression<Func<object>> resolver = () => null;

            return resolver;
        }
    }
}
