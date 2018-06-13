using System.Threading;
using System.Threading.Tasks;

namespace LiftSystem
{
    public interface ILift
    {
        Task DoWorkAsync(CancellationToken cancellationToken);
        void DoWork(CancellationToken cancellationToken);
        void SummonToFloor(int floor);
        int CostToMoveToFloor(int floorNumber);

        void Reset();
    }
}