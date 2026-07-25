# LTToeic — Bảng Theo Dõi Chức Năng

> Cập nhật lần cuối: 2026-07-25 (session 6)
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
| 1.7 | Khóa / mở khóa tài khoản | ✅ Hoàn thành | Admin khóa/mở khóa bằng ASP.NET Identity; cập nhật security stamp để thu hồi phiên cũ; tài khoản khóa không thể truy cập khóa học qua UI, URL trực tiếp hoặc nghiệp vụ backend — 2026-07-25 |

---

## 2. Hồ Sơ Người Dùng (User Profile)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 2.1 | Trang hồ sơ cá nhân | ⚠️ Một phần | Dropdown + Drawer trong NavMenu (FullName, SĐT, Ngày sinh); chưa có trang hồ sơ riêng |
| 2.2 | Chỉnh sửa thông tin cá nhân | ✅ Hoàn thành | `UpdateProfileEditModel`, `AuthRepository.UpdateProfileAsync`, Drawer trong NavMenu — 2026-05-17 |
| 2.3 | Upload ảnh đại diện | ❌ Chưa làm | |
| 2.4 | Dashboard người dùng (lịch sử thi, tiến độ) | ✅ Hoàn thành | `Dashboard.razor` `/tong-quan`: tổng lượt thi, điểm cao nhất/trung bình, thời gian học, biểu đồ tiến trình và 5 lần thi gần nhất |

---

## 3. Đề Thi TOEIC (Tests)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 3.1 | Danh sách đề thi (`/luyen-thi`) | ✅ Hoàn thành | `TestList.razor` |
| 3.2 | Làm bài thi (`/luyen-thi/{TestId}`) | ✅ Hoàn thành | `TakeTest.razor`, timer, answer sheet |
| 3.3 | Nộp bài & tính điểm tự động | ✅ Hoàn thành | `UserResultService.SubmitTestAsync` |
| 3.4 | Trang kết quả sau khi nộp bài | ✅ Hoàn thành | `TestResult.razor` `/ket-qua/{ResultId}` — 2026-05-17 |
| 3.5 | Xem lại đáp án chi tiết sau thi | ✅ Hoàn thành | Đủ 100 câu, navigator đúng/sai/bỏ qua, audio sticky theo Part, transcript sau khi hoàn thành — 2026-07-25 |
| 3.6 | Lịch sử các lần thi của người dùng | ✅ Hoàn thành | `ExamHistory.razor`, `ExamHistoryTable.razor`; hỗ trợ xem chi tiết kết quả và hiển thị trong Drawer của NavMenu |
| 3.7 | Chế độ luyện tập từng Part | ✅ Hoàn thành | `TestStartScreen` checkbox chọn Part; `TakeTest` filter `_parts` + `_questionIndex` giữ nguyên số câu theo DB — 2026-05-20 |
| 3.8 | Chế độ thi thử toàn bài (120 phút cố định) | ✅ Hoàn thành | `TestMode.Simulation`, màn hình chọn chế độ — 2026-05-17 |
| 3.9 | Lọc/tìm kiếm đề thi theo danh mục | ⚠️ Một phần | UI có filter, backend cần kiểm tra |
| 3.10 | Bảng quy đổi điểm Listening/Reading | ✅ Hoàn thành | Seeder 101 dòng; 0 câu đúng = 0 điểm; Listening-only tối đa 495 — 2026-07-25 |
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
| 4.9 | Quản lý danh mục đề thi | ➖ Không triển khai UI riêng | Đã xóa tab/ô “Danh mục” khỏi giao diện Admin; danh mục vẫn được dùng nội bộ để phân loại đề thi — 2026-07-25 |
| 4.10 | Import đề thi từ JSON/file | ✅ Seeder/CLI | Tự quét `SeedData/toeic_listening_*.json`; chạy trực tiếp DB bằng `--seed-only`; hướng dẫn tại `document/LISTENING_TEST_SEEDING.md` — 2026-07-25 |
| 4.11 | Phân quyền admin (bảo vệ route) | ✅ Hoàn thành | `[Authorize(Roles="Admin")]` trên `AdminLayout`, `ExamManagementList`, `CourseManagement`, `AdminIndex`; `AuthorizeRouteView` trong `Routes.razor` — 2026-05-17 |

---

## 5. Khóa Học (Courses)

| # | Chức năng | Trạng thái | Ghi chú |
|---|-----------|-----------|---------|
| 5.1 | Quản lý khóa học (admin) | ✅ Hoàn thành | `CourseManagement.razor` `/admin/quan-li-khoa-hoc`; editor nội dung dùng chung `RichTextEditor`, hỗ trợ ảnh và nội dung HTML |
| 5.2 | Thêm / Sửa / Xoá khóa học | ✅ Hoàn thành | |
| 5.3 | Quản lý chương học (Section) | ✅ Hoàn thành | |
| 5.4 | Quản lý bài học (Lesson) | ✅ Hoàn thành | |
| 5.5 | Upload thumbnail khóa học | ✅ Hoàn thành | Hỗ trợ thumbnail, ảnh trong bài học và video nhúng; seed khóa học có dữ liệu/media minh họa |
| 5.6 | Trang danh sách khóa học (user) | ✅ Hoàn thành | `CourseCatalog.razor` `/khoa-hoc`; tìm kiếm, lọc cấp độ Cơ bản/Khá/Nâng cao và hiển thị tiến độ |
| 5.7 | Trang chi tiết khóa học (user) | ✅ Hoàn thành | `CourseDetails.razor` `/khoa-hoc/{Id}`; mô tả, mục tiêu, chương trình học, video giới thiệu và đánh giá |
| 5.8 | Đăng ký khóa học | ✅ Hoàn thành | Đăng ký miễn phí, quản lý trạng thái enrollment và trang `/khoa-hoc-cua-toi` |
| 5.9 | Xem bài học / video | ✅ Hoàn thành | `CourseLearning.razor` `/khoa-hoc/{Id}/hoc`; bài đọc rich content, ảnh và video nhúng |
| 5.10 | Theo dõi tiến độ học (LessonCompletion) | ✅ Hoàn thành | Hoàn thành bài học, tự tính lại tiến độ khóa học và trạng thái enrollment |
| 5.11 | Quiz trong bài học | ✅ Hoàn thành | Nộp bài, chấm điểm, lưu lịch sử nhiều lần làm; thứ tự câu hỏi hiển thị đúng từ 1..n — 2026-07-25 |
| 5.12 | Đánh giá / Review khóa học | ✅ Hoàn thành | Học viên đã đăng ký có thể tạo/cập nhật đánh giá; Admin có thể xem và xóa |
| 5.13 | Học thử | ➖ Tạm ẩn | Chưa cần trong giai đoạn hiện tại; đã ẩn nút và nhãn “Học thử” ở phía người dùng — 2026-07-25 |
| 5.14 | Bảo vệ khóa học với tài khoản bị khóa | ✅ Hoàn thành | Ẩn link trên UI; middleware chặn route; repository kiểm tra `LockoutEnd` cho toàn bộ thao tác đọc/ghi khóa học — 2026-07-25 |

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
| 7.1 | Dashboard thống kê (số user, lượt thi...) | ✅ Hoàn thành | `AdminIndex.razor`: người dùng, đề thi, lượt thi hoàn thành và khóa học; đã lược bỏ điểm trung bình, Top 5 đề thi và hoạt động 7 ngày theo yêu cầu — 2026-07-25 |
| 7.2 | Quản lý người dùng (danh sách, phân quyền) | ✅ Hoàn thành | `UserManagement.razor`: tìm kiếm, trạng thái email, role, số lượt thi và khóa/mở khóa tài khoản |
| 7.3 | Xem kết quả thi của tất cả user | ✅ Hoàn thành | `ExamResultsManagement.razor`: tìm kiếm, tổng/Listening/Reading, độ chính xác, chế độ và trạng thái bài thi |
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
| 8.10 | Header / Footer dùng chung | ✅ Hoàn thành | `NavMenu` và `SiteFooter` được tái sử dụng trong `MainLayout`; footer responsive và dùng chung trạng thái truy cập khóa học — 2026-07-25 |
| 8.11 | Cột thao tác cố định trong bảng | ✅ Hoàn thành | Toàn bộ `ActionColumn` nằm cuối bảng được cố định bên phải bằng `ColumnFixPlacement.Right`, bổ sung `ScrollX` và `Width` khi cần — 2026-07-25 |
| 8.12 | Rich text và media khóa học | ✅ Hoàn thành | Tái sử dụng `RichTextEditor`, sửa cấu hình Quill, hỗ trợ ảnh/video và render nội dung khóa học an toàn, sinh động — 2026-07-25 |

---

## Tóm Tắt Nhanh

| Layer | Đã làm | Chưa làm |
|-------|--------|----------|
| Authentication | Đăng ký, đăng nhập, đăng xuất, xác nhận email, đổi mật khẩu | Quên MK / đặt lại MK |
| Test | Làm bài, nộp bài, admin CRUD, kết quả sau thi, xem lại đáp án, lịch sử thi, chọn chế độ thi, quy đổi điểm, luyện từng Part | Import đề thi qua UI |
| Course | Admin CRUD, rich text/media, catalog và chi tiết, đăng ký, học bài/video, tiến độ, quiz, đánh giá, bảo vệ tài khoản khóa | Học thử đang tạm ẩn |
| User Profile | Chỉnh sửa thông tin (Drawer NavMenu), đổi mật khẩu, dashboard và lịch sử thi | Trang hồ sơ riêng, ảnh đại diện |
| Admin | Dashboard, quản lý tài khoản/khóa, kết quả thi, đề thi, khóa học, phân quyền, trang AccessDenied | Quản lý bảng quy đổi điểm qua UI |
| Tài liệu | — | Toàn bộ |

---

## Log Cập Nhật

| Ngày | Chức năng hoàn thành |
|------|---------------------|
| 2026-05-17 | Khởi tạo file tracking, khảo sát toàn bộ codebase |
| 2026-05-17 | **Ưu tiên 2 hoàn thành:** Trang kết quả `/ket-qua/{ResultId}`, xem lại đáp án theo Part, màn hình chọn chế độ thi (`TestStartScreen`), timer đếm lên cho luyện tập không giới hạn, seeder bảng quy đổi điểm TOEIC chuẩn, 4 shared components mới |
| 2026-05-17 | **Session 3 — Auth & Admin hoàn thành:** Sửa toàn bộ luồng đăng ký/đăng nhập Blazor Server (hidden form POST, auth cookie đúng cách); sửa SMTP config (`MailSettings` DI, key `Username`, `From`); sửa xác nhận email (endpoint + double-decode token); đăng xuất qua `HttpContext.SignOutAsync`; profile dropdown + Drawer đổi thông tin/mật khẩu; `AdminSeeder` tạo role + user `admin/admin`; `[Authorize(Roles="Admin")]` toàn bộ admin; `AuthorizeRouteView` + trang `AccessDenied.razor` (`/khong-co-quyen`) |
| 2026-05-20 | **Session 4 — Luyện Part + Hiệu suất kết quả:** 3.7 luyện từng Part (checkbox chọn Part trong `TestStartScreen`, filter `_parts`/`_questionIndex` trong `TakeTest`); `TestResult.razor` dùng `Virtualize` thay `@foreach`, `scrollTo(0,0)` khi render, ẩn điểm nghe/đọc khi không luyện Part đó (`TestScoreCard` + banner `TakeTest`) |
| 2026-07-25 | **Session 5 — Hoàn thiện Course & giao diện:** Hoàn thành toàn bộ luồng khóa học phía người dùng (catalog, chi tiết, đăng ký, học rich text/ảnh/video, tiến độ, quiz, lịch sử làm bài, review); bổ sung seed dữ liệu/media; sửa thứ tự quiz; Việt hóa cấp độ thành Cơ bản/Khá/Nâng cao; tạm ẩn Học thử; bảo vệ khóa học với tài khoản bị khóa ở UI/middleware/repository; cố định các ActionColumn cuối bảng; tinh gọn dashboard Admin và xóa tab Danh mục; thêm `SiteFooter` responsive dùng chung trong `MainLayout` |
