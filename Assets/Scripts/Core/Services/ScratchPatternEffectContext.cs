namespace Core
{
    public readonly struct ScratchPatternEffectContext
    {
        public ScratchPatternEffectContext(ScratchCardModel model, ScratchCellModel cell)
        {
            Model = model;
            Cell = cell;
        }

        public ScratchCardModel Model { get; }
        public ScratchCellModel Cell { get; }
    }
}
