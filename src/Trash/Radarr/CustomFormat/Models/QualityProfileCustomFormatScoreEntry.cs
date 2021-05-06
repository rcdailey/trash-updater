namespace Trash.Radarr.CustomFormat.Models
{
    public class QualityProfileCustomFormatScoreEntry
    {
        public QualityProfileCustomFormatScoreEntry(ProcessedCustomFormatData customFormat, int score)
        {
            CustomFormat = customFormat;
            Score = score;
        }

        public ProcessedCustomFormatData CustomFormat { get; }
        public int Score { get; }
    }
}
