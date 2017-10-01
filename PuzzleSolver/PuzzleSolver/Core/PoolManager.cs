using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace PuzzleSolver.Core
{
    public class PoolManager<T> where T : class {
        ConcurrentBag<T> Pool;
        Func<T, T> Initializer;
        Func<T> NewCreator;

        public PoolManager(Func<T,T> Initializer, Func<T> NewCreator) {
            Pool = new ConcurrentBag<T>();
            this.Initializer = Initializer;
            this.NewCreator = NewCreator;
        }

        public void ChangeMethods(Func<T, T> Initializer, Func<T> NewCreator) {
			this.Initializer = Initializer;
			this.NewCreator = NewCreator;
        }

        public T Get() {
            T result;
            if (Pool.TryTake(out result))
            {
                result = Initializer(result);
            }
            else
            {
                result = NewCreator();
            }
            return result;
        }

        public void Return(T t) {
            Pool.Add(t);
        }
    }
}
