using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class CourseToeicSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CourseToeicSeeder> _logger;

    public CourseToeicSeeder(IDbContextFactory<AppDbContext> contextFactory, ILogger<CourseToeicSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        if (await ctx.Courses.AnyAsync(c => c.Title == "TOEIC Cơ Bản cho Người Mới"))
        {
            _logger.LogInformation("TOEIC course already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding TOEIC beginner course...");

        var course = new Course
        {
            Title       = "TOEIC Cơ Bản cho Người Mới",
            Description = "Khoá học TOEIC cơ bản dành cho người mới bắt đầu. Bao gồm từ vựng thiết yếu, ngữ pháp nền tảng và bài luyện thi thực tế theo định dạng TOEIC chính thức.",
            Objective   = "Sau khi hoàn thành khoá học, học viên sẽ nắm vững 500+ từ vựng TOEIC thông dụng, hiểu các cấu trúc ngữ pháp cơ bản, và tự tin làm các dạng câu hỏi Part 5 & Part 6.",
            Price       = 0,
            Status      = CourseStatus.Published,
            Level       = CourseLevel.Beginner,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow,
            Sections    = new List<CourseSection>
            {
                BuildVocabularySection(),
                BuildGrammarSection(),
                BuildPracticeSection()
            }
        };

        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        _logger.LogInformation("TOEIC beginner course seeded successfully.");
    }

    // ─────────────────────────────────────────────────────────────
    // SECTION 1: TỪ VỰNG TOEIC
    // ─────────────────────────────────────────────────────────────
    private static CourseSection BuildVocabularySection() => new()
    {
        Title      = "Từ vựng TOEIC",
        OrderIndex = 1,
        CreatedAt  = DateTime.UtcNow,
        UpdatedAt  = DateTime.UtcNow,
        Lessons    = new List<CourseLesson>
        {
            new()
            {
                Title      = "Từ vựng Văn phòng & Công sở",
                Type       = LessonType.Text,
                OrderIndex = 1,
                Duration   = 20,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Từ vựng Văn phòng & Công sở

Đây là những từ vựng xuất hiện thường xuyên nhất trong các bài thi TOEIC liên quan đến môi trường văn phòng.

| Từ vựng | Loại từ | Nghĩa | Ví dụ |
|---|---|---|---|
| **agenda** | n | chương trình nghị sự | *The agenda for today's meeting has been distributed.* |
| **deadline** | n | hạn chót | *We must meet the deadline by Friday.* |
| **memorandum / memo** | n | bản ghi nhớ nội bộ | *A memo was sent to all employees.* |
| **invoice** | n | hoá đơn | *Please send the invoice to our accounting department.* |
| **proposal** | n | đề xuất, bản đề nghị | *She submitted a proposal for the new project.* |
| **presentation** | n | buổi thuyết trình | *The sales presentation went very well.* |
| **supervisor** | n | người giám sát | *My supervisor approved the budget.* |
| **colleague** | n | đồng nghiệp | *I have lunch with my colleagues every day.* |
| **headquarters** | n | trụ sở chính | *The headquarters is located in New York.* |
| **overtime** | n/adv | làm thêm giờ | *She worked overtime to finish the report.* |
| **promotion** | n | sự thăng chức | *He received a promotion last month.* |
| **recruit** | v | tuyển dụng | *The company is recruiting new staff.* |
| **evaluate** | v | đánh giá | *We need to evaluate the results carefully.* |
| **delegate** | v | giao phó, uỷ quyền | *The manager delegated the task to her assistant.* |
| **submit** | v | nộp, gửi | *Please submit your report by tomorrow.* |
| **approve** | v | phê duyệt | *The board approved the new policy.* |
| **revise** | v | sửa đổi, xem lại | *Could you revise this draft before the meeting?* |
| **distribute** | v | phân phối, phát | *Distribute the handouts before the presentation.* |
| **collaborate** | v | hợp tác | *Our teams will collaborate on this project.* |
| **postpone** | v | hoãn lại | *The meeting was postponed to next week.* |
| **confirm** | v | xác nhận | *Please confirm your attendance by email.* |
| **notify** | v | thông báo | *Notify all staff of the schedule change.* |
| **implement** | v | thực hiện, triển khai | *We will implement the new system next quarter.* |
| **efficient** | adj | hiệu quả | *The new process is more efficient.* |
| **mandatory** | adj | bắt buộc | *Attendance at the meeting is mandatory.* |

### Mẹo học từ vựng

> **Nhóm theo chủ đề**: Học từ theo nhóm (động từ hành động, danh từ vai trò, tính từ mô tả) giúp ghi nhớ lâu hơn.
>
> **Chú ý collocations**: Ví dụ *submit a report*, *approve a proposal*, *meet a deadline* — cụm từ đi kèm rất quan trọng trong TOEIC.
"""
            },

            new()
            {
                Title      = "Từ vựng Kinh doanh & Tài chính",
                Type       = LessonType.Text,
                OrderIndex = 2,
                Duration   = 20,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Từ vựng Kinh doanh & Tài chính

Từ vựng về tài chính và kinh doanh xuất hiện nhiều trong Part 5, Part 6, và Part 7 của bài thi TOEIC.

| Từ vựng | Loại từ | Nghĩa | Ví dụ |
|---|---|---|---|
| **revenue** | n | doanh thu | *Annual revenue increased by 15% this year.* |
| **expense** | n | chi phí, khoản chi | *Travel expenses must be reported monthly.* |
| **profit** | n | lợi nhuận | *The company made a profit of $2 million.* |
| **budget** | n | ngân sách | *We are working within a tight budget.* |
| **investment** | n | đầu tư | *The investment in new technology paid off.* |
| **dividend** | n | cổ tức | *Shareholders receive dividends quarterly.* |
| **shareholder** | n | cổ đông | *The shareholders voted for the new CEO.* |
| **merger** | n | sáp nhập | *The merger was completed last year.* |
| **acquisition** | n | thâu tóm, mua lại | *The acquisition of the startup cost $50M.* |
| **audit** | n/v | kiểm toán | *The annual audit begins next week.* |
| **cash flow** | n | dòng tiền | *Good cash flow is essential for growth.* |
| **commission** | n | hoa hồng | *Salespeople earn a 10% commission.* |
| **discount** | n/v | giảm giá, chiết khấu | *We offer a 20% discount for bulk orders.* |
| **wholesale** | n/adj | bán buôn | *We buy products at wholesale prices.* |
| **retail** | n/adj | bán lẻ | *The retail price is higher than wholesale.* |
| **inventory** | n | hàng tồn kho | *Check the inventory before placing an order.* |
| **supplier** | n | nhà cung cấp | *We have three main suppliers for parts.* |
| **vendor** | n | người bán hàng | *The vendor delivered the goods on time.* |
| **warranty** | n | bảo hành | *The product comes with a 2-year warranty.* |
| **refund** | n/v | hoàn tiền | *Customers can request a full refund.* |
| **contract** | n | hợp đồng | *Both parties signed the contract.* |
| **negotiate** | v | đàm phán | *We need to negotiate better terms.* |
| **fiscal** | adj | thuộc tài khoá | *Our fiscal year ends in December.* |
| **quarterly** | adv/adj | hàng quý | *We report earnings quarterly.* |
| **annual** | adj | hàng năm | *The annual report is due next month.* |

### Cụm từ thường gặp trong TOEIC

- **balance the budget** — cân bằng ngân sách
- **cut costs** — cắt giảm chi phí
- **increase revenue** — tăng doanh thu
- **sign a contract** — ký hợp đồng
- **place an order** — đặt hàng
- **process a refund** — xử lý hoàn tiền
"""
            },

            new()
            {
                Title      = "Từ vựng Du lịch & Vận tải",
                Type       = LessonType.Text,
                OrderIndex = 3,
                Duration   = 15,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Từ vựng Du lịch & Vận tải

Chủ đề này xuất hiện thường xuyên trong Part 1 (ảnh sân bay, bến tàu) và Part 3-4 (hội thoại đặt vé, thông báo chuyến bay).

| Từ vựng | Loại từ | Nghĩa | Ví dụ |
|---|---|---|---|
| **itinerary** | n | lịch trình | *The travel agency prepared a detailed itinerary.* |
| **reservation** | n | đặt chỗ, đặt phòng | *I made a reservation at the hotel.* |
| **accommodation** | n | chỗ ở | *The conference includes free accommodation.* |
| **boarding pass** | n | thẻ lên máy bay | *Please have your boarding pass ready.* |
| **departure** | n | khởi hành | *The departure is scheduled for 9 AM.* |
| **arrival** | n | đến nơi | *The estimated arrival time is 3 PM.* |
| **customs** | n | hải quan | *All passengers must go through customs.* |
| **immigration** | n | xuất nhập cảnh | *Please have your passport ready at immigration.* |
| **terminal** | n | nhà ga, terminal | *Flight 402 departs from Terminal B.* |
| **transit** | n/adj | quá cảnh | *We have a 2-hour transit in Singapore.* |
| **baggage / luggage** | n | hành lý | *Please claim your baggage at carousel 3.* |
| **fare** | n | giá vé | *The train fare is $15 per trip.* |
| **round-trip** | adj/n | khứ hồi | *A round-trip ticket is cheaper than two one-ways.* |
| **one-way** | adj | một chiều | *I need a one-way ticket to London.* |
| **delay** | n/v | chậm trễ, hoãn | *The flight was delayed by two hours.* |
| **cancel** | v | huỷ bỏ | *The trip was cancelled due to bad weather.* |
| **confirm** | v | xác nhận | *Please confirm your booking 24 hours in advance.* |
| **check-in** | n/v | làm thủ tục lên máy bay | *Online check-in opens 24 hours before departure.* |
| **check-out** | n/v | trả phòng | *Check-out time is 11 AM.* |
| **destination** | n | điểm đến | *What is your final destination?* |
| **connecting flight** | n | chuyến bay nối | *There is a 1-hour layover for the connecting flight.* |
| **carry-on** | n/adj | hành lý xách tay | *Only one carry-on bag is allowed.* |
| **shuttle** | n | xe đưa đón | *A free shuttle runs between the hotel and airport.* |

### Thông báo sân bay thường gặp

> **"Flight XY 101 is now boarding at Gate 12."** — Chuyến bay XY 101 hiện đang lên máy bay tại cổng 12.
>
> **"Due to weather conditions, your flight has been delayed."** — Do thời tiết, chuyến bay của bạn bị trễ.
>
> **"Please proceed to the baggage claim area."** — Vui lòng đến khu vực nhận hành lý.
"""
            },

            new()
            {
                Title      = "Từ vựng Nhà hàng & Khách sạn",
                Type       = LessonType.Text,
                OrderIndex = 4,
                Duration   = 15,
                IsFree     = false,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Từ vựng Nhà hàng & Khách sạn

Chủ đề nhà hàng và khách sạn xuất hiện trong Part 2 (câu hỏi đặt bàn) và Part 3-4 (hội thoại phục vụ khách).

| Từ vựng | Loại từ | Nghĩa | Ví dụ |
|---|---|---|---|
| **reservation** | n | đặt bàn / đặt phòng | *I have a reservation for two at 7 PM.* |
| **menu** | n | thực đơn | *May I see the menu, please?* |
| **appetizer** | n | món khai vị | *We ordered soup as an appetizer.* |
| **entrée** | n | món chính | *The salmon is today's entrée special.* |
| **dessert** | n | món tráng miệng | *Would you like to order dessert?* |
| **beverage** | n | đồ uống | *Beverages are included in the set menu.* |
| **complimentary** | adj | miễn phí (do khách sạn/nhà hàng cung cấp) | *Breakfast is complimentary for hotel guests.* |
| **amenities** | n | tiện nghi | *The room comes with all basic amenities.* |
| **concierge** | n | nhân viên hướng dẫn khách sạn | *The concierge helped us book a tour.* |
| **front desk** | n | lễ tân | *Please collect your key at the front desk.* |
| **housekeeping** | n | dịch vụ dọn phòng | *Housekeeping will clean your room daily.* |
| **valet** | n | người giữ xe | *Valet parking is available for $20.* |
| **suite** | n | phòng suite | *They upgraded us to a deluxe suite.* |
| **vacancy** | n | phòng trống | *Do you have any vacancies for tonight?* |
| **receipt** | n | biên lai | *Please keep your receipt for refund purposes.* |
| **gratuity / tip** | n | tiền boa | *A 15% gratuity is included in the bill.* |
| **banquet** | n | bữa tiệc lớn | *The conference includes a banquet dinner.* |
| **catering** | n | dịch vụ ăn uống | *The company hired a catering service.* |
| **buffet** | n | tiệc buffet | *The hotel offers a breakfast buffet.* |
| **cuisine** | n | ẩm thực | *The restaurant specialises in French cuisine.* |

### Mẫu hội thoại đặt bàn

> **"I'd like to make a reservation for four people this Saturday evening."**
> — *"Of course. What time would you prefer?"*
>
> **"Do you have any tables available for walk-in guests?"**
> — *"I'm sorry, we're fully booked tonight."*
"""
            },

            new()
            {
                Title         = "Ôn tập từ vựng",
                Type          = LessonType.Quiz,
                OrderIndex    = 5,
                Duration      = 15,
                IsFree        = true,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow,
                QuizQuestions = BuildVocabQuiz()
            }
        }
    };

    private static List<QuizQuestion> BuildVocabQuiz() =>
    [
        Q("The word 'agenda' means:",
            "A list of topics to be discussed in a meeting",
            "A type of financial report",
            "A company's annual plan",
            "A formal letter of request",
            "1", "vocabulary"),

        Q("Choose the word that means 'the final date by which something must be done':",
            "Deadline",
            "Overtime",
            "Schedule",
            "Duration",
            "1", "vocabulary"),

        Q("An 'invoice' is:",
            "A business proposal",
            "A formal complaint letter",
            "A document requesting payment for goods or services",
            "A meeting agenda",
            "3", "vocabulary"),

        Q("The word 'revenue' refers to:",
            "A company's total costs",
            "Money that a company earns from its business activities",
            "The profit after taxes",
            "An investment return",
            "2", "vocabulary"),

        Q("Choose the word that means 'to officially say that something is acceptable':",
            "Submit",
            "Evaluate",
            "Approve",
            "Notify",
            "3", "vocabulary"),

        Q("A 'boarding pass' is used when:",
            "Checking into a hotel",
            "Boarding an airplane",
            "Entering a country through customs",
            "Checking out of a hotel",
            "2", "vocabulary"),

        Q("The word 'itinerary' means:",
            "A list of expenses for a trip",
            "A travel plan showing the places and times of a journey",
            "A type of airline ticket",
            "A hotel reservation number",
            "2", "vocabulary"),

        Q("'Complimentary breakfast' means the breakfast is:",
            "Optional",
            "Extra expensive",
            "Provided free of charge",
            "Only for VIP guests",
            "3", "vocabulary"),

        Q("Choose the word that means 'to put something off to a later time':",
            "Cancel",
            "Postpone",
            "Confirm",
            "Submit",
            "2", "vocabulary"),

        Q("A 'vendor' is:",
            "A type of business contract",
            "A company that provides goods or services",
            "A financial document",
            "A type of hotel room",
            "2", "vocabulary"),
    ];

    // ─────────────────────────────────────────────────────────────
    // SECTION 2: NGỮ PHÁP TOEIC
    // ─────────────────────────────────────────────────────────────
    private static CourseSection BuildGrammarSection() => new()
    {
        Title      = "Ngữ pháp TOEIC",
        OrderIndex = 2,
        CreatedAt  = DateTime.UtcNow,
        UpdatedAt  = DateTime.UtcNow,
        Lessons    = new List<CourseLesson>
        {
            new()
            {
                Title      = "Thì động từ (Verb Tenses)",
                Type       = LessonType.Text,
                OrderIndex = 1,
                Duration   = 25,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Thì động từ — Verb Tenses

Câu hỏi về thì động từ chiếm khoảng **20-25% Part 5** trong bài thi TOEIC. Bạn cần nắm vững 6 thì phổ biến nhất.

---

### 1. Present Simple (Hiện tại đơn)

**Dùng khi:** Sự thật hiển nhiên, thói quen thường xuyên, chính sách/quy định công ty.

| Cấu trúc | Ví dụ |
|---|---|
| S + V(s/es) | *The store **opens** at 9 AM every day.* |
| S + do/does + not + V | *She **does not** attend weekly meetings.* |

**Từ tín hiệu:** always, usually, often, every day/week/month, generally

---

### 2. Present Continuous (Hiện tại tiếp diễn)

**Dùng khi:** Hành động đang xảy ra tại thời điểm nói, kế hoạch trong tương lai gần.

| Cấu trúc | Ví dụ |
|---|---|
| S + am/is/are + V-ing | *We **are currently reviewing** the proposal.* |

**Từ tín hiệu:** now, at the moment, currently, right now

---

### 3. Past Simple (Quá khứ đơn)

**Dùng khi:** Hành động đã hoàn thành tại một thời điểm cụ thể trong quá khứ.

| Cấu trúc | Ví dụ |
|---|---|
| S + V-ed / V2 | *The manager **approved** the budget yesterday.* |

**Từ tín hiệu:** yesterday, last week/month/year, in 2020, ago

---

### 4. Present Perfect (Hiện tại hoàn thành)

**Dùng khi:** Hành động xảy ra trong quá khứ có liên quan đến hiện tại, kết quả còn ảnh hưởng đến bây giờ.

| Cấu trúc | Ví dụ |
|---|---|
| S + have/has + V-ed/V3 | *The company **has expanded** to 15 countries.* |

**Từ tín hiệu:** already, just, yet, recently, since, for, ever, never

---

### 5. Past Perfect (Quá khứ hoàn thành)

**Dùng khi:** Hành động xảy ra TRƯỚC một hành động khác trong quá khứ.

| Cấu trúc | Ví dụ |
|---|---|
| S + had + V-ed/V3 | *She **had submitted** the report before the meeting started.* |

**Từ tín hiệu:** before, after, by the time, when (kết hợp với quá khứ đơn)

---

### 6. Future (Tương lai)

| Cấu trúc | Dùng khi | Ví dụ |
|---|---|---|
| will + V | Quyết định tức thời, dự đoán | *We **will send** the contract tomorrow.* |
| be going to + V | Kế hoạch có chủ đích | *They **are going to** hire 10 new staff.* |

---

### ⚠️ Bẫy phổ biến trong TOEIC

> **Bẫy 1:** Dùng *since* với **Present Perfect**, không phải Past Simple.
> ✅ *The company **has been** here since 2010.*
> ❌ *The company **was** here since 2010.*
>
> **Bẫy 2:** *by the time* + quá khứ đơn → vế chính dùng **Past Perfect**.
> ✅ *By the time she arrived, the meeting **had ended**.*
"""
            },

            new()
            {
                Title      = "Danh từ và Mạo từ (Nouns & Articles)",
                Type       = LessonType.Text,
                OrderIndex = 2,
                Duration   = 20,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Danh từ và Mạo từ — Nouns & Articles

Mạo từ và danh từ là hai trong số các dạng câu hỏi hay gặp nhất ở Part 5 TOEIC.

---

### Danh từ (Nouns)

#### Danh từ đếm được vs. không đếm được

| Loại | Ví dụ | Ghi chú |
|---|---|---|
| **Đếm được** (countable) | report, meeting, employee | Có thể dùng a/an, có dạng số nhiều |
| **Không đếm được** (uncountable) | information, equipment, advice, furniture | Không dùng a/an, không có số nhiều |

> ⚠️ **Bẫy TOEIC:** *information*, *equipment*, *advice*, *furniture*, *news*, *luggage* là danh từ **không đếm được** — không bao giờ thêm -s.
> ✅ *Please give me some **advice**.*
> ❌ *Please give me some **advices**.*

---

### Mạo từ A / An / The

| Mạo từ | Dùng khi | Ví dụ |
|---|---|---|
| **a** | Danh từ đếm được số ít, nhắc đến lần đầu, phụ âm | *We hired **a** new manager.* |
| **an** | Danh từ đếm được số ít, nhắc đến lần đầu, nguyên âm (a,e,i,o,u) | *She sent **an** email.* |
| **the** | Đã nhắc đến trước đó, danh từ duy nhất, cụ thể | *Please read **the** report I sent.* |
| **∅ (không)** | Danh từ không đếm được chung chung, số nhiều chung chung | ***Information** is key. / **Meetings** are important.* |

---

### Hậu tố tạo danh từ thường gặp trong TOEIC

| Hậu tố | Ví dụ |
|---|---|
| -tion / -sion | *information, promotion, expansion* |
| -ment | *management, employment, achievement* |
| -ance / -ence | *performance, attendance, conference* |
| -ity | *productivity, availability, reliability* |
| -er / -or | *manager, director, supervisor* |
| -ee | *employee, trainee, attendee* |

---

### Phân biệt danh từ với từ loại khác

Nhiều câu hỏi Part 5 yêu cầu chọn đúng **từ loại**. Ví dụ:

> The **______** of the new product was a great success.
> (A) launch *(v)* &nbsp; **(B) launching** *(n-gerund)* &nbsp; **(C) launched** *(adj/v-past)* &nbsp; **✅ (D) launch** *(n)*

→ Sau mạo từ *the*, cần một **danh từ**: *The launch of the new product...*
"""
            },

            new()
            {
                Title      = "Tính từ và Trạng từ (Adjectives & Adverbs)",
                Type       = LessonType.Text,
                OrderIndex = 3,
                Duration   = 20,
                IsFree     = false,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Tính từ và Trạng từ — Adjectives & Adverbs

Đây là một trong những dạng câu hỏi **từ loại** (word form) phổ biến nhất trong TOEIC Part 5.

---

### Tính từ (Adjectives)

**Vị trí:**
1. Trước danh từ: *a **successful** project*
2. Sau động từ liên kết (be, seem, become, feel, look, appear): *The results **are impressive**.*

**Hậu tố tính từ phổ biến:**

| Hậu tố | Ví dụ |
|---|---|
| -ful | successful, useful, powerful |
| -less | careless, effortless, useless |
| -ive | effective, productive, creative |
| -al | annual, professional, financial |
| -ous | various, numerous, previous |
| -able / -ible | reliable, available, responsible |
| -ent / -ant | efficient, significant, relevant |

---

### Trạng từ (Adverbs)

**Vị trí:**
1. Trước tính từ hoặc trạng từ khác: *The project was **extremely** successful.*
2. Trước hoặc sau động từ chính: *She **carefully** reviewed the document.* / *The meeting ended **abruptly**.*
3. Đầu câu: ***Currently**, the company is expanding.*

**Hậu tố:** Phần lớn tạo bằng cách thêm **-ly** vào tính từ.

| Tính từ | Trạng từ |
|---|---|
| efficient | efficiently |
| significant | significantly |
| recent | recently |
| current | currently |
| immediate | immediately |
| approximate | approximately |

---

### ⚠️ Phân biệt Tính từ vs. Trạng từ trong TOEIC

> **Bẫy phổ biến:** Chọn giữa tính từ và trạng từ.

> *The new system works ______.*
> (A) efficient &nbsp; **(B) ✅ efficiently** &nbsp; (C) efficiency &nbsp; (D) more efficient

→ Bổ nghĩa cho **động từ** *works* → cần **trạng từ** *efficiently*.

> *The ______ solution was found quickly.*
> (A) efficiently &nbsp; **(B) ✅ efficient** &nbsp; (C) efficiency &nbsp; (D) efficiencies

→ Bổ nghĩa cho **danh từ** *solution* → cần **tính từ** *efficient*.
"""
            },

            new()
            {
                Title      = "Giới từ thông dụng (Prepositions)",
                Type       = LessonType.Text,
                OrderIndex = 4,
                Duration   = 20,
                IsFree     = false,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Giới từ thông dụng — Prepositions

Giới từ là một trong những phần khó nhất trong TOEIC vì chúng thường không theo quy tắc rõ ràng và phải học theo cụm từ cố định.

---

### Giới từ chỉ thời gian

| Giới từ | Dùng với | Ví dụ |
|---|---|---|
| **at** | Giờ cụ thể, ngày lễ | *at 3 PM / at Christmas / at noon* |
| **on** | Ngày cụ thể, thứ trong tuần | *on Monday / on July 4th / on weekdays* |
| **in** | Tháng, năm, mùa, thế kỷ | *in March / in 2024 / in the morning* |
| **by** | Hạn chót | *Please submit **by** Friday.* |
| **until / till** | Kéo dài đến | *The store is open **until** 10 PM.* |
| **for** | Khoảng thời gian | *She worked there **for** five years.* |
| **since** | Từ thời điểm cụ thể | *He has been here **since** Monday.* |
| **within** | Trong vòng | *Please reply **within** 48 hours.* |
| **during** | Trong suốt khoảng thời gian | *No calls **during** the meeting.* |

---

### Giới từ chỉ nơi chốn

| Giới từ | Dùng với | Ví dụ |
|---|---|---|
| **at** | Địa điểm cụ thể | *at the airport / at the office* |
| **in** | Không gian có chiều | *in the room / in New York / in the box* |
| **on** | Bề mặt | *on the desk / on the floor / on page 5* |

---

### Cụm giới từ cố định (Prepositional Phrases) — Phải học thuộc

| Cụm từ | Nghĩa | Ví dụ |
|---|---|---|
| **in charge of** | phụ trách | *She is in charge of marketing.* |
| **in addition to** | ngoài ra, thêm vào đó | *In addition to a salary, we offer benefits.* |
| **on behalf of** | thay mặt cho | *I am writing on behalf of the director.* |
| **due to / owing to** | do, vì | *The delay was due to bad weather.* |
| **according to** | theo | *According to the report, sales increased.* |
| **regardless of** | bất kể | *Regardless of experience, all may apply.* |
| **in terms of** | về mặt, xét về | *In terms of quality, this is the best.* |
| **prior to** | trước khi | *Please confirm prior to the meeting.* |
| **as of** | kể từ (ngày/thời điểm) | *As of January 1, the new policy applies.* |
| **instead of** | thay vì | *We used email instead of fax.* |

---

### ⚠️ Các giới từ hay nhầm

- **interested IN** *(not "interested at")*
- **responsible FOR** *(not "responsible of")*
- **apply FOR** a job *(not "apply to")*
- **congratulate someone ON** something *(not "for")*
- **provide someone WITH** something *(not "provide someone of")*
"""
            },

            new()
            {
                Title         = "Ôn tập ngữ pháp",
                Type          = LessonType.Quiz,
                OrderIndex    = 5,
                Duration      = 20,
                IsFree        = true,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow,
                QuizQuestions = BuildGrammarQuiz()
            }
        }
    };

    private static List<QuizQuestion> BuildGrammarQuiz() =>
    [
        Q("The company _____ three new offices since last year.",
            "(A) opens",
            "(B) opened",
            "(C) has opened",
            "(D) had opened",
            "3", "grammar"),

        Q("She _____ the report before the manager asked for it.",
            "(A) submits",
            "(B) has submitted",
            "(C) will submit",
            "(D) had already submitted",
            "4", "grammar"),

        Q("Please reply to this email _____ Friday.",
            "(A) in",
            "(B) on",
            "(C) by",
            "(D) at",
            "3", "grammar"),

        Q("The new policy will take effect _____ January 1st.",
            "(A) in",
            "(B) on",
            "(C) at",
            "(D) by",
            "2", "grammar"),

        Q("We need some _____ about the new product.",
            "(A) informations",
            "(B) an information",
            "(C) information",
            "(D) the informations",
            "3", "grammar"),

        Q("The project was completed _____ than expected.",
            "(A) efficient",
            "(B) efficiently",
            "(C) more efficiently",
            "(D) most efficient",
            "3", "grammar"),

        Q("She is _____ of the marketing department.",
            "(A) in charge",
            "(B) in charge of",
            "(C) charged",
            "(D) responsible",
            "2", "grammar"),

        Q("The store is open _____ 9 AM to 9 PM every day.",
            "(A) from",
            "(B) between",
            "(C) since",
            "(D) during",
            "1", "grammar"),

        Q("_____ the bad weather, the outdoor event was cancelled.",
            "(A) Although",
            "(B) However",
            "(C) Due to",
            "(D) Despite of",
            "3", "grammar"),

        Q("Employees must submit expense reports _____ 30 days of travel.",
            "(A) during",
            "(B) within",
            "(C) for",
            "(D) since",
            "2", "grammar"),
    ];

    // ─────────────────────────────────────────────────────────────
    // SECTION 3: LUYỆN THI TOEIC
    // ─────────────────────────────────────────────────────────────
    private static CourseSection BuildPracticeSection() => new()
    {
        Title      = "Luyện thi TOEIC",
        OrderIndex = 3,
        CreatedAt  = DateTime.UtcNow,
        UpdatedAt  = DateTime.UtcNow,
        Lessons    = new List<CourseLesson>
        {
            new()
            {
                Title      = "Hướng dẫn Part 5 — Incomplete Sentences",
                Type       = LessonType.Text,
                OrderIndex = 1,
                Duration   = 15,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Part 5 — Incomplete Sentences (Điền từ vào câu)

Part 5 có **30 câu hỏi**, mỗi câu cho một câu tiếng Anh có một chỗ trống. Bạn chọn một trong 4 đáp án để điền vào chỗ trống.

---

### Các dạng câu hỏi phổ biến

#### 1. Word Form (Từ loại) — ~40% câu hỏi Part 5

> Yêu cầu chọn **đúng từ loại** (danh từ, động từ, tính từ, trạng từ).

**Chiến lược:**
1. Xác định vị trí của chỗ trống trong câu
2. Phân tích từ xung quanh chỗ trống
3. Chọn từ loại phù hợp

> *The **______** of the new branch is planned for next spring.*
> (A) open (v) &nbsp; (B) openly (adv) &nbsp; (C) **✅ opening** (n) &nbsp; (D) opened (adj)
>
> → Sau mạo từ *The*, cần danh từ → *opening*

---

#### 2. Verb Tense (Thì động từ) — ~20% câu hỏi

> Chọn đúng thì/dạng của động từ.

**Chiến lược:**
- Tìm từ chỉ thời gian (yesterday, since, by the time...)
- Xem mệnh đề phụ (nếu có) để xác định thứ tự thời gian
- Chú ý chủ ngữ để chia đúng số

---

#### 3. Prepositions & Conjunctions (Giới từ & Liên từ) — ~20%

> Chọn đúng giới từ hoặc liên từ.

**Chiến lược:**
- Học các cụm cố định: *responsible for*, *due to*, *in addition to*...
- Phân biệt *although* (mặc dù) vs *because* (vì) vs *so that* (để)

---

#### 4. Vocabulary (Từ vựng) — ~20%

> Chọn từ có nghĩa phù hợp nhất với ngữ cảnh.

**Chiến lược:**
- Đọc toàn câu để hiểu ngữ cảnh
- Loại trừ đáp án không phù hợp về nghĩa
- Chú ý collocations (cụm từ đi kèm)

---

### Mẹo làm bài Part 5 nhanh

> ⏱️ **Mục tiêu:** Làm mỗi câu trong **20-30 giây**

1. Đọc câu, xác định ngay loại câu hỏi
2. Nếu là Word Form: xác định vị trí → chọn từ loại đúng
3. Nếu là Vocabulary: đọc context → chọn nghĩa phù hợp
4. **Không đọc quá kỹ** — Part 5 không cần hiểu ý nghĩa toàn câu

> 💡 **Lưu ý:** Part 5 không có âm thanh, làm ngay khi bắt đầu phần Reading.
"""
            },

            new()
            {
                Title      = "Hướng dẫn Part 6 — Text Completion",
                Type       = LessonType.Text,
                OrderIndex = 2,
                Duration   = 15,
                IsFree     = true,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
                Content    = """
## Part 6 — Text Completion (Điền từ vào đoạn văn)

Part 6 có **4 đoạn văn**, mỗi đoạn có **4 chỗ trống** (tổng 16 câu). Đây là dạng kết hợp giữa Part 5 và Part 7.

---

### Sự khác biệt với Part 5

| | Part 5 | Part 6 |
|---|---|---|
| Đơn vị | Câu riêng lẻ | Đoạn văn (email, letter, memo) |
| Ngữ cảnh | Không cần | **Rất quan trọng** |
| Dạng đặc biệt | Không | Có dạng **câu hoàn chỉnh** |

---

### Dạng câu hỏi đặc biệt: Chèn câu (Sentence Insertion)

> Trong mỗi đoạn Part 6, thường có **1 câu hỏi yêu cầu chọn CẢ CÂU** để điền vào chỗ trống.

**Chiến lược:**
1. Đọc câu trước và câu sau chỗ trống
2. Tìm **từ nối** (transition words): *However, Therefore, In addition, As a result*
3. Chọn câu phù hợp về nghĩa VÀ logic

---

### Cấu trúc đoạn văn thường gặp

1. **Email/Letter thương mại** — Đặt hàng, phàn nàn, xác nhận
2. **Thông báo nội bộ (Memo)** — Thay đổi chính sách, thông báo họp
3. **Quảng cáo** — Tuyển dụng, sản phẩm, dịch vụ
4. **Thông cáo báo chí** — Ra mắt sản phẩm, khai trương

---

### Ví dụ đoạn văn Part 6 (Email)

> *Dear Mr. Johnson,*
>
> *Thank you for your recent order. We are pleased to ______(1) that your shipment has been dispatched.*
>
> *______(2) delays in processing, your order will arrive within 3 business days.*
>
> *(A) inform &nbsp; (B) informing &nbsp; **(C) ✅ confirm** &nbsp; (D) confirmed*
> *(A) Despite &nbsp; (B) However &nbsp; **(C) ✅ Despite any** &nbsp; (D) Although*

---

### Chiến lược làm Part 6

> ⏱️ **Mục tiêu:** Mỗi đoạn trong **2 phút** (tổng 8 phút)

1. **Đọc lướt toàn đoạn** trước — hiểu chủ đề chính
2. Quay lại làm từng câu theo thứ tự
3. Với câu chèn câu: đọc rộng hơn (2 câu trước + 2 câu sau)
4. Kiểm tra tính mạch lạc sau khi điền

> 💡 **Lưu ý quan trọng:** Đáp án Part 6 phụ thuộc vào **ngữ cảnh đoạn văn** — không chỉ dựa vào cấu trúc ngữ pháp.
"""
            },

            new()
            {
                Title         = "Luyện tập Part 5",
                Type          = LessonType.Quiz,
                OrderIndex    = 3,
                Duration      = 20,
                IsFree        = true,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow,
                QuizQuestions = BuildPart5Quiz()
            },

            new()
            {
                Title         = "Luyện tập Part 6",
                Type          = LessonType.Quiz,
                OrderIndex    = 4,
                Duration      = 15,
                IsFree        = false,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow,
                QuizQuestions = BuildPart6Quiz()
            }
        }
    };

    private static List<QuizQuestion> BuildPart5Quiz() =>
    [
        Q("The new _____ of the downtown branch will take place on Saturday.",
            "(A) open",
            "(B) openly",
            "(C) opening",
            "(D) opened",
            "3", "part5"),

        Q("All employees are required to attend the _____ safety training next month.",
            "(A) mandate",
            "(B) mandated",
            "(C) mandatory",
            "(D) mandatorily",
            "3", "part5"),

        Q("The manager _____ the report before the board meeting started.",
            "(A) review",
            "(B) reviewed",
            "(C) reviewing",
            "(D) has reviewed",
            "2", "part5"),

        Q("The new software will be _____ available to all users next week.",
            "(A) full",
            "(B) fully",
            "(C) fullest",
            "(D) fuller",
            "2", "part5"),

        Q("Ms. Garcia is responsible _____ coordinating the annual conference.",
            "(A) of",
            "(B) to",
            "(C) for",
            "(D) with",
            "3", "part5"),

        Q("Please submit your expense report _____ the end of this month.",
            "(A) until",
            "(B) during",
            "(C) while",
            "(D) by",
            "4", "part5"),

        Q("The company has _____ its workforce by 20% over the past two years.",
            "(A) expand",
            "(B) expanding",
            "(C) expanded",
            "(D) expansion",
            "3", "part5"),

        Q("_____ the high cost of materials, the project remained within budget.",
            "(A) Although",
            "(B) Despite",
            "(C) However",
            "(D) Because",
            "2", "part5"),

        Q("The quarterly _____ showed a significant increase in customer satisfaction.",
            "(A) survey",
            "(B) surveys",
            "(C) surveyed",
            "(D) surveying",
            "1", "part5"),

        Q("According to the policy, staff members must _____ their supervisor before taking leave.",
            "(A) notify",
            "(B) notification",
            "(C) notifying",
            "(D) notified",
            "1", "part5"),
    ];

    private static List<QuizQuestion> BuildPart6Quiz() =>
    [
        Q("[Email context] Thank you for contacting us. We are writing to _____ receipt of your application.",
            "(A) confirm",
            "(B) confirmed",
            "(C) confirmation",
            "(D) confirming",
            "1", "part6"),

        Q("[Memo context] _____ the merger is complete, all employees will be notified of any changes to their roles.",
            "(A) When",
            "(B) Although",
            "(C) Despite",
            "(D) However",
            "1", "part6"),

        Q("[Advertisement context] Candidates should have _____ three years of experience in project management.",
            "(A) at most",
            "(B) at least",
            "(C) at best",
            "(D) at once",
            "2", "part6"),

        Q("[Press release context] The new product line has been _____ developed over the past 18 months.",
            "(A) careful",
            "(B) carefully",
            "(C) more careful",
            "(D) carefulness",
            "2", "part6"),

        Q("[Letter context] We apologise for any _____ caused by the delay in processing your order.",
            "(A) inconvenient",
            "(B) inconveniently",
            "(C) inconvenience",
            "(D) inconveniences",
            "3", "part6"),
    ];

    // ─────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────
    private static QuizQuestion Q(
        string question,
        string opt1, string opt2, string opt3, string opt4,
        string correct,
        string type) => new()
    {
        Question   = question,
        Type       = type,
        CreatedAt  = DateTime.UtcNow,
        UpdatedAt  = DateTime.UtcNow,
        Option     = new QuizQuestionOption
        {
            OptionText1 = opt1,
            OptionText2 = opt2,
            OptionText3 = opt3,
            OptionText4 = opt4,
            CorrectOption = correct,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        }
    };
}
