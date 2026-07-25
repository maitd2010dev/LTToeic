SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE ListeningScoreConversions
SET Score = 0
WHERE Correct = 0;

UPDATE ReadingScoreConversions
SET Score = 0
WHERE Correct = 0;

-- Correct results saved before the zero-score rule was fixed.
UPDATE UserResults
SET ListeningScore =
        CASE
            WHEN TotalListeningQuestions = 0 OR ListeningCorrects = 0 THEN 0
            ELSE ListeningScore
        END,
    ReadingScore =
        CASE
            WHEN TotalReadingQuestions = 0 OR ReadingCorrects = 0 THEN 0
            ELSE ReadingScore
        END,
    TotalScore =
        CASE
            WHEN TotalListeningQuestions = 0 OR ListeningCorrects = 0 THEN 0
            ELSE ListeningScore
        END
        +
        CASE
            WHEN TotalReadingQuestions = 0 OR ReadingCorrects = 0 THEN 0
            ELSE ReadingScore
        END;

COMMIT TRANSACTION;
