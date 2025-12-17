using Grpc.Core;
using MovieApp.Calculator;

namespace MovieApp.StatsService.Services
{
    public class CalculatorGrpcService : MovieCalculator.MovieCalculatorBase
    {
        private readonly ILogger<CalculatorGrpcService> _logger;

        public CalculatorGrpcService(ILogger<CalculatorGrpcService> logger)
        {
            _logger = logger;
        }

        // Calculate average rating
        public override Task<AverageResponse> CalculateAverageRating(
            RatingList request, 
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Calculating average for {Count} ratings", request.Ratings.Count);

            if (request.Ratings.Count == 0)
            {
                return Task.FromResult(new AverageResponse
                {
                    Average = 0,
                    Count = 0,
                    Highest = 0,
                    Lowest = 0
                });
            }

            var ratings = request.Ratings.ToList();
            var average = ratings.Average();
            var highest = ratings.Max();
            var lowest = ratings.Min();

            _logger.LogInformation("gRPC: Average={Avg}, Highest={High}, Lowest={Low}", 
                average, highest, lowest);

            return Task.FromResult(new AverageResponse
            {
                Average = Math.Round(average, 2),
                Count = ratings.Count,
                Highest = highest,
                Lowest = lowest
            });
        }

        // Calculate rating distribution
        public override Task<DistributionResponse> CalculateRatingDistribution(
            RatingList request, 
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Calculating distribution for {Count} ratings", 
                request.Ratings.Count);

            var excellent = 0;
            var good = 0;
            var average = 0;
            var poor = 0;

            foreach (var rating in request.Ratings)
            {
                if (rating >= 8.0) excellent++;
                else if (rating >= 6.0) good++;
                else if (rating >= 4.0) average++;
                else poor++;
            }

            _logger.LogInformation("gRPC: Distribution - Excellent={E}, Good={G}, Average={A}, Poor={P}",
                excellent, good, average, poor);

            return Task.FromResult(new DistributionResponse
            {
                Excellent = excellent,
                Good = good,
                Average = average,
                Poor = poor
            });
        }

        // Get rating tier
        public override Task<TierResponse> GetRatingTier(
            SingleRating request, 
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Getting tier for rating {Rating}", request.Rating);

            string tier, emoji, description;

            if (request.Rating >= 8.0)
            {
                tier = "Excellent";
                emoji = "🌟";
                description = "Outstanding movie! Highly recommended.";
            }
            else if (request.Rating >= 6.0)
            {
                tier = "Good";
                emoji = "⭐";
                description = "Good movie, worth watching.";
            }
            else if (request.Rating >= 4.0)
            {
                tier = "Average";
                emoji = "🙂";
                description = "Decent movie, might be worth a watch.";
            }
            else
            {
                tier = "Poor";
                emoji = "😐";
                description = "Below average, proceed with caution.";
            }

            _logger.LogInformation("gRPC: Tier={Tier}", tier);

            return Task.FromResult(new TierResponse
            {
                Tier = tier,
                Emoji = emoji,
                Description = description
            });
        }
    }
}