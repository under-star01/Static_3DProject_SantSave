using System;
[Serializable]
public class RankingEntry
{
    public string name;
    public int score;
    public string date; // "yyyy-MM-dd HH:mm:ss"

    public RankingEntry(string name, int score, string date)
    {
        this.name = name;
        this.score = score;
        this.date = date;
    }
}
