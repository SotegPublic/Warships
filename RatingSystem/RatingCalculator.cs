public class RatingCalculator
{
    private RanksConfig _ranksConfig;

    private const float TOTAL_GAMES_MODIFIER = 0.01f;
    private const int MIN_GAMES_COUNT_FOR_CALC_RATING = 10;

    public const string NO_RATING_RANK = "оэур";

    public RatingCalculator(RanksConfig ranksConfig) 
    {
        _ranksConfig = ranksConfig;
    }

    public int GetNewRating(int wons, int totalGameCount)
    {
        if(totalGameCount > MIN_GAMES_COUNT_FOR_CALC_RATING)
        {
            var newRating = ((wons / totalGameCount) * 1000) + (totalGameCount * TOTAL_GAMES_MODIFIER);

            return (int)newRating;
        }
        else
        {
            return 0;
        }
    }

    public string GetRank(int rating)
    {
        return _ranksConfig.GetRankName(rating);
    }
}
