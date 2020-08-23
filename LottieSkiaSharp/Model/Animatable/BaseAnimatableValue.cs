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
using System.Collections.Generic;
using System.Text;
using LottieSharp.Value;
using LottieSharp.Animation.Keyframe;

namespace LottieSharp.Model.Animatable
{
    public abstract class BaseAnimatableValue<TV, TO> : IAnimatableValue<TV, TO>
    {
        internal readonly List<Keyframe<TV>> Keyframes;

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        internal BaseAnimatableValue(TV value) : this(new List<Keyframe<TV>> { new Keyframe<TV>(value) })
        {
        }

        internal BaseAnimatableValue(List<Keyframe<TV>> keyframes)
        {
            Keyframes = keyframes;
        }

        public abstract IBaseKeyframeAnimation<TV, TO> CreateAnimation();

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Keyframes.Count > 0)
            {
                sb.Append("values=").Append("[" + string.Join(",", Keyframes) + "]");
            }
            return sb.ToString();
        }
    }
}