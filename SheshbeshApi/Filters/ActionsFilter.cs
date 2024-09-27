using Microsoft.AspNetCore.Mvc.Filters;

namespace SheshbeshApi.Filters
{
    public class ActionsFilter: IAsyncActionFilter
    {
        private readonly ILogger<ActionsFilter> _logger;
        public ActionsFilter(ILogger<ActionsFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var startTime = DateTime.UtcNow;
            var actionName = context.ActionDescriptor.DisplayName;
            _logger.LogWarning($"Starting execution of action: {actionName} at {startTime}", actionName, startTime);

            var action = await next();

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            if (action.Exception == null)
            {
                _logger.LogWarning($"Completed execution of action: {actionName} at {endTime} (Duration: {duration}ms)", actionName, endTime, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogError(action.Exception.Message, $"Exception occurred while executing action: {actionName} at {endTime} (Duration: {duration}ms)", actionName, endTime, duration.TotalMilliseconds);
            }
        }
    }
}
