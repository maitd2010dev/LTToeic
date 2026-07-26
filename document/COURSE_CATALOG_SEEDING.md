# Hướng dẫn seed catalog khóa học TOEIC

> Cập nhật: 2026-07-26  
> Áp dụng cho `CourseCatalogSeeder`, JSON schema version `1`.

Seeder này tạo nhiều khóa học theo cấp độ từ một khóa nguồn đã có đầy đủ
section, lesson và quiz. Mỗi khóa mới chỉ lấy những section được khai báo trong
JSON, vì vậy catalog có các lộ trình tập trung khác nhau thay vì clone nguyên
khóa nguồn.

## 1. File liên quan

| Mục đích | File |
|---|---|
| Danh sách khóa học | `CoreLTToeic.UI/SeedData/course_catalog_toeic_levels.json` |
| Model JSON | `CoreLTToeic.Infrastructure/Data/Seeders/CourseCatalogJsonModels.cs` |
| Validate và clone | `CoreLTToeic.Infrastructure/Data/Seeders/CourseCatalogSeeder.cs` |
| Khóa nguồn | `CoreLTToeic.Infrastructure/Data/Seeders/CourseToeicSeeder.cs` |
| Tự quét file seed | `CoreLTToeic.UI/Program.cs` |

Ứng dụng tự quét:

```text
CoreLTToeic.UI/SeedData/course_catalog_*.json
```

`CourseToeicSeeder` chạy trước để đảm bảo khóa nguồn tồn tại, sau đó
`CourseCatalogSeeder` mới tạo catalog.

## 2. JSON schema

```json
{
  "schemaVersion": 1,
  "sourceCourseTitle": "[Complete TOEIC] Lộ trình 450–800+",
  "source": "CoreLTToeic.Infrastructure/Data/Seeders/CourseToeicSeeder.cs",
  "courses": [
    {
      "title": "TOEIC Listening 450–650",
      "description": "<p>Mô tả khóa học.</p>",
      "objective": "<ul><li>Mục tiêu học tập.</li></ul>",
      "thumbnailUrl": "/images/courses/complete-toeic-cover.jpg",
      "previewVideoUrl": "https://www.youtube.com/embed/example",
      "price": 0,
      "level": "Intermediate",
      "status": "Published",
      "sectionTitles": [
        "Chiến lược TOEIC Listening"
      ]
    }
  ]
}
```

Giá trị hợp lệ:

- `level`: `Beginner`, `Intermediate`, `Advanced`.
- `status`: `Draft`, `Published`.
- `price`: số không âm.
- `sectionTitles`: tên section phải trùng với khóa nguồn.

## 3. Dữ liệu được clone

Mỗi khóa mới có metadata riêng và clone:

- section được chọn;
- lesson dạng Text, Video hoặc Quiz;
- nội dung, mô tả, video, thời lượng và trạng thái học thử;
- quiz question, bốn lựa chọn và đáp án;
- thứ tự section, lesson và quiz được đánh lại từ 1.

Không clone dữ liệu người dùng:

- `CourseEnrollments`;
- `LessonCompletions`;
- `QuizAttempts` và câu trả lời;
- `CourseReviews`.

## 4. Idempotency và an toàn

Title khóa học là khóa nhận diện:

- title chưa tồn tại: tạo khóa mới;
- title đã có đầy đủ section/lesson và đúng level/status: bỏ qua;
- title đã có nhưng thiếu nội dung hoặc sai metadata: dừng, không xóa hay ghi
  đè;
- dữ liệu được tạo trong transaction;
- khóa nguồn và khóa đã có học viên không bị thay đổi.

Seeder từ chối cấu hình khi thiếu title, mô tả, mục tiêu, section hoặc khi
level/status không hợp lệ.

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

## 6. Kiểm tra sau khi seed

```sql
SELECT
    CASE Level
        WHEN 0 THEN N'Cơ bản'
        WHEN 1 THEN N'Khá'
        WHEN 2 THEN N'Nâng cao'
    END AS Level,
    COUNT(*) AS PublishedCourses
FROM Courses
WHERE Status = 1
GROUP BY Level
ORDER BY Level;
```

Catalog hiện tại mong đợi:

```text
Cơ bản: 4
Khá: 3
Nâng cao: 3
Tổng Published: 10
```

```sql
SELECT
    c.Id,
    c.Title,
    c.Level,
    (SELECT COUNT(*) FROM CourseSections s
        WHERE s.CourseId = c.Id) AS Sections,
    (SELECT COUNT(*)
        FROM CourseLessons l
        JOIN CourseSections s ON s.Id = l.SectionId
        WHERE s.CourseId = c.Id) AS Lessons,
    (SELECT COUNT(*)
        FROM QuizQuestions q
        JOIN CourseLessons l ON l.Id = q.LessonId
        JOIN CourseSections s ON s.Id = l.SectionId
        WHERE s.CourseId = c.Id) AS QuizQuestions,
    (SELECT COUNT(*) FROM CourseEnrollments e
        WHERE e.CourseId = c.Id) AS Enrollments
FROM Courses c
ORDER BY c.Id;
```

Mỗi khóa được cấu hình phải có section và lesson. Khóa mới phải có
`Enrollments = 0`.

## 7. Checklist

- [ ] Khóa nguồn tồn tại và có section, lesson, quiz hợp lệ.
- [ ] JSON đúng schema version 1 và pattern `course_catalog_*.json`.
- [ ] Title khóa học duy nhất.
- [ ] Level/status hợp lệ.
- [ ] Tên section trùng với khóa nguồn.
- [ ] Giữ đường dẫn nguồn trong field `source`.
- [ ] Build Release và chạy `--seed-only`.
- [ ] Kiểm tra số khóa Published theo cấp độ.
- [ ] Kiểm tra section, lesson, quiz và enrollment.
- [ ] Chạy seed lần hai để xác nhận không tạo trùng.
