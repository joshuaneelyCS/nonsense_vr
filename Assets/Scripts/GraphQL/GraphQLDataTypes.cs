using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;

public class GraphQLResponse
{
    public Data data { get; set; }
}

public class Data
{
    public TitleConnection titleConnection { get; set; }
    public SceneConnection sceneConnection { get; set; }
}

public class TitleConnection
{
    public Edge[] edges { get; set; }
}

public class SceneConnection
{
    public Edge[] edges;
}

    public class Edge
{
    public string cursor { get; set; }
    public Movie movie { get; set; }
    public Scene scene { get; set; }
}

public class Movie
{
    public string title { get; set; }
    public string id { get; set; }
    public Scene[] allScenes { get; set; }
    public string posterImageUrl { get; set; }
    public string summary {  get; set; }
}

public class Scene
{
    public string mediaTitle { get; set; }
    public string portraitVideoUrl { get; set; }
    public Chapter chapter { get; set; }
    public float timeStart { get; set; }
    public float timeEnd { get; set; }
    public PhraseConnection phraseConnection { get; set; }
}

public class Chapter
{
    public float timeStart { get; set; }
}

public class PhraseConnection
{
    public List<PhraseEdge> edges { get; set; }
}
public class PhraseEdge
{
    public Word words { get; set; }
}

public class Word
{
    public string text { get; set; }
    public string timeStart { get; set; }
    public string timeEnd { get; set; }
    public List<Token> tokens { get; set; }
    public List<TokenTimings> tokenTimings { get; set; }
}

public class Token
{
    public string text {  set; get; }
}

public class TokenTimings
{
    public string timeStart { get; set; }
    public string timeEnd { get; set; }
}