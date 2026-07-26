# Hướng dẫn nối dữ liệu TOEIC Reading vào đề thi bằng JSON

> Cập nhật: 2026-07-26  
> Áp dụng cho `ReadingTestSeeder`, JSON schema version `1`.

Tài liệu này mô tả cách lấy 100 câu Reading từ HTML, lưu thành JSON có thể tái
sử dụng và nối vào một đề Listening 100 câu đã có trong SQL Server. Kết quả là
một đề TOEIC đủ 200 câu mà không xóa đề hoặc `UserResults` cũ.

## 1. Các file liên quan

| Mục đích | File |
|---|---|
| JSON Reading Actual Test 02 năm 2023 | `CoreLTToeic.UI/SeedData/toeic_reading_actual_test_02_2023.json` |
| Model đọc JSON | `CoreLTToeic.Infrastructure/Data/Seeders/ReadingTestJsonModels.cs` |
| Validate và nối dữ liệu | `CoreLTToeic.Infrastructure/Data/Seeders/ReadingTestSeeder.cs` |
| Đăng ký dependency | `CoreLTToeic.Infrastructure/Helper/BuildServices.cs` |
| Tự quét JSON và chạy seeder | `CoreLTToeic.UI/Program.cs` |
| Tạo nhiều đề demo từ đề 200 câu | `document/TEST_VARIANT_SEEDING.md` |

Ứng dụng tự quét các file:

```text
CoreLTToeic.UI/SeedData/toeic_reading_*.json
```

Listening được seed trước, Reading được append sau. Vì vậy có thể chạy cùng
một lệnh trên database trống nếu đã có cả JSON Listening và Reading tương ứng.

## 2. Nguồn dữ liệu và quy tắc trích xuất

Nguồn của dữ liệu mẫu hiện tại:

```text
document/feed-html/Practice toeic reading test 2023 with answer - Actual test 02 - Practice Toeic Tests.html
```

Khi trích xuất từ một HTML khác:

- Lấy câu hỏi, bốn lựa chọn và Answer Key từ chính file nguồn.
- Giữ nguyên thứ tự câu `101..200`.
- Part 5 lưu dưới `questions`.
- Part 6 và Part 7 lưu đoạn văn chung dưới `groups[].content`; câu hỏi của đoạn
  văn đặt trong `groups[].questions`.
- Nếu đoạn văn có ảnh, lưu URL vào `groups[].images`.
- Ghi đường dẫn HTML vào `source.questions` để truy vết.
- Chỉ nhập dữ liệu mà dự án có quyền lưu và phân phối.

## 3. Cấu trúc và validation bắt buộc

| Part | Dải câu | Số câu | Cấu trúc |
|---|---:|---:|---|
| Part 5 | 101–130 | 30 | `questions` |
| Part 6 | 131–146 | 16 | 4 `groups` |
| Part 7 | 147–200 | 54 | `groups` |
| Tổng Reading | 101–200 | 100 | Số câu duy nhất, liên tục |

Seeder từ chối dữ liệu nếu:

- `schemaVersion` khác `1`;
- thiếu `targetTestTitle`;
- không đủ đúng 100 câu hoặc số câu không phủ kín `101..200`;
- số câu từng Part không phải `30/16/54`;
- `correctAnswer` không phải `A`, `B`, `C` hoặc `D`;
- câu hỏi thiếu nội dung hoặc thiếu một trong bốn lựa chọn;
- group Part 6/7 không có nội dung đoạn văn.

JSON tối thiểu:

```json
{
  "schemaVersion": 1,
  "targetTestTitle": "TOEIC Listening Actual Test 02 (2023)",
  "category": "2023",
  "duration": 120,
  "source": {
    "questions": "document/feed-html/reading-source.html"
  },
  "parts": [
    {
      "partNum": 5,
      "directions": "Directions for Part 5",
      "questions": [],
      "groups": []
    }
  ]
}
```

Ví dụ trên chỉ minh họa schema, chưa đủ dữ liệu để vượt qua validation.

## 4. Cách nối vào đề đã có

`targetTestTitle` là khóa tìm đề đích. `ReadingTestSeeder` áp dụng các nguyên
tắc sau:

- Không tìm thấy đề: ghi warning và không tạo một đề Reading rời.
- Đề chưa có đúng 100 câu Listening thuộc Part 1–4: dừng với lỗi.
- Đề chưa có Reading: thêm Parts 5–7 và 100 câu Reading.
- Đề đã có đủ 100 câu Reading: bỏ qua, nên chạy lại lệnh seed không tạo bản sao.
- Đề đã có Reading một phần: dừng với lỗi, không xóa hoặc thay dữ liệu.

Sau khi append thành công:

- `Tests.TotalQuestions = 200`;
- `Tests.Duration` tối thiểu bằng `duration` trong JSON;
- category được tạo nếu chưa có và gắn vào đề;
- đề gốc và toàn bộ `UserResults` được giữ nguyên.

Không đổi title của đề đích chỉ để tên hiển thị đẹp hơn: JSON Listening dùng
title đó để nhận diện dữ liệu đã seed. Nếu cần đổi tên, phải cập nhật đồng bộ
`title` của JSON Listening và `targetTestTitle` của JSON Reading.

## 5. Feed trực tiếp vào SQL Server

Database đích được lấy từ:

```text
CoreLTToeic.UI/appsettings.json
ConnectionStrings:DefaultConnection
```

Từ thư mục root repository:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj -c Release -- --seed-only
```

Nếu đã build Release:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj `
  -c Release --no-build --no-launch-profile -- --seed-only
```

Lệnh này tự seed Listening trước, sau đó quét `toeic_reading_*.json`, append
Reading và thoát mà không mở web server.

## 6. Truy vấn kiểm tra sau khi seed

Thay giá trị `@Title` nếu seed đề khác.

```sql
DECLARE @Title nvarchar(256) =
    N'TOEIC Listening Actual Test 02 (2023)';
DECLARE @TestId int =
    (SELECT TOP 1 Id FROM Tests WHERE Title = @Title);

SELECT
    t.Id,
    t.Title,
    t.TotalQuestions,
    t.Duration,
    c.Name AS Category,
    COUNT(DISTINCT q.Id) AS ActualQuestions,
    COUNT(DISTINCT ur.Id) AS UserResults
FROM Tests t
LEFT JOIN TestCategories c ON c.Id = t.TestCategoryId
LEFT JOIN Questions q ON q.TestId = t.Id
LEFT JOIN UserResults ur ON ur.TestId = t.Id
WHERE t.Id = @TestId
GROUP BY t.Id, t.Title, t.TotalQuestions, t.Duration, c.Name;
```

Kết quả mong đợi: `TotalQuestions = 200`, `ActualQuestions = 200`; số
`UserResults` không bị giảm.

```sql
SELECT p.PartNum, COUNT(q.Id) AS QuestionCount
FROM Parts p
LEFT JOIN Questions q ON q.PartId = p.Id
WHERE p.TestId = @TestId
GROUP BY p.PartNum
ORDER BY p.PartNum;
```

Kết quả mong đợi cho Parts 1–7:

```text
6, 25, 39, 30, 30, 16, 54
```

```sql
SELECT
    COUNT(*) AS ReadingQuestions,
    COUNT(DISTINCT q.OrderNumber) AS UniqueNumbers,
    MIN(q.OrderNumber) AS FirstNumber,
    MAX(q.OrderNumber) AS LastNumber,
    SUM(CASE WHEN q.CorrectAnswer NOT IN ('A','B','C','D')
        THEN 1 ELSE 0 END) AS InvalidAnswers,
    SUM(CASE WHEN NULLIF(LTRIM(RTRIM(q.Content)), '') IS NULL
        THEN 1 ELSE 0 END) AS MissingContent,
    SUM(CASE WHEN
        NULLIF(LTRIM(RTRIM(q.Answer1)), '') IS NULL OR
        NULLIF(LTRIM(RTRIM(q.Answer2)), '') IS NULL OR
        NULLIF(LTRIM(RTRIM(q.Answer3)), '') IS NULL OR
        NULLIF(LTRIM(RTRIM(q.Answer4)), '') IS NULL
        THEN 1 ELSE 0 END) AS MissingOptions
FROM Questions q
JOIN Parts p ON p.Id = q.PartId
WHERE q.TestId = @TestId
  AND p.PartNum BETWEEN 5 AND 7;
```

Kết quả mong đợi: `100 / 100 / 101 / 200 / 0 / 0 / 0`.

## 7. Checklist handoff

- [ ] Đọc tài liệu này trước khi tạo hoặc sửa seed Reading.
- [ ] Giữ file HTML nguồn trong `document/feed-html`.
- [ ] Tạo JSON đúng pattern `toeic_reading_*.json`.
- [ ] Đặt đúng `targetTestTitle` của đề Listening đã có.
- [ ] Validate đủ câu `101..200` và Part counts `30/16/54`.
- [ ] Không xóa đề có `UserResults`; không tự sửa dữ liệu Reading một phần.
- [ ] Build Release và chạy `--seed-only`.
- [ ] Chạy ba truy vấn kiểm tra ở trên.
- [ ] Mở giao diện làm bài và xem lại để kiểm tra Parts 5–7 hiển thị đúng.
