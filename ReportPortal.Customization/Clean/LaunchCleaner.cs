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

    public class LaunchCleaner : ILaunchCleaner
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

            var testItems = await GetTestItems(launch.Id);
            var toDelete = GetTestsMarkedForDeletion(testItems);

            toDelete
                .ForEach(async id => await _service.DeleteTestItemAsync(id)
                .ConfigureAwait(false));

            return launch;
        }

        private async Task<List<TestItem>> GetTestItems(string launchId)
        {
            var items = new List<TestItem>();
            var current = new List<TestItem>();

            int page = 1;

            do
            {
                var container = await _service
                    .GetTestItemsAsync(GetFilter(page++, launchId))
                    .ConfigureAwait(false);

                current = container.TestItems;
                items.AddRange(current);
            }
            while (current.Count > 0);

            return items;
        }

        private List<string> GetTestsMarkedForDeletion(List<TestItem> items)
        {
            var marked = new Dictionary<string, TestItemType>();
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
                        marked[item.Id] = item.Type;

                        foreach (var descendant in descendants)
                        {
                            marked[descendant.Id] = item.Type;
                        }
                    }
                    else
                    {
                        if (!marked.ContainsKey(item.Id))
                        {
                            marked[item.Id] = item.Type;
                        }
                    }
                }
            }

            return marked.OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private Func<TestItem, bool> CompilePredicate()
        {
            var parameter = Expression.Parameter(typeof(TestItem), "item");

            Expression initial = Call(nameof(ReportPortalExtension.IsFailed), parameter);

            if (_options.RemoveSkipped)
            {
                initial = Expression.Or(left: initial,
                   right: Call(nameof(ReportPortalExtension.IsSkipped), parameter));
            }
            if (_options.RemoveInterrupted)
            {
                initial = Expression.Or(left: initial,
                   right: Call(nameof(ReportPortalExtension.IsInterrupted), parameter));
            }

            initial = Expression.Or(left: initial,
                right: Call(nameof(ReportPortalExtension.IsNotTest), parameter));

            return Expression.Lambda<Func<TestItem, bool>>(initial, parameter).Compile();
        }

        private MethodCallExpression Call(string name, ParameterExpression parameter)
        {
            var methodInfo = typeof(ReportPortalExtension)
                .GetMethod(name, BindingFlags.Static | BindingFlags.Public);

            return Expression.Call(null, methodInfo, parameter);
        }

        private static FilterOption GetFilter(int page, string launchId)
        {
            return new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Equals, "launch", launchId)
                },
                Paging = new Paging(page, int.MaxValue),
            };
        }
    }
}
