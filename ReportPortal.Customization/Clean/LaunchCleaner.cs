namespace ReportPortal.Customization.Clean
{
    using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        private readonly CleanOptions _options;

        private readonly Service _service;

        public LaunchCleaner(Service service, CleanOptions options, ILogger logger = null)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger;
        }

        public async Task<Launch> CleanAsync(Launch launch)
        {
            if (launch is null)
            {
                throw new ArgumentNullException(nameof(launch));
            }

            var testItems = await GetTestItems(launch.Id);
            var toDelete = GetTestsMarkedForDeletion(testItems);

            foreach (var testItem in toDelete)
            {
                try
                {
                    var message = await _service.DeleteTestItemAsync(testItem);
                    _logger?.LogDebug(message.Info);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex.Message);
                }
            }

            return launch;
        }

        private async Task<List<TestItem>> GetTestItems(string launchId)
        {
            var items = new List<TestItem>();
            int page = 1;

            TestItemsContainer container;
            do
            {
                container = await _service
                   .GetTestItemsAsync(GetFilter(page++, launchId))
                   .ConfigureAwait(false);

                items.AddRange(container.TestItems);
            }
            while (container.TestItems.Count > 0);

            return items;
        }

        private List<string> GetTestsMarkedForDeletion(List<TestItem> items)
        {
            var marked = new Dictionary<string, (TestItemType Type, bool Delete)>();
            var predicate = CompilePredicate();

            //ordering for the passage of first complex tests like test and suites, and then steps.
            foreach (var item in items.OrderBy(t => t.Type))
            {
                if (marked.ContainsKey(item.Id))
                {
                    continue;
                }

                var descendants = items.Where(d => d.PathNames.ContainsKey(item.Id));

                bool toDelete = item.IsSuite()
                        ? descendants.All(predicate)
                        : predicate(item);

                if (toDelete)
                {
                    // if suite, mark for deleting only suite(children removed automatically)
                    if (item.IsSuite())
                    {
                        marked[item.Id] = (item.Type, true);

                        foreach (var descendant in descendants)
                        {
                            marked[descendant.Id] = (item.Type, false);
                        }
                    }
                    else
                    {
                        marked[item.Id] = (item.Type, true);
                    }
                }
            }

            var returned = marked.OrderByDescending(kvp => kvp.Value)
                .Where(kvp => kvp.Value.Delete);

            _logger?.LogDebug($"Marked to deletion next items :\n{string.Join("\n", returned)}");
            return returned.Select(kvp => kvp.Key).ToList();
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
