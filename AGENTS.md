# LTToeic repository instructions

## TOEIC Listening seed/import tasks

Before creating, changing, importing, or debugging TOEIC Listening seed data:

1. Read `document/LISTENING_TEST_SEEDING.md`.
2. Use schema version 1 and place reusable data in:
   `CoreLTToeic.UI/SeedData/toeic_listening_*.json`.
3. Prefer `ListeningTestSeeder` over ad-hoc inserts.
4. Feed the configured SQL Server database with:

   ```powershell
   dotnet run --project CoreLTToeic.UI/CoreLTToeic.UI.csproj -c Release -- --seed-only
   ```

5. Validate all of the following before reporting completion:
   - Exactly 100 unique questions numbered 1 through 100.
   - Part counts are 6, 25, 39, and 30.
   - Every question has a valid answer A-D.
   - All four Listening Parts have an audio source.
   - Every question can resolve a transcript from either the question or group.
6. Do not delete or replace a test that already has `UserResults`.
7. Keep Listening transcripts hidden while a test is in progress. They may only
   be exposed after submission/completion.
8. Listening-only tests use a maximum score of 495. A blank section scores 0.
9. Preserve source attribution paths in the JSON `source` object.

The complete JSON shape, database mapping, idempotency rules, commands, SQL
verification queries, and handoff checklist are maintained in
`document/LISTENING_TEST_SEEDING.md`.

## TOEIC Reading append/import tasks

Before appending Reading data to an existing Listening test:

1. Read `document/READING_TEST_SEEDING.md`.
2. Use schema version 1 and place reusable data in:
   `CoreLTToeic.UI/SeedData/toeic_reading_*.json`.
3. Use `ReadingTestSeeder`; do not write ad-hoc inserts for the 100 questions.
4. Identify the existing test with `targetTestTitle`.
5. The target must already contain exactly 100 Listening questions. Append
   Reading questions 101 through 200; never replace the existing test.
6. Validate Reading Part counts as 30, 16, and 54 for Parts 5, 6, and 7.
7. Every Reading question must have content, four choices, and an A-D answer.
8. If partial Reading data already exists, stop and investigate instead of
   deleting it, especially when the test has `UserResults`.
9. Feed the configured database with the same `--seed-only` command above and
   verify all seven Parts total exactly 200 questions.

## TOEIC test variant tasks

Before cloning an existing 200-question test into multiple categories:

1. Read `document/TEST_VARIANT_SEEDING.md`.
2. Put schema-version-1 configuration in:
   `CoreLTToeic.UI/SeedData/toeic_variants_*.json`.
3. Use `ToeicTestVariantSeeder`; variants must reference an existing complete
   test through `sourceTestTitle`.
4. Validate the source has 200 questions, all seven Parts, Listening audio and
   transcripts before cloning.
5. Never clone or change `UserResults` and `UserAnswers`.
6. Existing variant titles are idempotent. Never delete or replace a partial
   existing test automatically.
7. After seed, verify category totals and verify every new variant has exactly
   200 questions with Part counts `6/25/39/30/30/16/54`.

## Course catalog seed tasks

Before creating or changing reusable course catalog seed data:

1. Read `document/COURSE_CATALOG_SEEDING.md`.
2. Put schema-version-1 configuration in:
   `CoreLTToeic.UI/SeedData/course_catalog_*.json`.
3. Use `CourseCatalogSeeder` to select and clone sections from the complete
   source course.
4. Never clone or modify enrollments, completions, quiz attempts, reviews, or
   other user learning data.
5. Existing course titles are idempotent. Do not automatically delete or
   replace a partial course.
6. Validate every seeded course has sections and lessons, every quiz has valid
   options, and the Published catalog covers Beginner, Intermediate, Advanced.
