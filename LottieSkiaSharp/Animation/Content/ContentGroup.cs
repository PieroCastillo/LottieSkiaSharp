﻿//   Copyright 2018 yinyue200.com

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using LottieSharp.Animation.Keyframe;
using LottieSharp.Model;
using LottieSharp.Model.Animatable;
using LottieSharp.Model.Content;
using LottieSharp.Model.Layer;
using LottieSharp.Value;
using LottieSharp.Expansion;

namespace LottieSharp.Animation.Content
{
    internal class ContentGroup : IDrawingContent, IPathContent, IKeyPathElement
    {
        private static List<IContent> ContentsFromModels(ILottieDrawable drawable, BaseLayer layer, List<IContentModel> contentModels)
        {
            var contents = new List<IContent>(contentModels.Count);
            for (var i = 0; i < contentModels.Count; i++)
            {
                var content = contentModels[i].ToContent(drawable, layer);
                if (content != null)
                {
                    contents.Add(content);
                }
            }
            return contents;
        }

        internal static AnimatableTransform FindTransform(List<IContentModel> contentModels)
        {
            for (var i = 0; i < contentModels.Count; i++)
            {
                var contentModel = contentModels[i];
                if (contentModel is AnimatableTransform animatableTransform)
                {
                    return animatableTransform;
                }
            }
            return null;
        }

        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();
        private readonly SKPath _path = new SKPath();
        private SKRect _rect;

        private readonly bool _hidden;
        private readonly List<IContent> _contents;
        private List<IPathContent> _pathContents;
        private readonly TransformKeyframeAnimation _transformAnimation;

        internal ContentGroup(ILottieDrawable lottieDrawable, BaseLayer layer, ShapeGroup shapeGroup)
            : this(lottieDrawable, layer, shapeGroup.Name,
                shapeGroup.IsHidden, ContentsFromModels(lottieDrawable, layer, shapeGroup.Items),
                FindTransform(shapeGroup.Items))
        {
        }

        internal ContentGroup(ILottieDrawable lottieDrawable, BaseLayer layer, string name, bool hidden, List<IContent> contents, AnimatableTransform transform)
        {
            Name = name;
            _hidden = hidden;
            _contents = contents;

            if (transform != null)
            {
                _transformAnimation = transform.CreateAnimation();

                _transformAnimation.AddAnimationsToLayer(layer);
                _transformAnimation.ValueChanged += (sender, args) =>
                {
                    lottieDrawable.InvalidateSelf();
                };
            }

            var greedyContents = new List<IGreedyContent>();
            for (var i = contents.Count - 1; i >= 0; i--)
            {
                var content = contents[i];
                if (content is IGreedyContent greedyContent)
                {
                    greedyContents.Add(greedyContent);
                }
            }

            for (var i = greedyContents.Count - 1; i >= 0; i--)
            {
                greedyContents[i].AbsorbContent(_contents);
            }
        }

        public string Name { get; }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            // Do nothing with contents after.
            var myContentsBefore = new List<IContent>(contentsBefore.Count + _contents.Count);
            myContentsBefore.AddRange(contentsBefore);

            for (var i = _contents.Count - 1; i >= 0; i--)
            {
                var content = _contents[i];
                content.SetContents(myContentsBefore, _contents.Take(i + 1).ToList());
                myContentsBefore.Add(content);
            }
        }

        internal List<IPathContent> PathList
        {
            get
            {
                if (_pathContents == null)
                {
                    _pathContents = new List<IPathContent>();
                    for (var i = 0; i < _contents.Count; i++)
                    {
                        if (_contents[i] is IPathContent content)
                        {
                            _pathContents.Add(content);
                        }
                    }
                }
                return _pathContents;
            }
        }

        internal Matrix3X3 TransformationMatrix
        {
            get
            {
                if (_transformAnimation != null)
                {
                    return _transformAnimation.Matrix;
                }
                _matrix.Reset();
                return _matrix;
            }
        }

        public SKPath Path
        {
            get
            {
                // TODO: cache this somehow.
                _matrix.Reset();
                if (_transformAnimation != null)
                {
                    _matrix.Set(_transformAnimation.Matrix);
                }
                _path.Reset();
                if (_hidden)
                {
                    return _path;
                }
                for (var i = _contents.Count - 1; i >= 0; i--)
                {
                    if (_contents[i] is IPathContent pathContent)
                    {
                        var m = _matrix.ToSKMatrix();
                        _path.AddPath(pathContent.Path,ref m);
                    }
                }
                return _path;
            }
        }

        public void Draw(SKCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            if (_hidden)
            {
                return;
            }
            _matrix.Set(parentMatrix);
            byte alpha;
            if (_transformAnimation != null)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
                alpha = (byte)(_transformAnimation.Opacity.Value / 100f * parentAlpha / 255f * 255);
            }
            else
            {
                alpha = parentAlpha;
            }

            for (var i = _contents.Count - 1; i >= 0; i--)
            {
                var drawingContent = _contents[i] as IDrawingContent;
                drawingContent?.Draw(canvas, _matrix, alpha);
            }
        }

        public void GetBounds(out SKRect outBounds, Matrix3X3 parentMatrix)
        {
            outBounds = SKRect.Empty;
            _matrix.Set(parentMatrix);
            if (_transformAnimation != null)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
            }
            RectExt.Set(ref _rect, 0, 0, 0, 0);
            for (var i = _contents.Count - 1; i >= 0; i--)
            {
                if (_contents[i] is IDrawingContent drawingContent)
                {
                    drawingContent.GetBounds(out _rect, _matrix);
                    if (outBounds.IsEmpty)
                    {
                        RectExt.Set(ref outBounds, _rect);
                    }
                    else
                    {
                        RectExt.Set(ref outBounds,
                            Math.Min(outBounds.Left, _rect.Left),
                            Math.Min(outBounds.Top, _rect.Top),
                            Math.Max(outBounds.Right, _rect.Right),
                            Math.Max(outBounds.Bottom, _rect.Bottom));
                    }
                }
            }
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            if (!keyPath.Matches(Name, depth))
            {
                return;
            }

            if (!"__container".Equals(Name))
            {
                currentPartialKeyPath = currentPartialKeyPath.AddKey(Name);

                if (keyPath.FullyResolvesTo(Name, depth))
                {
                    accumulator.Add(currentPartialKeyPath.Resolve(this));
                }
            }

            if (keyPath.PropagateToChildren(Name, depth))
            {
                int newDepth = depth + keyPath.IncrementDepthBy(Name, depth);
                for (int i = 0; i < _contents.Count; i++)
                {
                    var content = _contents[i];
                    if (content is IKeyPathElement element)
                    {
                        element.ResolveKeyPath(keyPath, newDepth, accumulator, currentPartialKeyPath);
                    }
                }
            }
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            _transformAnimation?.ApplyValueCallback(property, callback);
        }
    }
}