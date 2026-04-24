using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// 刮刮卡结算结果。
    /// </summary>
    public class ScratchSettlementResult
    {
        public int FinalScore { get; set; }
        public string Summary { get; set; }
        public List<int> WinningPatternIds { get; set; } = new List<int>();
    }
}
