using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ScoreConversionSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ScoreConversionSeeder(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SeedAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();

        if (!await ctx.ListeningScoreConversions.AnyAsync())
        {
            ctx.ListeningScoreConversions.AddRange(ListeningTable());
            await ctx.SaveChangesAsync();
        }

        if (!await ctx.ReadingScoreConversions.AnyAsync())
        {
            ctx.ReadingScoreConversions.AddRange(ReadingTable());
            await ctx.SaveChangesAsync();
        }

        // Keep existing databases in sync with the application's zero-score rule.
        var listeningZero = await ctx.ListeningScoreConversions
            .FirstOrDefaultAsync(x => x.Correct == 0);
        var readingZero = await ctx.ReadingScoreConversions
            .FirstOrDefaultAsync(x => x.Correct == 0);

        if (listeningZero != null)
            listeningZero.Score = 0;
        if (readingZero != null)
            readingZero.Score = 0;

        await ctx.SaveChangesAsync();
    }

    // Approximate TOEIC Listening conversion (0 correct = 0 in the app)
    private static IEnumerable<ListeningScoreConversion> ListeningTable()
    {
        int[] scores =
        [
              0,   5,   5,  10,  10,  15,  20,  25,  30,  35, // 0-9
             40,  45,  50,  55,  60,  65,  70,  75,  80,  85, // 10-19
             90,  95, 100, 105, 110, 115, 120, 125, 130, 135, // 20-29
            140, 145, 150, 155, 160, 165, 170, 175, 180, 185, // 30-39
            195, 200, 205, 210, 215, 220, 230, 235, 240, 245, // 40-49
            255, 260, 265, 270, 275, 280, 290, 295, 300, 310, // 50-59
            315, 320, 325, 330, 335, 340, 345, 350, 355, 365, // 60-69
            370, 375, 380, 385, 390, 400, 405, 410, 415, 425, // 70-79
            430, 435, 440, 445, 450, 455, 460, 465, 470, 475, // 80-89
            480, 485, 490, 490, 490, 495, 495, 495, 495, 495, // 90-99
            495                                                // 100
        ];

        return scores.Select((s, i) => new ListeningScoreConversion { Correct = i, Score = s });
    }

    // Approximate TOEIC Reading conversion (0 correct = 0 in the app)
    private static IEnumerable<ReadingScoreConversion> ReadingTable()
    {
        int[] scores =
        [
              0,   5,   5,   5,  10,  15,  20,  25,  30,  35, // 0-9
             40,  45,  50,  55,  60,  65,  70,  75,  80,  85, // 10-19
             90,  95, 100, 105, 110, 115, 120, 125, 135, 140, // 20-29
            145, 150, 155, 160, 165, 170, 175, 180, 185, 190, // 30-39
            195, 200, 205, 210, 220, 225, 230, 235, 240, 250, // 40-49
            255, 260, 265, 270, 275, 280, 285, 290, 295, 305, // 50-59
            310, 315, 325, 330, 335, 340, 345, 350, 355, 360, // 60-69
            370, 375, 380, 385, 390, 395, 400, 410, 415, 420, // 70-79
            430, 435, 440, 445, 450, 455, 460, 465, 470, 475, // 80-89
            480, 485, 490, 490, 495, 495, 495, 495, 495, 495, // 90-99
            495                                                // 100
        ];

        return scores.Select((s, i) => new ReadingScoreConversion { Correct = i, Score = s });
    }
}
