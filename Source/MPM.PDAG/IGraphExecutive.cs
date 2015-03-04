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
    /// Manages the parallel execution of a <see cref="IDirectedAcyclicGraph"/> and provides methods to <see cref="Pause"/>, <see cref="Resume"/> and <see cref="Cancel"/> excution.
    /// </summary>
    public interface IGraphExecutive
    {
        /// <summary>
        /// Raised on successful graph execution
        /// </summary>
        event EventHandler<EventArgs<TimeSpan>> OnFinished;

        /// <summary>
        /// Raised on graph execution cancellation
        /// </summary>
        event EventHandler OnCancelled;
        
        /// <summary>
        /// Raised on graph execution start
        /// </summary>
        event EventHandler OnStarted;

        /// <summary>
        /// Raised on graph execution resumed
        /// </summary>
        event EventHandler OnResumed;

        /// <summary>
        /// Raised on graph execution paused
        /// </summary>
        event EventHandler OnPaused;

        /// <summary>
        /// Raised on graph execution paused pending
        /// </summary>
        event EventHandler OnPausePending;

        /// <summary>
        /// Raised on graph execution cancel pending
        /// </summary>
        event EventHandler OnCancelPending;

        /// <summary>
        /// Raised on graph execution resume pending
        /// </summary>
        event EventHandler OnResumePending;

        /// <summary>
        /// Raised on graph execution failed
        /// </summary>
        event EventHandler<EventArgs<Tuple<TimeSpan, Exception>>> OnFailed;

        /// <summary>
        /// Raised each time am <see cref="IVertex"/> repors progress
        /// </summary>
        //event EventHandler<VertexProgressEventArgs> OnVertexProgress;

        /// <summary>
        /// Starts <see cref="IDirectedAcyclicGraph"/> asynchronously
        /// </summary>
        void Run();

        /// <summary>
        /// Pauses execution
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes execution when paused
        /// </summary>
        void Resume();

        /// <summary>
        /// Cancels execution
        /// </summary>
        void Cancel();

        /// <summary>
        /// Current execution state
        /// </summary>
       // GraphExecutiveState State { get; }

        /// <summary>
        /// The graph being managed buy this executive
        /// </summary>
        IDirectedAcyclicGraph Graph { get; }

        /// <summary>
        /// The result of the last execution 
        /// </summary>
        

        /// <summary>
        /// Returns true if graph execution is running, false if cancelled or waits if paused/throttled
        /// </summary>
        /// <returns></returns>
        bool CanVertexExecutionProceed();

        /// <summary>
        /// Called by vertex shortly before execution end, regardless of result
        /// </summary>
        void SignalVertexExecutionEnd();
    }
}