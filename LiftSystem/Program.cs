using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace LiftSystem
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            const int numberOfFloors = 8;
            const int numberOfLifts = 2;

            // initialise lifts
            var tokenSource = new CancellationTokenSource();
            var lifts = new List<Lift>(numberOfLifts);

            var random = new Random(DateTime.Now.Millisecond);
            Parallel.For(0, numberOfLifts,
                (int i) => { lifts.Add(new Lift(numberOfLifts, numberOfFloors, random)); });

            var tasks = new List<Task>(numberOfLifts);
            Parallel.ForEach(lifts, (lift) => tasks.Add(lift.DoWorkAsync(tokenSource.Token)));

            // initialise controller
            var controller = new LiftController(lifts, numberOfFloors);

            while (!tokenSource.IsCancellationRequested)
            {
                Console.WriteLine("C to cancel");
                var input = Console.ReadLine();
                if (input != "C")
                {
                    if (int.TryParse(input, out var floorNumber))
                    {
                        controller.SummonLift(floorNumber);
                    }

                    continue;
                }

                tokenSource.Cancel();
                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}