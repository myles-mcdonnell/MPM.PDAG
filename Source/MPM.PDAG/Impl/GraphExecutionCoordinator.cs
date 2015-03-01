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
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MPM.PDAG.Impl;

namespace MPM.PDAG
{
    public class GraphExecutionCoordinator : IGraphExecutionCoordinator
    {
        public event EventHandler<EventArgs<IGraphExecutive>> OnGraphExecutiveStarted;
        
        private readonly IDirectedAcyclicGraph _directedAcyclicGraph;
        //private readonly IDirectedAcyclicGraphFactory _directedAcyclicGraphFactory;
        private readonly IGraphExecutiveFactory _graphExecutiveFactory;
        private readonly IDictionary<IVertex, DateTime> _pendingReExecution = new Dictionary<IVertex, DateTime>();
        private readonly object _lock = new object();
        private readonly Timer _timer = new Timer(1000);

        public GraphExecutionCoordinator(IDirectedAcyclicGraph directedAcyclicGraph, 
            IGraphExecutiveFactory graphExecutiveFactory = null)
        {
            AutoExecutionDelay = TimeSpan.MinValue;
            _directedAcyclicGraph = directedAcyclicGraph;
            //_directedAcyclicGraphFactory = directedAcyclicGraphFactory??new DirectedAcyclicGraphFactory();
            _graphExecutiveFactory = graphExecutiveFactory??new GraphExecutiveFactory();

            _timer.Elapsed += _timer_Elapsed;

            foreach (var vertex in _directedAcyclicGraph.AllVertices)
                vertex.OnRequiresReExecution += vertex_OnRequiresReExecution;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                foreach (var vertex in _pendingReExecution.Keys.Where(vertex => now - _pendingReExecution[vertex] > AutoExecutionDelay).ToArray())
                {
                    _pendingReExecution.Remove(vertex);
                    Execute(vertex);
                }
            }
        }

        void vertex_OnRequiresReExecution(object sender, EventArgs<IVertex> e)
        {
            lock(_lock)
            {
                if (AutoExecutionDelay==TimeSpan.MinValue)
                    Execute(e.Argument);
                else if (_pendingReExecution.ContainsKey(e.Argument))
                    _pendingReExecution[e.Argument] = DateTime.UtcNow;
                else if (!_pendingReExecution.Keys.Any(v => v.IsDependency(e.Argument)))
                {
                    foreach (var dependency in _pendingReExecution.Keys.Where(v => v.IsDependent(e.Argument)).ToList())
                        _pendingReExecution.Remove(dependency);

                    _pendingReExecution.Add(e.Argument, DateTime.UtcNow);
                }
            }
        }

        public bool Enabled
        {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }

        public TimeSpan AutoExecutionDelay { get; set; }

        private void Execute(IVertex vertex)
        {
            ExecuteGraph(vertex.GetGraph());
        }

        private void ExecuteGraph(IDirectedAcyclicGraph graph)
        {
            var graphExecutive = _graphExecutiveFactory.Build(graph);

            graphExecutive.Run();

            if (OnGraphExecutiveStarted!=null)
                OnGraphExecutiveStarted(this, new EventArgs<IGraphExecutive>(graphExecutive));
        }
    }
}
