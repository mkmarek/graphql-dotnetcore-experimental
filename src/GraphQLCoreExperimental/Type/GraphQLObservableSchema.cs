namespace GraphQLCoreExperimental.Type
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using Execution;
    using GraphQLCore.Type;

    public class GraphQLObservableSchema : GraphQLSchema
    {
        private Queue<object> deferredFields;

        public IObservable<dynamic> Observe(string expression, dynamic variables, string operationToExecute)
        {
            using (var context = new ReactiveExecutionManager(this, expression, variables))
            {
                return context.Observe(operationToExecute);
            }
        }
    }
}
