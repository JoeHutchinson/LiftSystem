using System;
using System.Collections.Generic;
using System.Linq;

namespace LiftSystem
{
    internal sealed class LiftController
    {
        private readonly List<Lift> _lifts;
        private readonly int _numberOfFloors;

        public LiftController(List<Lift> lifts, int numberOfFloors)
        {
            _lifts = lifts;
            _numberOfFloors = numberOfFloors;
        }

        public void SummonLift(int floorNumber)
        {
            bool IsFloorValid()
            {
                return floorNumber < 0 || floorNumber > _numberOfFloors;
            }

            if (IsFloorValid())
            {
                Console.WriteLine($"Invalid floor number, must be greater than 0 and less than or equal to {_numberOfFloors}");
                return;
            }
            var lift = _lifts.OrderBy(x => x.CostToMoveToFloor(floorNumber)).First();

            lift.SummonToFloor(floorNumber);
        }
    }
}
