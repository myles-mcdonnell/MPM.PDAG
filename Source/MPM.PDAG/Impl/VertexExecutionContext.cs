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

using System;
using System.Threading;

namespace MPM.PDAG.Impl
{
    public class VertexExecutionContext : IVertexExecutionContext
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Action<string, int> _reportProgressAction;

        internal VertexExecutionContext(CancellationToken cancellationToken, Action<string,int> reportProgressAction)
        {
            _cancellationToken = cancellationToken;
            _reportProgressAction = reportProgressAction;
        }

        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }

        public void ReportProgress(string message = "", int percentageComplete = -1)
        {
            _reportProgressAction(message, percentageComplete);
        }
    }
}
