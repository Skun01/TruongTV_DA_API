using System.Text.Json;
using Application.DTOs.ExamSessions;

namespace Application.Helper;

public static class JlptAiAnalysisPromptHelper
{
    public const string PromptVersion = "jlpt-ai-analysis-v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string GetSystemPrompt()
    {
        return """
            You are a JLPT learning analyst. Analyze a submitted JLPT mock exam for a Vietnamese learner of Japanese.
            Return only valid JSON matching the provided schema. Do not include markdown.
            Use Vietnamese for learner-facing text. Keep advice concrete and tied to evidence from questions and section scores.
            Do not invent question ids, scores, options, or facts not present in input.
            """;
    }

    public static string BuildUserPrompt(JlptAiAnalysisInput input)
    {
        var payloadJson = JsonSerializer.Serialize(input, JsonOptions);

        return $$"""
            Analyze the JLPT exam result payload below.

            Learner-facing text requirements:
            - Use Vietnamese.
            - Be concrete and evidence-based.
            - Keep strings concise enough for UI rendering.
            - `questionInsights` should focus on incorrect or unanswered questions only.

            Return JSON with exactly this shape:
            {
              "summary": {
                "headline": "string",
                "overallBand": "Excellent|Good|NeedsPractice|Weak",
                "scorePercent": 0,
                "passed": false,
                "estimatedLevelReadiness": "Ready|Borderline|NotReady"
              },
              "sectionAnalyses": [
                {
                  "sectionType": "Moji|Bunpou|Dokkai|Choukai",
                  "score": 0,
                  "maxScore": 0,
                  "passScore": 0,
                  "isPassed": false,
                  "performanceBand": "Strong|Stable|Weak|Critical",
                  "diagnosis": "string",
                  "strengths": ["string"],
                  "weaknesses": ["string"],
                  "recommendedFocus": ["string"]
                }
              ],
              "mistakePatterns": [
                {
                  "patternId": "string",
                  "title": "string",
                  "severity": "Low|Medium|High",
                  "sectionTypes": ["Moji|Bunpou|Dokkai|Choukai"],
                  "questionIds": ["string"],
                  "evidence": "string",
                  "advice": "string"
                }
              ],
              "questionInsights": [
                {
                  "questionId": "string",
                  "sectionType": "Moji|Bunpou|Dokkai|Choukai",
                  "isCorrect": false,
                  "selectedOptionId": "string|null",
                  "correctOptionId": "string",
                  "rootCause": "string",
                  "explanation": "string",
                  "reviewTags": ["string"]
                }
              ],
              "recommendations": [
                {
                  "type": "ReviewWrongQuestions|ReviewSection|StudyVocabulary|StudyGrammar|PracticeReading|PracticeListening|RetakeExam",
                  "priority": "Low|Medium|High",
                  "title": "string",
                  "reason": "string",
                  "estimatedMinutes": 15,
                  "targetRoute": "string|null",
                  "targetIds": ["string"]
                }
              ],
              "nextActions": [
                {
                  "label": "string",
                  "actionType": "ReviewWrongQuestions|StartRemedialSession|RetakeExam|BackToExamList",
                  "targetRoute": "string|null"
                }
              ]
            }

            Exam result payload:
            {{payloadJson}}
            """;
    }

    public static string BuildRepairPrompt(JlptAiAnalysisInput input, string invalidJson)
    {
        var payloadJson = JsonSerializer.Serialize(input, JsonOptions);

        return $$"""
            Your previous response was invalid for the required JSON schema or contained invalid references.
            Repair it and return only valid JSON. Do not include markdown or explanations.
            Keep Vietnamese learner-facing text.
            Do not invent question ids, section types, scores, or facts outside the input.

            Source exam result payload:
            {{payloadJson}}

            Previous invalid response:
            {{invalidJson}}
            """;
    }
}
