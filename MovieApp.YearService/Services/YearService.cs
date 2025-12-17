using Grpc.Core;

namespace YearService.Services
{
    public class YearCalculatorService : YearCalculator.YearCalculatorBase
    {
        private readonly ILogger<YearCalculatorService> _logger;

        public YearCalculatorService(ILogger<YearCalculatorService> logger)
        {
            _logger = logger;
        }

        public override Task<YearResponse> CalculateYearsAgo(YearRequest request, ServerCallContext context)
        {
            var currentYear = DateTime.Now.Year;
            var yearsAgo = currentYear - request.Year;

            _logger.LogInformation("gRPC: Year {Year} was {YearsAgo} years ago", request.Year, yearsAgo);

            var message = yearsAgo switch
            {
                0 => "This year!",
                1 => "Last year",
                < 0 => $"{Math.Abs(yearsAgo)} years in the future",
                _ => $"{yearsAgo} years ago"
            };

            return Task.FromResult(new YearResponse
            {
                YearsAgo = yearsAgo,
                Message = message
            });
        }
    }
}