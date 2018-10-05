﻿using LottieUWP.Value;
using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace LottieUWP.Animation.Keyframe
{
    internal class PathKeyframeAnimation : KeyframeAnimation<Vector2?>, IDisposable
    {
        private PathKeyframe _pathMeasureKeyframe;
        private SKPathMeasure _pathMeasure;

        internal PathKeyframeAnimation(List<Keyframe<Vector2?>> keyframes)
            : base(keyframes)
        {
        }

        public override Vector2? GetValue(Keyframe<Vector2?> keyframe, float keyframeProgress)
        {
            var pathKeyframe = (PathKeyframe) keyframe;
            var path = pathKeyframe.Path;
            if (path == null || path.IsEmpty)
            {
                return keyframe.StartValue;
            }

            if (ValueCallback != null)
            {
                var value = ValueCallback.GetValueInternal(pathKeyframe.StartFrame.Value, pathKeyframe.EndFrame.Value,
                    pathKeyframe.StartValue, pathKeyframe.EndValue, LinearCurrentKeyframeProgress,
                    keyframeProgress, Progress);
                if (value != null)
                {
                    return value;
                }
            }

            if (_pathMeasureKeyframe != pathKeyframe)
            {
                _pathMeasure?.Dispose();
                _pathMeasure = new SKPathMeasure(path);
                _pathMeasureKeyframe = pathKeyframe;
            }
            _pathMeasure.GetPositionAndTangent(keyframeProgress * _pathMeasure.Length,out var pos, out _);
            return new Vector2(pos.X, pos.Y);
        }

        private void Dispose(bool disposing)
        {
            if (_pathMeasure != null)
            {
                _pathMeasure.Dispose();
                _pathMeasure = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PathKeyframeAnimation()
        {
            Dispose(false);
        }
    }
}