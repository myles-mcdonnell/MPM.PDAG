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

#region Using

using System;
using System.Collections.Generic;

#endregion

namespace MPM.PDAG
{
    /// <summary>
    ///   Complex dependency structures are expressed by creating <seealso cref = "IVertex" />s and the dependencies between them.
    /// </summary>
    public interface IVertex
    {
        event EventHandler<EventArgs<IVertex>> OnStarted;
        event EventHandler<EventArgs<IVertex>> OnCompleted;
        event EventHandler<EventArgs<Tuple<IVertex, Exception>>> OnFailed;
        event EventHandler<VertexProgressEventArgs> OnProgress;
        event EventHandler<EventArgs<IVertex>> OnRequiresReExecution;

        IEnumerable<IVertex> Dependencies { get; }
        IEnumerable<IVertex> Dependents { get; }

        object Tag { get; set; }
        string Name { get; set; }
        bool Enabled { get; set; }
        
        VertexState State { get; }
        ExecutionResult LastExecutionResult { get; }

        void Execute(IGraphExecutive graphExecutive);
        void Cancel();
        
        void RemoveRedundantDependencies();
        event EventHandler<EventArgs<IVertex>> OnCancelled;

        IDirectedAcyclicGraph GetGraph();
        bool IsDependency(IVertex vertex);
        bool IsDependent(IVertex vertex);
    }
}