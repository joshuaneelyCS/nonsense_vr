using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Base class for data in the dictionary

// This class stores all the needed information to
// 1. Make a video screen with title and videoUrl
// 2. Needed information for subtitles
public class VideoPanelData
{
    public MediaPlayer mediaPlayer;
    public string mediaTitle;
    public string portriatVideoUrl;
    public float sceneTimeStart;
    public float sceneTimeEnd;
    public float chapterTimeStart;
    public List<string> phrasesText = new List<string>();
    public List<float> phrasesTimeStart = new List<float>();
    public List<float> phrasesTimeEnd = new List<float>();
    public List<List<string>> tokens = new List<List<string>>();
    public List<List<float>> tokensTimeStart = new List<List<float>>();
    public List<List<float>> tokensTimeEnd = new List<List<float>>();


}
