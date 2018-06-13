using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LiftSystem
{
    internal sealed class Lift : ILift
    {
        public enum State
        {
            Idle,
            GoingUp,
            GoingDown
        }

        private int _currentFloor;
        private State _currentState;

        private readonly ConcurrentDictionary<int, bool> _floors;
        private readonly ConcurrentQueue<int> _summonQueue;
        private int? _destFloorMax;
        private int? _destFloorMin;
        private int _reset;

        private readonly Random _random;

        public Lift(int numberOfLifts, int numberOfFloors, Random random)
        {
            _floors = new ConcurrentDictionary<int, bool>(numberOfLifts, numberOfFloors);
            _summonQueue = new ConcurrentQueue<int>();
            _currentFloor = 0;
            _currentState = State.Idle;
            _random = random;
        }

        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => DoWork(cancellationToken), cancellationToken);
        }

        public void DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ProcessSummonQueue();
                MoveToNextFloor();
            }
        }

        public void SummonToFloor(int floor)
        {
            _summonQueue.Enqueue(floor);
        }

        private void ProcessSummonQueue()
        {
            while (_summonQueue.TryDequeue(out var floor))
            {
                _floors.AddOrUpdate(floor, true, (i, b) => true);

                if (floor > _currentFloor)
                {
                    if (!_destFloorMax.HasValue)
                    {
                        _destFloorMax = floor;
                        return;
                    }

                    _destFloorMax = floor > _destFloorMax ? floor : _destFloorMax;
                }
                else if(floor < _currentFloor) 
                {
                    if (!_destFloorMin.HasValue)
                    {
                        _destFloorMin = floor;
                        return;
                    }

                    _destFloorMin = floor < _destFloorMin ? floor : _destFloorMin;
                }
            }
        }

        private void MoveToNextFloor()
        {
            UpdateState();

            switch (_currentState)
            {
                case State.GoingUp:
                    _currentFloor++;
                    Console.WriteLine(
                        $"{Thread.CurrentThread.ManagedThreadId} : Moved up to floor {_currentFloor}");
                    break;
                case State.GoingDown:
                    _currentFloor--;
                    Console.WriteLine(
                        $"{Thread.CurrentThread.ManagedThreadId} : Moved down to floor {_currentFloor}");
                    break;
            }

            // open doors if summoned
            if (_floors.ContainsKey(_currentFloor) && _floors[_currentFloor])
            {
                _floors[_currentFloor] = false;
            }

            if (_currentFloor == _destFloorMax)
            {
                _destFloorMax = null;
                _currentState = State.Idle;
            }

            if (_currentFloor == _destFloorMin)
            {
                _destFloorMin = null;
                _currentState = State.Idle;
            }

            Thread.Sleep(1000);
        }

        private void UpdateState()
        {
            if (_currentState == State.Idle)
            {
                if (_destFloorMax.HasValue && _destFloorMin.HasValue)
                {
                    // tied so randomly select a direction to go
                    _currentState = _random.Next(0, 1) == 1 ? State.GoingUp : State.GoingDown;
                }
                else if (_destFloorMax.HasValue)
                {
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : Going up");
                    _currentState = State.GoingUp;
                }
                else if (_destFloorMin.HasValue)
                {
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : Going down");
                    _currentState = State.GoingDown;
                }
                else
                {
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : Idling floor number {_currentFloor}");
                    _currentState = State.Idle;
                }
            }
        }

        public int CostToMoveToFloor(int floorNumber)
        {
            var currentFloor = _currentFloor;

            if (floorNumber == currentFloor)
            {
                return 0;
            }

            if (floorNumber > currentFloor && _currentState == State.GoingDown)
            {
                // going down so complete that movement before moving up
                return (currentFloor - _destFloorMin.GetValueOrDefault()) + floorNumber;
            }

            if (floorNumber < currentFloor && _currentState == State.GoingUp)
            {
                // going up so complete that movement before moving down
                return (_destFloorMax.GetValueOrDefault() - currentFloor) + floorNumber;
            }

            // is idle
            return Math.Abs(currentFloor - floorNumber);
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _reset, 1);
        }
    }
}
