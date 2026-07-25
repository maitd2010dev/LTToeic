using CoreLTToeic.Application.Interfaces.Pattern;
using CoreLTToeic.Domain.Entities;

namespace CoreLTToeic.Application.Interfaces.IRepository
{
    public interface ITestCategoryRepository : IRepository<TestCategory>
    {
        Task<List<TestCategory>> GetAllWithTestsAsync();
    }
}
