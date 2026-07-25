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
