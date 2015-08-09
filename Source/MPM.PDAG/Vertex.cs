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
using System.Linq;
using System.Threading;
using MPM.PDAG.Exceptions;

#endregion

namespace MPM.PDAG
{
    public class Vertex 
    {
		private readonly ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();
        private readonly Action _doWorkAction;
        private readonly ICollection<Vertex> _dependencies = new List<Vertex>();
        private readonly ICollection<Vertex> _dependents = new List<Vertex>();

		public Vertex(Action doWorkAction)
        {
            _doWorkAction = doWorkAction;
        }
        public bool IsDependency(Vertex vertex)
        {
            return Dependencies.Any(d => d == vertex) || (Dependencies.Any(dependency => dependency.IsDependency(vertex)));
        }
    
        public IEnumerable<Vertex> Dependencies
        {
            get { return _dependencies; }
        }

        public Vertex AddDependencies(params Vertex[] dependencies)
        {
			_stateLock.EnterWriteLock();
			try
			{
				foreach (var dependency in dependencies) {
					if (dependency == this)
						throw new CircularDependencyException(dependency);

					CheckForCircularDependency(dependency.Dependencies);

					if (_dependencies.IsDependency(dependency))
						return this;

					_dependencies.Add(dependency);
					dependency._dependents.Add(this);
				}
			}
			finally
			{
				_stateLock.ExitWriteLock();
			}
				
            return this;
        }

        public IEnumerable<Vertex> Dependents
        {
            get { return _dependents; }
        }

        public void RemoveRedundantDependencies()
		{
			var redundants = Dependencies.Where(dependency => Dependencies.FirstOrDefault(d => d != dependency && d.IsDependency(dependency)) != null).ToList();

            foreach (var redundancy in redundants)
				_dependencies.Remove (redundancy);

			foreach (var dependent in Dependencies)
				dependent.RemoveRedundantDependencies ();
		}

        private void CheckForCircularDependency(IEnumerable<Vertex> dependencies)
        {
            dependencies = dependencies.ToList();

            if (dependencies.Any(dependency => dependency == this))
                throw new CircularDependencyException(this);

            foreach (var dependency in dependencies)
                CheckForCircularDependency(dependency.Dependencies);
        }
			
        public void Execute()
        {
			_doWorkAction();
        }
    }
}