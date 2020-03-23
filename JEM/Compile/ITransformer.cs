namespace JEM.Compile
{
    public interface ITransformer<TInput, TOutput>
    {
        bool MatchesPattern(TInput input);
        TOutput Transform(TInput input);
    }
}