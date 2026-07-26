# Hướng dẫn tạo nhiều đề TOEIC từ một đề mẫu

> Cập nhật: 2026-07-26  
> Áp dụng cho `ToeicTestVariantSeeder`, schema version `1`.

Quy trình này dùng khi cần tạo nhiều đề demo có cùng nội dung 200 câu với một
đề đã seed hoàn chỉnh. Seeder clone dữ liệu đề thi nhưng không clone
`UserResults` hoặc `UserAnswers`.

Không dùng variant seeder để nhập một bộ câu hỏi mới. Với dữ liệu mới, tiếp tục
dùng `ListeningTestSeeder` và `ReadingTestSeeder`.

## 1. File liên quan

| Mục đích | File |
|---|---|
| Danh sách đề năm 2025–2026 | `CoreLTToeic.UI/SeedData/toeic_variants_2025_2026.json` |
| Model JSON | `CoreLTToeic.Infrastructure/Data/Seeders/ToeicTestVariantJsonModels.cs` |
| Validate và clone dữ liệu | `CoreLTToeic.Infrastructure/Data/Seeders/ToeicTestVariantSeeder.cs` |
| Tự quét file seed | `CoreLTToeic.UI/Program.cs` |

Ứng dụng tự quét:

```text
CoreLTToeic.UI/SeedData/toeic_variants_*.json
```

Thứ tự chạy seeder là Listening, Reading, rồi Variant. Vì vậy đề nguồn đã đủ
200 câu trước khi được clone.

## 2. Cấu trúc JSON

```json
{
  "schemaVersion": 1,
  "sourceTestTitle": "TOEIC Listening Actual Test 02 (2023)",
  "source": {
    "listening": "CoreLTToeic.UI/SeedData/toeic_listening_actual_test_02.json",
    "reading": "CoreLTToeic.UI/SeedData/toeic_reading_actual_test_02_2023.json"
  },
  "variants": [
    {
      "title": "TOEIC Full Practice Test 01 (2026)",
      "category": "2026",
      "duration": 120,
      "status": "Active"
    }
  ]
}
```

`source` là metadata truy vết. `sourceTestTitle` phải trùng chính xác title đề
200 câu trong database.

## 3. Dữ liệu được clone

Mỗi variant có:

- metadata đề thi, thời gian, trạng thái và category riêng;
- 7 Parts;
- 200 Questions, giữ nguyên số thứ tự `1..200`;
- Question Groups và ảnh của group;
- nội dung, lựa chọn, đáp án và explanation;
- ảnh, audio, transcript và timestamp.

Không clone:

- `UserResults`;
- `UserAnswers`;
- ID database của đề, Part, group, question hoặc image.

## 4. Validation và an toàn dữ liệu

Trước khi tạo variant, đề nguồn phải có:

- đúng 200 câu duy nhất từ `1..200`;
- Part counts `6/25/39/30/30/16/54`;
- đáp án A–D cho mọi câu;
- audio cho cả bốn Listening Parts;
- transcript có thể resolve cho mọi câu Listening.

Seeder dùng `title` của variant làm khóa idempotency:

- title chưa có: tạo đề mới;
- title đã có đủ 200 câu, 7 Parts và đúng category: bỏ qua;
- title đã tồn tại nhưng thiếu dữ liệu hoặc sai category: dừng, không xóa hoặc
  ghi đè;
- dữ liệu mới được tạo trong transaction; validation sau seed lỗi thì rollback.

Không thay thế đề nguồn và không thay đổi các kết quả làm bài đã tồn tại.

## 5. Chạy seed

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj `
  -c Release -- --seed-only
```

Nếu đã build:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj `
  -c Release --no-build --no-launch-profile -- --seed-only
```

Có thể chạy lặp lại; các title đã hoàn chỉnh sẽ được bỏ qua.

## 6. Kiểm tra category và dữ liệu

```sql
SELECT c.Name AS Category, COUNT(t.Id) AS Tests
FROM TestCategories c
LEFT JOIN Tests t ON t.TestCategoryId = c.Id
WHERE c.Name IN ('2025', '2026')
GROUP BY c.Name
ORDER BY c.Name;
```

Với seed hiện tại, kết quả mong đợi là 10 đề cho năm 2025 và 10 đề cho năm
2026. Năm 2025 gồm một đề đã có sẵn và chín variant; năm 2026 gồm mười
variant.

```sql
SELECT
    c.Name AS Category,
    COUNT(DISTINCT t.Id) AS Tests,
    MIN(x.QuestionCount) AS MinQuestions,
    MAX(x.QuestionCount) AS MaxQuestions,
    MIN(x.PartCount) AS MinParts,
    MAX(x.PartCount) AS MaxParts
FROM Tests t
JOIN TestCategories c ON c.Id = t.TestCategoryId
CROSS APPLY (
    SELECT
        (SELECT COUNT(*) FROM Questions q WHERE q.TestId = t.Id)
            AS QuestionCount,
        (SELECT COUNT(*) FROM Parts p WHERE p.TestId = t.Id)
            AS PartCount
) x
WHERE t.Title LIKE 'TOEIC Full Practice Test %'
GROUP BY c.Name
ORDER BY c.Name;
```

Mọi variant phải có `QuestionCount = 200` và `PartCount = 7`.

## 7. Checklist

- [ ] Đề nguồn đã được seed đủ 200 câu.
- [ ] JSON dùng schema version 1 và đúng pattern `toeic_variants_*.json`.
- [ ] Title của từng variant là duy nhất.
- [ ] Category và thời gian làm bài được khai báo rõ.
- [ ] Giữ đường dẫn JSON nguồn trong object `source`.
- [ ] Build Release và chạy `--seed-only`.
- [ ] Kiểm tra số đề theo category.
- [ ] Kiểm tra mỗi variant đủ 200 câu và 7 Parts.
- [ ] Chạy seed lần hai để xác nhận không có bản ghi trùng.
