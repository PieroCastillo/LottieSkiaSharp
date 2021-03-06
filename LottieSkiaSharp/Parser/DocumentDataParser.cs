﻿using LottieSharp.Model;
using SkiaSharp;

namespace LottieSharp.Parser
{
    public class DocumentDataParser : IValueParser<DocumentData>
    {
        public static readonly DocumentDataParser Instance = new DocumentDataParser();

        public DocumentData Parse(JsonReader reader, float scale)
        {
            string text = null;
            string fontName = null;
            double size = 0;
            int justification = 0;
            int tracking = 0;
            double lineHeight = 0;
            double baselineShift = 0;
            SKColor fillColor = SKColors.Transparent;
            SKColor strokeColor = SKColors.Transparent;
            int strokeWidth = 0;
            bool strokeOverFill = true;

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "t":
                        text = reader.NextString();
                        break;
                    case "f":
                        fontName = reader.NextString();
                        break;
                    case "s":
                        size = reader.NextDouble();
                        break;
                    case "j":
                        justification = reader.NextInt();
                        break;
                    case "tr":
                        tracking = reader.NextInt();
                        break;
                    case "lh":
                        lineHeight = reader.NextDouble();
                        break;
                    case "ls":
                        baselineShift = reader.NextDouble();
                        break;
                    case "fc":
                        fillColor = JsonUtils.JsonToColor(reader);
                        break;
                    case "sc":
                        strokeColor = JsonUtils.JsonToColor(reader);
                        break;
                    case "sw":
                        strokeWidth = reader.NextInt();
                        break;
                    case "of":
                        strokeOverFill = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            return new DocumentData(text, fontName, size, justification, tracking, lineHeight,
                baselineShift, fillColor, strokeColor, strokeWidth, strokeOverFill);
        }
    }
}
