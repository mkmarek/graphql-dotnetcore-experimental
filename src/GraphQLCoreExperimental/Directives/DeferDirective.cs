namespace GraphQLCoreExperimental.Directives
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;

    public class DeferDirective : GraphQLDirectiveType
    {
        public DeferDirective() 
            : base("defer", "Defers an execution of selected field and returns it as part of patch", DirectiveLocation.FIELD)
        {
        }

        public override bool PreExecutionIncludeFieldIntoResult(GraphQLDirective directive, ISchemaRepository schemaRepository)
        {
            return false;
        }

        public override bool PostExecutionIncludeFieldIntoResult(GraphQLDirective directive, ISchemaRepository schemaRepository, object value, object parentValue)
        {
            return false;
        }

        public override LambdaExpression GetResolver(Func<Task<object>> valueGetter, object parentValue)
        {
            Expression<Func<object>> resolver = () => null;

            return resolver;
        }
    }
}
