using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.CognitiveServices.Speech;

namespace VoicerStudio.Application.Models;

public class SpeechGenerateRequest
{
    /// <summary>
    /// The language that you want the voice to speak.
    /// </summary>
    public string Locale { get; set; }

    /// <summary>
    /// Voice short name.
    /// </summary>
    public string Voice { get; set; }

    /// <summary>
    /// Text to speech.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The voice-specific speaking style.
    /// You can express emotions like cheerfulness, empathy, and calm.
    /// You can also optimize the voice for different scenarios like customer service, newscast, and voice assistant.
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// The intensity of the speaking style.
    /// You can specify a stronger or softer style to make the speech more expressive or subdued.
    /// The range of accepted values are: 0.01 to 2 inclusive.
    /// The default value is 1, which means the predefined style intensity.
    /// The minimum unit is 0.01, which results in a slight tendency for the target style.
    /// A value of 2 results in a doubling of the default style intensity.
    /// </summary>
    public double? StyleDegree { get; set; } = 1f;

    /// <summary>
    /// The speaking role-play.
    /// The voice can imitate a different age and gender, but the voice name isn't changed.
    /// For example, a male voice can raise the pitch and change the intonation to imitate a female voice,
    /// but the voice name won't be changed.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Indicates the baseline pitch for the text.
    /// Pitch changes can be applied at the sentence level.
    /// The pitch changes should be within 0.5 to 1.5 times the original audio.
    /// </summary>
    public double? Pitch { get; set; }

    /// <summary>
    /// Indicates the volume level of the speaking voice. (%)
    /// </summary>
    public double? Volume { get; set; }

    /// <summary>
    /// Indicates the speaking rate (speed) of the text.
    /// Speaking rate can be applied at the word or sentence level.
    /// The rate changes should be within 0.5 to 2 times the original audio.
    /// </summary>
    public double? Speed { get; set; }

    public TimeSpan? Start { get; set; }
    public TimeSpan? End { get; set; }

    [JsonIgnore] public TimeSpan? Duration => End - Start;

    public SpeechSynthesisOutputFormat Format { get; set; } = SpeechSynthesisOutputFormat.Riff48Khz16BitMonoPcm;
}

public class SpeechGenerateRequestValidator : AbstractValidator<SpeechGenerateRequest>
{
    public SpeechGenerateRequestValidator()
    {
        RuleFor(o => o.Locale).NotEmpty();
        RuleFor(o => o.Voice).NotEmpty();
        RuleFor(o => o.Text).NotEmpty();
        RuleFor(o => o.Style);
        When(o => !string.IsNullOrEmpty(o.Style), () =>
        {
            RuleFor(o => o.StyleDegree).GreaterThan(0).LessThanOrEqualTo(2.0);
        });
        RuleFor(o => o.Pitch).GreaterThanOrEqualTo(-0.5).LessThanOrEqualTo(0.5);
        RuleFor(o => o.Volume).GreaterThan(-1.0).LessThanOrEqualTo(1.0);

        When(o => o.Speed is null, () =>
        {
            RuleFor(o => o.End).NotNull();
            RuleFor(o => o.Start).NotNull().LessThan(o => o.End);
        }).Otherwise(() =>
        {
            RuleFor(o => o.Speed).GreaterThanOrEqualTo(-0.5).LessThanOrEqualTo(1.0);
        });
    }
}