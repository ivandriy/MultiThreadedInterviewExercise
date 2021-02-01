using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectionPool
{
    public class Service : IService
    {
        private readonly BlockingCollection<Request> _requests = new BlockingCollection<Request>();
        private readonly List<Task<int>> _handlerTasks = new List<Task<int>>();

        public Service(int connectionsCount)
        {
            for (var i = 1; i <= connectionsCount; i++)
            {
                _handlerTasks.Add(Task.Run(HandleRequests));
            }
        }
        
        public void sendRequest(Request request)
        {
            _requests.Add(request);
            Console.WriteLine($"{request.Command} request added");
        }

        public void notifyFinishedLoading()
        {
            _requests.CompleteAdding();
        }

        public int getSummary()
        {
            var results = Task.WhenAll(_handlerTasks).Result;
            return results.Sum();
        }

        private async Task<int> HandleRequests()
        {
            var sum = 0;
            while(!_requests.IsCompleted)
            {
                _requests.TryTake(out var request);
                if(request == null) continue;
                using var connection = new Connection();
                var result = await connection.runCommandAsync(request.Command);
                sum += result;
            }

            return sum;
        }
        
    }
}
