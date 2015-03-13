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
    /// <summary>
    /// The <see cref="ConcurrentGraphExecutive"/> can be directed to throttle concurrency either before scheduliung the vertic execution thread, or after scheduling the thread but before 
    /// executing the vertex
    /// </summary>
    public enum ConcurrencyThrottleStrategy
    {
        PreThreadQueue,
        PostThreadQueue
    }
}