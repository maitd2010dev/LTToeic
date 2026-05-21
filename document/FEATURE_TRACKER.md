# LTToeic — Bảng Theo Dõi Chức Năng

> Cập nhật lần cuối: 2026-05-20 (session 4)
> Đánh dấu `[x]` khi hoàn thành, thêm ngày hoàn thành vào cuối dòng.

---

## 1. Xác Thực Người Dùng (Authentication)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 1.1 | Đăng ký tài khoản (`/register`) | ✅ Hoàn thành | `Register.razor` + `AuthRepository` |
| 1.2 | Đăng nhập (`/login`) | ✅ Hoàn thành | Hidden form POST → `/api/auth/signin` (tạo auth cookie đúng cách trong Blazor Server) — 2026-05-17 |
| 1.3 | Xác nhận email (`/confirm`) | ✅ Hoàn thành | `GET /api/auth/confirmemail` đã tạo; `Confirm.razor` gọi `IAuthRepository` trực tiếp; sửa double-decode token — 2026-05-17 |
| 1.4 | Đăng xuất | ✅ Hoàn thành | `GET /api/auth/signout` dùng `HttpContext.SignOutAsync` — 2026-05-17 |
| 1.5 | Quên mật khẩu / Đặt lại mật khẩu | ❌ Chưa làm | |
| 1.6 | Đổi mật khẩu | ✅ Hoàn thành | Drawer trong NavMenu, `AuthRepository.ChangePasswordAsync` — 2026-05-17 |

---

## 2. Hồ Sơ Người Dùng (User Profile)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 2.1 | Trang hồ sơ cá nhân | ⚠️ Một phần | Dropdown + Drawer trong NavMenu (FullName, SĐT, Ngày sinh); chưa có trang hồ sơ riêng |
| 2.2 | Chỉnh sửa thông tin cá nhân | ✅ Hoàn thành | `UpdateProfileEditModel`, `AuthRepository.UpdateProfileAsync`, Drawer trong NavMenu — 2026-05-17 |
| 2.3 | Upload ảnh đại diện | ❌ Chưa làm | |
| 2.4 | Dashboard người dùng (lịch sử thi, tiến độ) | ❌ Chưa làm | |

---

## 3. Đề Thi TOEIC (Tests)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 3.1 | Danh sách đề thi (`/luyen-thi`) | ✅ Hoàn thành | `TestList.razor` |
| 3.2 | Làm bài thi (`/luyen-thi/{TestId}`) | ✅ Hoàn thành | `TakeTest.razor`, timer, answer sheet |
| 3.3 | Nộp bài & tính điểm tự động | ✅ Hoàn thành | `UserResultService.SubmitTestAsync` |
| 3.4 | Trang kết quả sau khi nộp bài | ✅ Hoàn thành | `TestResult.razor` `/ket-qua/{ResultId}` — 2026-05-17 |
| 3.5 | Xem lại đáp án chi tiết sau thi | ✅ Hoàn thành | `QuestionReviewItem.razor` — màu đúng/sai/bỏ qua, nhóm theo Part — 2026-05-17 |
| 3.6 | Lịch sử các lần thi của người dùng | ❌ Chưa làm | `UserResultRepository.GetByUserIdAsync` đã có |
| 3.7 | Chế độ luyện tập từng Part | ✅ Hoàn thành | `TestStartScreen` checkbox chọn Part; `TakeTest` filter `_parts` + `_questionIndex` giữ nguyên số câu theo DB — 2026-05-20 |
| 3.8 | Chế độ thi thử toàn bài (120 phút cố định) | ✅ Hoàn thành | `TestMode.Simulation`, màn hình chọn chế độ — 2026-05-17 |
| 3.9 | Lọc/tìm kiếm đề thi theo danh mục | ⚠️ Một phần | UI có filter, backend cần kiểm tra |
| 3.10 | Bảng quy đổi điểm Listening/Reading | ✅ Hoàn thành | Seeder 101 dòng (0–100→5–495), `ScoreConversionRepository` lookup — 2026-05-17 |
| 3.11 | Màn hình chọn chế độ thi trước khi bắt đầu | ✅ Hoàn thành | `TestStartScreen.razor` — Thi thử (120p) / Luyện tập (30/60/90/120p / không giới hạn) — 2026-05-17 |
| 3.12 | Timer đếm lên khi luyện tập không giới hạn | ✅ Hoàn thành | `_countUp` mode trong `TakeTest.razor`, lưu thời gian thực khi nộp — 2026-05-17 |

---

## 4. Quản Lý Đề Thi (Admin — Test Management)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 4.1 | Danh sách đề thi (admin) | ✅ Hoàn thành | `ExamManagementList.razor` `/admin/quan-li-de-thi` |
| 4.2 | Thêm / Sửa / Xoá đề thi | ✅ Hoàn thành | |
| 4.3 | Quản lý Part (Part 1–7) | ✅ Hoàn thành | `TestPartManager.razor` |
| 4.4 | Thêm / Sửa / Xoá câu hỏi đơn | ✅ Hoàn thành | |
| 4.5 | Thêm / Sửa / Xoá nhóm câu hỏi | ✅ Hoàn thành | |
| 4.6 | Upload audio cho câu hỏi | ✅ Hoàn thành | |
| 4.7 | Upload ảnh cho câu hỏi | ✅ Hoàn thành | |
| 4.8 | Preview câu hỏi realtime | ✅ Hoàn thành | `QuestionPreviewCard.razor`, `QuestionGroupPreviewCard.razor` |
| 4.9 | Quản lý danh mục đề thi | ⚠️ Một phần | Service method có, chưa có trang UI riêng |
| 4.10 | Import đề thi từ JSON/file | ⚠️ Có seeder | `ToeicTestSeeder` chỉ dùng để seed, chưa phải UI import |
| 4.11 | Phân quyền admin (bảo vệ route) | ✅ Hoàn thành | `[Authorize(Roles="Admin")]` trên `AdminLayout`, `ExamManagementList`, `CourseManagement`, `AdminIndex`; `AuthorizeRouteView` trong `Routes.razor` — 2026-05-17 |

---

## 5. Khóa Học (Courses)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 5.1 | Quản lý khóa học (admin) | ✅ Hoàn thành | `CourseManagement.razor` `/admin/quan-li-khoa-hoc` |
| 5.2 | Thêm / Sửa / Xoá khóa học | ✅ Hoàn thành | |
| 5.3 | Quản lý chương học (Section) | ✅ Hoàn thành | |
| 5.4 | Quản lý bài học (Lesson) | ✅ Hoàn thành | |
| 5.5 | Upload thumbnail khóa học | ✅ Hoàn thành | |
| 5.6 | Trang danh sách khóa học (user) | ❌ Chưa làm | NavMenu disabled |
| 5.7 | Trang chi tiết khóa học (user) | ❌ Chưa làm | |
| 5.8 | Đăng ký khóa học | ❌ Chưa làm | Entity `CourseEnrollment` có |
| 5.9 | Xem bài học / video | ❌ Chưa làm | |
| 5.10 | Theo dõi tiến độ học (LessonCompletion) | ❌ Chưa làm | Entity có |
| 5.11 | Quiz trong bài học | ❌ Chưa làm | Entity `QuizQuestion` + `QuizQuestionOption` có |
| 5.12 | Đánh giá / Review khóa học | ❌ Chưa làm | Entity `CourseReview` có |

---

## 6. Tài Liệu (Materials)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 6.1 | Trang tài liệu luyện thi | ❌ Chưa làm | NavMenu disabled |
| 6.2 | Upload / tải tài liệu PDF | ❌ Chưa làm | |
| 6.3 | Phân loại tài liệu theo Part | ❌ Chưa làm | |

---

## 7. Quản Trị Hệ Thống (Admin)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 7.1 | Dashboard thống kê (số user, lượt thi...) | ❌ Chưa làm | |
| 7.2 | Quản lý người dùng (danh sách, phân quyền) | ❌ Chưa làm | |
| 7.3 | Xem kết quả thi của tất cả user | ❌ Chưa làm | |
| 7.4 | Quản lý bảng quy đổi điểm | ⚠️ Một phần | Seeder + repository đã có, chưa có trang admin để chỉnh sửa |
| 7.5 | Phân quyền bảo vệ tất cả route `/admin/*` | ✅ Hoàn thành | `AdminSeeder` tạo role "Admin" + user `admin/admin`; middleware `UseAuthentication`/`UseAuthorization`; trang `/khong-co-quyen` (`AccessDenied.razor`) cho user không có quyền — 2026-05-17 |

---

## 8. Hạ Tầng & Kỹ Thuật

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 8.1 | Clean Architecture 4 layers | ✅ Hoàn thành | |
| 8.2 | ASP.NET Core Identity | ✅ Hoàn thành | |
| 8.3 | EF Core + SQL Server | ✅ Hoàn thành | |
| 8.4 | AutoMapper profiles | ✅ Hoàn thành | |
| 8.5 | Email xác nhận (SMTP Gmail) | ✅ Hoàn thành | Sửa `MailSettings` DI (`Configure<MailSettings>`), sửa config key `Username`, sửa `From` — 2026-05-17 |
| 8.6 | Endpoint `GET /api/auth/confirmemail` | ✅ Hoàn thành | Minimal API trong `Program.cs`; sửa double-decode token — 2026-05-17 |
| 8.7 | Seed data bảng quy đổi điểm | ✅ Hoàn thành | `ScoreConversionSeeder` — 101 dòng mỗi bảng, idempotent — 2026-05-17 |
| 8.8 | Components tái sử dụng kết quả thi | ✅ Hoàn thành | `TestScoreCard`, `TestResultStats`, `QuestionReviewItem`, `TestStartScreen` — 2026-05-17 |
| 8.9 | Hiệu suất trang kết quả | ✅ Hoàn thành | `Virtualize` lazy-render danh sách câu hỏi; `scrollTo(0,0)` khi load; ẩn điểm Nghe/Đọc khi không có Part tương ứng — 2026-05-20 |

---

## Tóm Tắt Nhanh

| Layer | Đã làm | Chưa làm |
|-------|--------|----------|
| Authentication | Đăng ký, đăng nhập, đăng xuất, xác nhận email, đổi mật khẩu | Quên MK / đặt lại MK |
| Test | Làm bài, nộp bài, admin CRUD, kết quả sau thi, xem lại đáp án, chọn chế độ thi, quy đổi điểm, luyện từng Part | Lịch sử thi |
| Course | Admin CRUD đầy đủ | Toàn bộ UI phía user |
| User Profile | Chỉnh sửa thông tin (Drawer NavMenu), đổi mật khẩu | Trang hồ sơ riêng, dashboard, ảnh đại diện |
| Admin | Quản lý đề thi, khóa học, phân quyền, seed admin, trang AccessDenied | Dashboard, user management |
| Tài liệu | — | Toàn bộ |

---

## Log Cập Nhật

| Ngày | Chức năng hoàn thành |
|------|---------------------|
| 2026-05-17 | Khởi tạo file tracking, khảo sát toàn bộ codebase |
| 2026-05-17 | **Ưu tiên 2 hoàn thành:** Trang kết quả `/ket-qua/{ResultId}`, xem lại đáp án theo Part, màn hình chọn chế độ thi (`TestStartScreen`), timer đếm lên cho luyện tập không giới hạn, seeder bảng quy đổi điểm TOEIC chuẩn, 4 shared components mới |
| 2026-05-17 | **Session 3 — Auth & Admin hoàn thành:** Sửa toàn bộ luồng đăng ký/đăng nhập Blazor Server (hidden form POST, auth cookie đúng cách); sửa SMTP config (`MailSettings` DI, key `Username`, `From`); sửa xác nhận email (endpoint + double-decode token); đăng xuất qua `HttpContext.SignOutAsync`; profile dropdown + Drawer đổi thông tin/mật khẩu; `AdminSeeder` tạo role + user `admin/admin`; `[Authorize(Roles="Admin")]` toàn bộ admin; `AuthorizeRouteView` + trang `AccessDenied.razor` (`/khong-co-quyen`) |
| 2026-05-20 | **Session 4 — Luyện Part + Hiệu suất kết quả:** 3.7 luyện từng Part (checkbox chọn Part trong `TestStartScreen`, filter `_parts`/`_questionIndex` trong `TakeTest`); `TestResult.razor` dùng `Virtualize` thay `@foreach`, `scrollTo(0,0)` khi render, ẩn điểm nghe/đọc khi không luyện Part đó (`TestScoreCard` + banner `TakeTest`) |
