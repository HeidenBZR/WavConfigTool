using System.Linq;

namespace WavConfigCore
{
    public class AliasTypeMask
    {
        internal int[] Positions { get; private set; }
        internal bool CanTakeAllPositions { get; private set; }

        public AliasTypeMask()
        {
            CanTakeAllPositions = true;
        }

        public AliasTypeMask(int[] positions)
        {
            Positions = positions;
            CanTakeAllPositions = Positions == null || Positions.Length == 0;
        }

        public bool IsAllowedOnPosition(int position)
        {
            return CanTakeAllPositions || Positions.Contains(position);
        }
    }
}
