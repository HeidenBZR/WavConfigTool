using System.Linq;

namespace WavConfigTool.Classes
{
    public class AliasTypeMask
    {
        private int[] positions;
        private bool canTakeAllPositions;

        public AliasTypeMask()
        {
            canTakeAllPositions = true;
        }

        public AliasTypeMask(int[] positions)
        {
            this.positions = positions;
            canTakeAllPositions = positions == null || positions.Length == 0;
        }

        public bool IsAllowedOnPosition(int position)
        {
            return canTakeAllPositions || positions.Contains(position);
        }

        public bool GetCanTakeAllPositions() => canTakeAllPositions;
        public int[] GetPositions() => positions.Clone() as int[];
    }
}
