//   Copyright 2012 Myles McDonnell (myles.mcdonnell.public@gmail.com)

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;

namespace MPM.PDAG.UnitTests
{
    public class MaxRegister
    {
        public int MaxValue { get; private set; }
        public int CurrentValue { get; private set; }

        private readonly object _lock = new object();

        public void Increment()
        {
            lock (_lock)
            {
                MaxValue = Math.Max(++CurrentValue, MaxValue);
            }
        }

        public void Decrement()
        {
            lock (_lock)
            {
                CurrentValue--;
            }
        }
    }
}
