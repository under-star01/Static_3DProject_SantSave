[Serializable]
public class RankingEntry
{
    public string name;
    public int score;
    public string date; // "yyyy-MM-dd HH:mm:ss"

    public string GetDisplayDate()
    {
        return date;
    }
}
