namespace SolarEnergy.ViewModels
{
    public class CompanyEvaluationsViewModel
    {
        public EvaluationStatsViewModel Stats { get; set; } = new();
        public List<CompanyEvaluationViewModel> Reviews { get; set; } = new();
        public string CompanyName { get; set; } = string.Empty;
    }

    public class EvaluationStatsViewModel
    {
        public int TotalReviews { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
        public int CommentsOnly { get; set; }
        public int RecentReviews { get; set; }

        public double FiveStarsPercentage => TotalRatings > 0 ? (double)FiveStars / TotalRatings * 100 : 0;
        public double FourStarsPercentage => TotalRatings > 0 ? (double)FourStars / TotalRatings * 100 : 0;
        public double ThreeStarsPercentage => TotalRatings > 0 ? (double)ThreeStars / TotalRatings * 100 : 0;
        public double TwoStarsPercentage => TotalRatings > 0 ? (double)TwoStars / TotalRatings * 100 : 0;
        public double OneStarPercentage => TotalRatings > 0 ? (double)OneStar / TotalRatings * 100 : 0;
    }

    public class CompanyEvaluationViewModel
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerLocation { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Retorna se a avaliação foi editada
        /// </summary>
        public bool IsEdited => UpdatedAt > CreatedAt.AddMinutes(1);
        
        /// <summary>
        /// Retorna texto da data de forma amigável
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} dia(s) atrás";
                if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hora(s) atrás";
                if (timeSpan.TotalMinutes >= 1)
                    return $"{(int)timeSpan.TotalMinutes} minuto(s) atrás";
                return "Agora mesmo";
            }
        }
    }
}