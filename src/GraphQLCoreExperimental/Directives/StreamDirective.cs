using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCoreExperimental.Directives
{
    using System.Linq.Expressions;
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;

    public class StreamDirective : GraphQLDirectiveType
    {
        public StreamDirective()
            : base("stream", "", DirectiveLocation.FIELD)
        {

        }

        /*public override bool PostponeNodeResolve(FieldScope scope, IWithDirectives node, out IEnumerable<Task<dynamic>> postponedNodes)
        {

            postponedNodes = enu
            //postponedNodes = scope.GetCollectionStream((GraphQLFieldSelection)node);
            return true;
        }*/

        public override bool PreExecutionIncludeFieldIntoResult(GraphQLDirective directive, ISchemaRepository schemaRepository)
        {
            return true;
        }

        public override LambdaExpression GetResolver(Func<Task<object>> valueGetter, object parentValue)
        {
            Expression<Func<object>> resolver = () => Enumerable.Empty<object>();

            return resolver;
        }
    }
}
