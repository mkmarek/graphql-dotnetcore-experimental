namespace GraphQLCoreExperimental.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Translation;

    public class ReactiveFieldCollector : FieldCollector
    {
        public Queue<Tuple<FieldScope, GraphQLFieldSelection>> DeferredFields { get; } = new Queue<Tuple<FieldScope, GraphQLFieldSelection>>();

        public ReactiveFieldCollector(
            Dictionary<string, GraphQLFragmentDefinition> fragments,
            ISchemaRepository schemaRepository)
            :base(fragments, schemaRepository)
        {
        }

        protected override void CollectField(GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields, FieldScope scope)
        {
            if (this.ShouldBeDeferred(selection))
            {
                selection.Directives = selection.Directives.Where(e => e.Name.Value != "defer");
               
                this.DeferredFields.Enqueue(new Tuple<FieldScope, GraphQLFieldSelection>(scope, selection));
            }
            else
                base.CollectField(selection, fields, scope);
        }

        private bool ShouldBeDeferred(GraphQLFieldSelection field)
        {
            return field.Directives.Any(e => e.Name.Value == "defer");
        }
    }
}
