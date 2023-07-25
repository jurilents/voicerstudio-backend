using System.Globalization;
using System.Text;
using System.Xml;
using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.CognitiveServices.Azure;

public class SsmlBuilder
{
    public static string BuildSsml(SpeechGenerateRequest request)
    {
        const string speakNamespace = "https://www.w3.org/2001/10/synthesis";
        const string msttsNamespace = "https://www.w3.org/2001/mstts";

        var xmlSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates
        };
        var stringBuilder = new StringBuilder();
        using var xml = XmlWriter.Create(stringBuilder, xmlSettings);

        xml.WriteStartElement(null, "speak", speakNamespace);
        xml.WriteAttributeString("version", "1.0");
        xml.WriteAttributeString("xml", "lang", null, request.Locale);
        {
            xml.WriteStartElement("voice");
            xml.WriteAttributeString("name", request.Voice);
            // if (!string.IsNullOrEmpty(request.Effect))
            // xml.WriteAttributeString("effect", request.Effect);
            {
                xml.WriteStartElement("mstts", "silence", msttsNamespace);
                {
                    xml.WriteAttributeString("type", "Tailing-exact");
                    xml.WriteAttributeString("value", "0ms");
                }
                xml.WriteEndElement();

                if (request.Duration.HasValue)
                {
                    xml.WriteStartElement("mstts", "audioduration", msttsNamespace);
                    xml.WriteAttributeString("value", $"{request.Duration.Value.TotalMilliseconds}ms");
                    xml.WriteEndElement();
                }

                if (string.IsNullOrEmpty(request.Style))
                {
                    ApplyGlobalProsodies(xml, request);
                }
                else
                {
                    xml.WriteStartElement("mstts", "express-as", msttsNamespace);
                    xml.WriteAttributeString("style", request.Style);
                    xml.WriteAttributeString("styledegree", $"{(request.StyleDegree ?? 1f).ToString(CultureInfo.InvariantCulture)}");
                    if (!string.IsNullOrEmpty(request.Role)) xml.WriteAttributeString("role", request.Role);
                    {
                        ApplyGlobalProsodies(xml, request);
                    }
                    xml.WriteEndElement();
                }
            }
            xml.WriteEndElement();
        }
        xml.WriteEndElement();
        xml.Flush();

        var result = stringBuilder.ToString();
        return result;
    }

    private static void ApplyGlobalProsodies(XmlWriter xml, SpeechGenerateRequest request)
    {
        if (request.Speed is null or 0
            && request.Volume is null or 0
            && request.Pitch is null or 0)
        {
            xml.WriteString(request.Text);
            return;
        }

        // At least one of those params must be not null
        // otherwise Azure will return the empty audio
        xml.WriteStartElement("prosody");
        if (HasChange(request.Speed)) xml.WriteAttributeString("rate", DeltaToPercents(request.Speed));
        if (HasChange(request.Volume)) xml.WriteAttributeString("volume", DeltaToPercents(request.Volume));
        if (HasChange(request.Pitch)) xml.WriteAttributeString("pitch", DeltaToPercents(request.Pitch));
        {
            xml.WriteString(request.Text);
        }
        xml.WriteEndElement();
    }

    private static bool HasChange(double? num) => num.HasValue && Math.Abs(num.Value) > 0.01;

    private static string DeltaToPercents(double? num)
    {
        var percents = (num!.Value * 100.0).ToString(CultureInfo.InvariantCulture);
        return num >= 0 ? $"+{percents}%" : $"{percents}%";
    }
}