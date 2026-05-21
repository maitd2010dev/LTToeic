using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Common.Constants;

public static class ToeicPartHelper
{
    public static string GetPartLabel(ToeicLRPart part) => part switch
    {
        ToeicLRPart.Part1 => "Photographs",
        ToeicLRPart.Part2 => "Question-Response",
        ToeicLRPart.Part3 => "Conversations",
        ToeicLRPart.Part4 => "Talks",
        ToeicLRPart.Part5 => "Incomplete Sentences",
        ToeicLRPart.Part6 => "Text Completion",
        ToeicLRPart.Part7 => "Reading Passages",
        _ => $"Part {(int)part}"
    };

    public static string GetPartLabel(int partNum) =>
        Enum.IsDefined(typeof(ToeicLRPart), partNum)
            ? GetPartLabel((ToeicLRPart)partNum)
            : $"Part {partNum}";
}
