using System;
using System.Collections.Generic;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// Factory + Strategy: holds every available IFunction implementation keyed by
    /// name (case-insensitively, since formulas are usually typed as =sum(...) or
    /// =SUM(...) interchangeably), and hands the right one to FunctionCallNode at
    /// evaluation time. Adding a new built-in function later means writing one new
    /// IFunction class and registering it here -- nothing else in the module changes.
    /// </summary>
    public sealed class FunctionRegistry
    {
        private readonly Dictionary<string, IFunction> _functions = new(StringComparer.OrdinalIgnoreCase);

        public void Register(IFunction function) => _functions[function.Name] = function;

        /// <summary>Looks up a function by name; returns null (not an exception) if it isn't registered, so FunctionCallNode can turn that into a #NAME? error value.</summary>
        public IFunction? Resolve(string name) => _functions.TryGetValue(name, out var function) ? function : null;

        /// <summary>Builds a registry with all eight required functions already registered: SUM, AVERAGE, MIN, MAX, COUNT, IF, ROUND, LOOKUP.</summary>
        public static FunctionRegistry CreateDefault()
        {
            var registry = new FunctionRegistry();
            registry.Register(new SumFunction());
            registry.Register(new AverageFunction());
            registry.Register(new MinFunction());
            registry.Register(new MaxFunction());
            registry.Register(new CountFunction());
            registry.Register(new IfFunction());
            registry.Register(new RoundFunction());
            registry.Register(new LookupFunction());
            return registry;
        }
    }
}
