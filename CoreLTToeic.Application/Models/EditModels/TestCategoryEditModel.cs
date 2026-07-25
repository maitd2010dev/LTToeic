using System.ComponentModel.DataAnnotations;

namespace CoreLTToeic.Application.Models.EditModels
{
    public class TestCategoryEditModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;
    }
}
