namespace ReportPortal.Customization.Clean
{
    using ReportPortal.Client;
    using ReportPortal.Client.Filtering;
    using ReportPortal.Client.Models;
    using ReportPortal.Customization.Extension;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    public class LaunchCleaner
    {
        private readonly CleanOptions _options;

        private readonly Service _service;

        public LaunchCleaner(Service service, CleanOptions options)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<Launch> CleanAsync(Launch launch)
        {
            if (launch is null)
            {
                throw new ArgumentNullException(nameof(launch));
            }

            var container = await _service
                .GetTestItemsAsync(GetFilter(launch.Id))
                .ConfigureAwait(false);

            var toDelete = GetTestsMarkedForDeletion(container.TestItems);

            toDelete
                .ForEach(async id => await _service.DeleteTestItemAsync(id)
                .ConfigureAwait(false));

            return launch;
        }

        private List<string> GetTestsMarkedForDeletion(List<TestItem> items)
        {
            var marked = new Dictionary<string, bool>();
            var predicate = CompilePredicate();

            foreach (var item in items.OrderBy(t => t.Type))
            {
                var descendants = items.Where(d => d.PathNames.ContainsKey(item.Id));

                bool toDelete = item.IsSuite()
                        ? descendants.All(predicate)
                        : predicate(item);

                if (toDelete)
                {
                    if (item.IsSuite() && !marked.ContainsKey(item.Id))
                    {
                        marked[item.Id] = true;

                        foreach (var descendant in descendants)
                        {
                            marked[descendant.Id] = false;
                        }
                    }
                    else
                    {
                        if (!marked.ContainsKey(item.Id))
                        {
                            marked[item.Id] = true;
                        }
                    }
                }
            }

            return marked.Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private Func<TestItem, bool> CompilePredicate()
        {
            var parameter = Expression.Parameter(typeof(TestItem), "item");
            Expression final = default(Expression);

            var initial = Call(nameof(ReportPortalExtension.IsFailed), parameter);

            if (_options.RemoveSkipped)
            {
                final = Expression.Or(left: initial,
                   right: Call(nameof(ReportPortalExtension.IsSkipped), parameter));
            }
            if (_options.RemoveInterrupted)
            {
                final = Expression.Or(left: final,
                   right: Call(nameof(ReportPortalExtension.IsInterrupted), parameter));
            }

            final = Expression.Or(left: final,
                right: Call(nameof(ReportPortalExtension.IsNotTest), parameter));

            return Expression.Lambda<Func<TestItem, bool>>(final, parameter).Compile();
        }

        private MethodCallExpression Call(string name, ParameterExpression parameter)
        {
            var methodInfo = typeof(ReportPortalExtension)
                .GetMethod(name, BindingFlags.Static | BindingFlags.Public);

            return Expression.Call(null, methodInfo, parameter);
        }

        private static FilterOption GetFilter(string launchId)
        {
            return new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Equals, "launch", launchId)
                },
                Paging = new Paging(1, int.MaxValue),
            };
        }
    }
}
