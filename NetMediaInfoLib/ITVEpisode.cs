namespace NetMediaInfoLib
{
    public interface ITVEpisode : IChild<ITVSeason>
    {
        int Number { get; }
        string Title { get; }
        string Description { get; }
        string ReleaseDate { get; }
    }
}
