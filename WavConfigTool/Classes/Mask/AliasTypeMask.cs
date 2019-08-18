using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.Mask
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
