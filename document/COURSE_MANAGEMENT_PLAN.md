# Course Management Implementation Plan

## Implementation Status and Session Handoff

**Last updated:** 2026-07-25

### Completed

- Created this implementation plan and checked the course-related schema in `document/feature/ClassForCourseManagement.docx`.
- Retained the existing course entities and added:
  - `QuizAttempt`.
  - `QuizAttemptAnswer`.
- Added EF Core model rules for:
  - One enrollment per user/course.
  - One completion per user/lesson.
  - One review per user/course.
  - One attempt number per user/quiz lesson.
- Generated and applied migration:
  - `20260725090649_AddCourseLearningAttempts`.
  - The configured database now contains `QuizAttempts` and `QuizAttemptAnswers`.
- Extended administrator services and repositories with:
  - Course, section, lesson, and quiz-question management.
  - Section and lesson move-up/move-down ordering.
  - Publication validation.
  - Enrollment activation/cancellation and progress inspection.
  - Quiz-attempt history.
  - Review inspection and deletion.
  - Hard-deletion protection when learner progress exists.
- Added the separate learner-facing `ICourseLearningService` and repository with:
  - Published-course catalog and course details.
  - Idempotent free enrollment.
  - Enrolled-course listing and progress.
  - Free-preview access for authenticated users.
  - Explicit text/video lesson completion.
  - Server-side quiz grading.
  - Persistent attempt and question-level answer history.
  - Course-completion recalculation.
  - Create/update review behavior.
- Added administrator pages:
  - `/admin/quan-li-khoa-hoc`.
  - `/admin/quan-li-khoa-hoc/{id}` with General, Curriculum, Students, and Reviews views.
- Added learner pages:
  - `/khoa-hoc`.
  - `/khoa-hoc/{id}`.
  - `/khoa-hoc-cua-toi`.
  - `/khoa-hoc/{id}/hoc`.
- Enabled the `Khóa học` navigation link.
- Removed Price from the new Course interfaces and force `Price = 0` in application logic while retaining the database column.
- Added a project-local, HTML-encoded Markdown renderer supporting headings, lists, tables, HTTP/HTTPS links, and emphasis without adding a package.
- Verified that normal learner course-detail responses do not contain correct quiz answers. Correct answers are returned only after submission or in the current learner's saved attempt history.
- Built the solution successfully with no warnings introduced by Course Management.
- Generated and inspected both migration up and down SQL scripts.
- Fixed the runtime `Invalid object name 'QuizAttempts'` error by applying the pending migration.

### Files and Areas Added

- Domain:
  - `CoreLTToeic.Domain/Entities/QuizAttempt.cs`.
  - `CoreLTToeic.Domain/Entities/QuizAttemptAnswer.cs`.
- Application:
  - `CourseLearningService`.
  - `ICourseLearningService`.
  - `ICourseLearningRepository`.
  - Course learning, enrollment-admin, quiz-question, and attempt view/edit models.
- Infrastructure:
  - `CourseLearningRepository`.
  - Extended `CourseRepository`.
  - Updated `AppDbContext`.
  - Migration `20260725090649_AddCourseLearningAttempts`.
- UI:
  - `Components/Pages/Admin/CourseEditor.razor`.
  - `Components/Pages/Course/`.
  - `Helpers/CourseMarkdownRenderer.cs`.

### Constraints Preserved

- No Microsoft/.NET framework or core-library version was changed.
- No package reference was added or upgraded.
- No third-party Markdown package was added.
- The existing database `Price` column was retained.
- The existing untracked `.github/` directory and source DOCX were not modified.

### Remaining Validation Work

The implementation is compiled and the configured database is migrated, but the complete acceptance matrix below has not yet been executed end to end:

- Admin CRUD, ordering, publication, enrollment, progress, quiz-attempt, and review scenarios.
- Learner catalog, enrollment, access control, completion, quiz history, course completion, and review scenarios.
- Concurrent duplicate enrollment, completion, and review attempts.
- Anonymous, learner, and administrator authorization checks.
- Migration down/up testing against a separate disposable database.
- Browser-based visual and responsive checks for all new pages.

`FEATURE_TRACKER.md` has intentionally not been updated yet. Update it only after all remaining acceptance scenarios pass.

### Suggested Prompt for a New Session

> Continue Course Management from `document/COURSE_MANAGEMENT_PLAN.md`. Read the Implementation Status and Session Handoff section first. Do not change Microsoft/.NET versions or add packages. The migration `20260725090649_AddCourseLearningAttempts` is already applied to the configured database. Run the remaining acceptance tests, fix any Course Management defects, and update `FEATURE_TRACKER.md` only after every listed scenario passes. Notify me before any further migration, database, project configuration, or environment change.

## Summary

Implement complete Course Management for administrators and learners based on the existing entities and the supplied schema in `document/feature/ClassForCourseManagement.docx`.

Success means administrators can manage the full curriculum and learner activity, while users can enroll, learn, complete lessons, submit quizzes, track progress, and review courses.

## Data and Service Changes

- Retain the existing Course, Section, Lesson, Enrollment, Completion, Review, QuizQuestion, and QuizQuestionOption models.
- Add:
  - `QuizAttempt`: user, lesson, attempt number, totals, score, submission time.
  - `QuizAttemptAnswer`: attempt, question snapshot, selected option, correct option, correctness.
- Add database uniqueness rules for:
  - One enrollment per user/course.
  - One completion per user/lesson.
  - One review per user/course.
  - One attempt number per user/quiz lesson.
- Add a new EF Core migration for attempt tables and indexes. Notify the user and obtain approval before generating or applying it.
- Keep the existing admin `ICourseService`; extend it with section/lesson update, ordering, quiz CRUD, enrollment administration, progress inspection, attempt history, and review deletion.
- Add a separate learner-facing service for published-course queries, enrollment, learning access, completion, quiz submission/history, and review submission.
- Never expose correct quiz answers in normal course-detail responses; grade submissions on the server.

## Administrator Experience

- Keep `/admin/quan-li-khoa-hoc` as the searchable course list with status and level filters.
- Add a dedicated course editor with tabs:
  - General information and publication status.
  - Curriculum.
  - Students and progress.
  - Reviews.
- Curriculum management must support:
  - Create, edit, delete, and move sections up/down.
  - Create, edit, delete, and move lessons up/down.
  - Type-specific lesson fields for Text, Video, and Quiz.
  - Quiz question CRUD with three or four options and one correct answer.
- Publication validation:
  - Course requires title and description.
  - Published course requires at least one section and lesson.
  - Every section requires a title.
  - Video lessons require a valid HTTP/HTTPS video URL.
  - Quiz lessons require at least one fully configured question.
- Student management shows enrollment status, progress, completed lessons, quiz attempts, and scores. Admin can activate or cancel enrollment.
- Review management supports viewing and deleting inappropriate reviews.
- Block hard deletion of courses or lessons with enrollment/progress records; administrators must unpublish the course instead.

## Learner Experience

- Enable the `Khóa học` navigation link.
- Add:
  - `/khoa-hoc`: published-course catalog with search and level filtering.
  - `/khoa-hoc/{id}`: course description, objectives, curriculum, reviews, and enrollment.
  - `/khoa-hoc-cua-toi`: enrolled courses and progress.
  - `/khoa-hoc/{id}/hoc`: text/video/quiz lesson player.
- All enrollments are free:
  - Hide the Price field from admin and user interfaces.
  - Save `Price = 0` while retaining the database column for future use.
  - Enrollment is authenticated and idempotent.
- Free-preview lessons may be opened by logged-in users before enrollment; progress is only saved after enrollment.
- Text and video lessons use an explicit “Complete lesson” action.
- Quiz behavior:
  - Require every question to be answered.
  - Save every submission and its question-level answers.
  - Any submitted attempt completes the quiz lesson, regardless of score.
  - Show score, corrections, and previous attempts.
- Mark the enrollment `Completed` when every current lesson is completed. Recalculate it if the curriculum later changes.
- Enrolled users can create or update one 1–5-star review.
- Render the existing Markdown with a simple project-local, HTML-encoded renderer supporting headings, lists, tables, links, and emphasis. Do not add a package.

## Validation and Configuration Rules

- Do not modify Microsoft/.NET core libraries or framework versions.
- Do not add third-party packages.
- Notify the user before any migration, project configuration, database, or environment change.
- Validate with:
  - Clean solution build with no new warnings.
  - Migration up/down test against a disposable database.
  - Admin CRUD, ordering, publication, enrollment, progress, attempt, and review scenarios.
  - User catalog, enrollment, access control, lesson completion, quiz history, course completion, and review scenarios.
  - Duplicate enrollment/completion/review concurrency checks.
  - Authorization checks for anonymous users, learners, and administrators.
- Update `FEATURE_TRACKER.md` only after every Course acceptance scenario passes.
