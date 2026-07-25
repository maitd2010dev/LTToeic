namespace CoreLTToeic.Application.Models.ViewModels
{
    public class TestCategoryViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public List<TestViewModel> Tests { get; set; } = [];
        public int TestCount => Tests.Count;
    }
}
