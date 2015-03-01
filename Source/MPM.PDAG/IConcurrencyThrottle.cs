#region HEADER
//   Copyright 2015 Myles McDonnell (mcdonnell.myles@gmail.com)

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

namespace MPM.PDAG
{
    public interface IConcurrencyThrottle
    {
        /// <summary>
        /// Calling enter will cause thread to wait until bandwidth is available
        /// </summary>
        void Enter();

        /// <summary>
        /// Calling exit signals that a throttled operation is complete and will in turn release a waiting thread if one exists
        /// </summary>
        void Exit();
        
        /// <summary>
        /// Enables/disables throttle
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// The maximum number of concurrent operations
        /// </summary>
        int MaxValue { get; set; }

        /// <summary>
        /// The current count of concurrent operations
        /// </summary>
        int CurrentValue { get; }
    }
}
