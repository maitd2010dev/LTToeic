using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Infrastructure.Context;
using CoreLTToeic.Infrastructure.Pattern;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Repositories
{
    public class QuestionGroupRepository : Repository<QuestionGroup>, IQuestionGroupRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public QuestionGroupRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<QuestionGroup>> GetByTestIdAsync(long testId)
        {
            using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.QuestionGroups
                .Include(g => g.Part)
                .Include(g => g.Images)
                .Include(g => g.Questions)
                .Where(g => g.TestId == testId)
                .OrderBy(g => g.Id)
                .ToListAsync();
        }
    }
}
