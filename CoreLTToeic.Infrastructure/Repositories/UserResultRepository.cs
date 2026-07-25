using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Infrastructure.Context;
using CoreLTToeic.Infrastructure.Pattern;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Repositories
{
    public class UserResultRepository : Repository<UserResult>, IUserResultRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public UserResultRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
        {
            _factory = factory;
        }

        public async Task<List<UserResult>> GetByUserIdAsync(string userId)
        {
            using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.UserResults
                .Include(r => r.Test)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();
        }

        public async Task<UserResult?> GetByIdWithAnswersAsync(long id)
        {
            using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.UserResults
                .Include(r => r.Test)
                .Include(r => r.UserAnswers).ThenInclude(a => a.Question).ThenInclude(q => q!.Part)
                .Include(r => r.UserAnswers).ThenInclude(a => a.Question).ThenInclude(q => q!.QuestionGroup)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
