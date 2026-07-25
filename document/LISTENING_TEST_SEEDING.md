# Hướng dẫn feed đề TOEIC Listening từ JSON vào SQL Server

> Cập nhật: 2026-07-25  
> Áp dụng cho `ListeningTestSeeder`, JSON schema version `1`.

Tài liệu này là handoff cho các session sau. Quy trình chuẩn là:

1. Đọc dữ liệu câu hỏi/đáp án và transcript từ nguồn HTML.
2. Chuẩn hóa thành một file JSON.
3. Đặt JSON vào `CoreLTToeic.UI/SeedData`.
4. Chạy lệnh seed-only để ghi trực tiếp vào database.
5. Chạy các truy vấn kiểm tra ở cuối tài liệu.

## 1. Các file liên quan

| Mục đích | File |
|---|---|
| JSON mẫu hoàn chỉnh 100 câu | `CoreLTToeic.UI/SeedData/toeic_listening_actual_test_02.json` |
| Model đọc JSON | `CoreLTToeic.Infrastructure/Data/Seeders/ListeningTestJsonModels.cs` |
| Validate và ghi database | `CoreLTToeic.Infrastructure/Data/Seeders/ListeningTestSeeder.cs` |
| Đăng ký dependency | `CoreLTToeic.Infrastructure/Helper/BuildServices.cs` |
| Tự quét JSON và chạy seeder | `CoreLTToeic.UI/Program.cs` |
| Sửa điểm 0 và dữ liệu lịch sử | `document/sql/fix-zero-toeic-scores.sql` |

Ứng dụng tự quét tất cả file có tên:

```text
CoreLTToeic.UI/SeedData/toeic_listening_*.json
```

Vì vậy khi thêm đề mới, không cần sửa `Program.cs`.

## 2. Chuẩn bị dữ liệu nguồn

Với một đề Listening, nên lưu cả hai trang nguồn:

- Trang câu hỏi, ảnh, đáp án và audio.
- Trang transcript.

Ví dụ hiện tại:

```text
document/feed-html/Practice toeic listening test 2023 with answer - Actual test 02 - Practice Toeic Tests.html
document/feed-html/Transcript toeic listening test 02 - Practice Toeic Tests.html
```

Khi trích xuất:

- Giữ đúng thứ tự câu `1..100`.
- Giữ URL ảnh/audio đầy đủ nếu chưa tải media về storage riêng.
- Ghi đường dẫn nguồn vào object `source` để có thể kiểm tra lại.
- Chỉ sử dụng dữ liệu mà dự án có quyền lưu và phân phối.

## 3. Quy tắc JSON bắt buộc

Seeder hiện tại chỉ nhận một đề Listening đầy đủ:

| Part | Số câu | Kiểu dữ liệu |
|---|---:|---|
| Part 1 | 6 | `questions` |
| Part 2 | 25 | `questions` |
| Part 3 | 39 | `groups`, thường 3 câu/group |
| Part 4 | 30 | `groups`, thường 3 câu/group |
| Tổng | 100 | Order number duy nhất từ 1 đến 100 |

Các validation đang được áp dụng:

- `schemaVersion` phải bằng `1`.
- Tổng số câu phải bằng `100`.
- `orderNumber` phải duy nhất và phủ đủ `1..100`.
- `correctAnswer` chỉ nhận `A`, `B`, `C`, `D`.
- Số câu từng Part phải đúng `6 / 25 / 39 / 30`.

Tên file đề xuất:

```text
toeic_listening_<nguon>_<nam>_<ma-de>.json
```

Ví dụ:

```text
toeic_listening_actual_2023_test_02.json
```

### JSON tối thiểu

```json
{
  "schemaVersion": 1,
  "title": "TOEIC Listening Actual Test 03 (2024)",
  "category": "2024",
  "duration": 45,
  "status": "Active",
  "source": {
    "questions": "document/feed-html/questions.html",
    "transcript": "document/feed-html/transcript.html"
  },
  "parts": [
    {
      "partNum": 1,
      "directions": "Directions for Part 1",
      "questions": [],
      "groups": []
    }
  ]
}
```

Đoạn trên chỉ minh họa schema và chưa vượt qua validation 100 câu.

## 4. Cách điền từng Part

### Part 1

- Câu `1..6`.
- `content` thường là `null`.
- `answer1..answer4` có thể là chuỗi rỗng vì đáp án không được in khi làm bài.
- Mỗi câu có `image`.
- Audio chung của Part đặt ở câu đầu; các câu còn lại để `audio: null`.
- Transcript đặt tại từng câu trong field `transcript`.

### Part 2

- Câu `7..31`.
- Chỉ có đáp án `A`, `B`, `C`.
- Đặt `answer4: null` để UI không render lựa chọn D.
- Audio chung của Part đặt ở câu 7.
- Transcript đặt tại từng câu.

### Part 3 và Part 4

- Part 3: câu `32..70`.
- Part 4: câu `71..100`.
- Dùng `groups`.
- Transcript chung của đoạn hội thoại/bài nói đặt trong `group.content`.
- Audio chung của Part đặt tại group đầu tiên; các group sau có thể để `audio: null`.
- Ảnh chung của group đặt trong `group.images`.
- Các question bên trong group để `transcript: null`, tránh lặp transcript.

Ví dụ group:

```json
{
  "name": "Questions 32-34",
  "audio": "https://example.com/part-3.mp3",
  "content": "<p><strong>Transcript</strong><br>...</p>",
  "images": [],
  "questions": [
    {
      "orderNumber": 32,
      "content": "What is the conversation about?",
      "answer1": "Option A",
      "answer2": "Option B",
      "answer3": "Option C",
      "answer4": "Option D",
      "correctAnswer": "B",
      "image": null,
      "audio": null,
      "transcript": null
    }
  ]
}
```

## 5. Mapping JSON sang database

| JSON | Database |
|---|---|
| `title`, `duration`, `status` | `Tests` |
| `category` | `TestCategories`; tự tạo nếu chưa có |
| `parts[].partNum`, `directions` | `Parts` |
| `parts[].questions[]` | `Questions` |
| `parts[].groups[]` | `QuestionGroups` |
| `groups[].images[]` | `QuestionGroupImages` |
| `group.content` | Transcript chung Part 3/4 |
| `question.transcript` | Transcript từng câu Part 1/2 |

Field `source` chỉ là metadata trong JSON, không ghi vào database.

## 6. Feed trực tiếp vào database

Database đích lấy từ:

```text
CoreLTToeic.UI/appsettings.json
ConnectionStrings:DefaultConnection
```

Từ thư mục root repository, chạy:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj -c Release -- --seed-only
```

Lệnh này:

- Build ứng dụng Release.
- Chạy các seeder.
- Tự tìm mọi file `toeic_listening_*.json`.
- Ghi dữ liệu vào SQL Server.
- Thoát ngay sau khi seed, không mở web server.

Nếu đã build trước đó:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj -c Release --no-build -- --seed-only
```

Nếu schema database chưa cập nhật, chạy migration trước:

```powershell
dotnet ef database update `
  --project CoreLTToeic.Infrastructure `
  --startup-project CoreLTToeic.UI
```

Khởi động ứng dụng bình thường cũng tự chạy seeder:

```powershell
dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj
```

## 7. Idempotency và cập nhật đề đã tồn tại

Seeder dùng `title` làm khóa nhận diện.

- Chưa có title: tạo đề mới.
- Đã có title và đủ 100 câu: bỏ qua.
- Đã có title, thiếu câu và chưa có kết quả người dùng: seeder xóa bản lỗi rồi tạo lại.
- Đã có title, thiếu câu nhưng đã có kết quả người dùng: không xóa; ghi warning.

Nếu JSON của một đề đủ 100 câu đã thay đổi:

1. Cách an toàn nhất là đổi `title` để tạo một version mới.
2. Hoặc xóa đề cũ trong Admin nếu chưa có kết quả người dùng, rồi chạy lại seed.
3. Không xóa trực tiếp đề đã có `UserResults`; cần migration dữ liệu có chủ đích.

## 8. Kiểm tra sau khi seed

### Kiểm tra tổng câu và category

```sql
DECLARE @Title nvarchar(256) =
    N'TOEIC Listening Actual Test 02 (2023)';

SELECT
    t.Id,
    t.Title,
    c.Name AS Category,
    t.TotalQuestions,
    COUNT(q.Id) AS ActualQuestions
FROM Tests t
LEFT JOIN TestCategories c ON c.Id = t.TestCategoryId
LEFT JOIN Questions q ON q.TestId = t.Id
WHERE t.Title = @Title
GROUP BY t.Id, t.Title, c.Name, t.TotalQuestions;
```

Kết quả mong đợi: `TotalQuestions = 100`, `ActualQuestions = 100`.

### Kiểm tra số câu từng Part

```sql
DECLARE @Title nvarchar(256) =
    N'TOEIC Listening Actual Test 02 (2023)';

SELECT p.PartNum, COUNT(q.Id) AS QuestionCount
FROM Tests t
JOIN Parts p ON p.TestId = t.Id
LEFT JOIN Questions q ON q.PartId = p.Id
WHERE t.Title = @Title
GROUP BY p.PartNum
ORDER BY p.PartNum;
```

Kết quả mong đợi: `6, 25, 39, 30`.

### Kiểm tra audio, transcript và đáp án

```sql
DECLARE @Title nvarchar(256) =
    N'TOEIC Listening Actual Test 02 (2023)';

SELECT
    COUNT(DISTINCT COALESCE(q.Audio, qg.Audio)) AS AudioFiles,
    SUM(CASE
        WHEN NULLIF(q.Transcript, '') IS NOT NULL
          OR NULLIF(qg.Content, '') IS NOT NULL
        THEN 1 ELSE 0 END) AS QuestionsWithTranscript,
    SUM(CASE WHEN q.CorrectAnswer IN ('A','B','C','D')
        THEN 1 ELSE 0 END) AS QuestionsWithValidAnswer
FROM Tests t
JOIN Questions q ON q.TestId = t.Id
LEFT JOIN QuestionGroups qg ON qg.Id = q.QuestionGroupId
WHERE t.Title = @Title;
```

Với dữ liệu mẫu hiện tại:

- `AudioFiles = 4`.
- `QuestionsWithTranscript = 100`.
- `QuestionsWithValidAnswer = 100`.

Có thể chạy file SQL bằng:

```powershell
sqlcmd -S localhost -d CoreLTToeic -E -C -i "path\to\script.sql"
```

## 9. Quy tắc UI không được phá vỡ

- Khi đang làm bài, transcript Listening phải bị ẩn.
- Chỉ sau khi bài thi hoàn thành mới được xem transcript.
- Trang kết quả phải lấy audio theo thứ tự:
  - `Question.Audio`.
  - Nếu không có, fallback `QuestionGroup.Audio`.
- Audio review hiển thị một lần theo Part và sticky khi cuộn.
- Đề Listening-only dùng thang tối đa `495`, không phải `990`.
- Section không tồn tại hoặc có 0 câu đúng phải nhận `0` điểm.

Các file UI/nghiệp vụ liên quan:

```text
CoreLTToeic.UI/Components/Shared/TestTaking/TestQuestionGroupItem.razor
CoreLTToeic.UI/Components/Pages/Test/TestResult.razor
CoreLTToeic.Application/Mapping/MappingProfile.cs
CoreLTToeic.Application/Business/UserResultService.cs
```

## 10. Checklist handoff cho session sau

- [ ] Đọc tài liệu này trước khi sửa JSON/seeder.
- [ ] Giữ lại hai file nguồn questions và transcript.
- [ ] Tạo file đúng pattern `toeic_listening_*.json`.
- [ ] Không lặp transcript group vào từng question Part 3/4.
- [ ] Validate đủ 100 câu và đúng `6/25/39/30`.
- [ ] Chạy `--seed-only`.
- [ ] Chạy ba truy vấn kiểm tra.
- [ ] Kiểm tra transcript bị ẩn trước khi nộp.
- [ ] Kiểm tra audio và transcript ở trang kết quả.
- [ ] Kiểm tra bài trắng là `0/495`.

