using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TalkData  {
    public string Message { get; set; }
    public float Time { get; set; }
}

public class TalkDataContainer
{
    private const int TimeInterval = 5;
    public List<TalkData> TalkDatas { get; set; }

    public TalkDataContainer()
    {
        TalkDatas = new List<TalkData>();
    }

    public void AddMessage(string message, float time)
    {
        var data = new TalkData {Message = message, Time = time};
        TalkDatas.Add(data);
    }

    public void Clear()
    {
        TalkDatas.Clear();
    }
    public string GetString()
    {
        var sb = new StringBuilder();
        foreach (var talkData in TalkDatas)
        {
            sb.Append(talkData.Message);
        }
        return sb.ToString();
    }

    public void Update(float current)
    {
        foreach (var talkData in TalkDatas.Where(p=> p.Time + TimeInterval < current).ToList())
        {
            TalkDatas.Remove(talkData);
        }
    }
}
