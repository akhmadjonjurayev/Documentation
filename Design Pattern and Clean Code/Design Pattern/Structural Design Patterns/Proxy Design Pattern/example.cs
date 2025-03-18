public interface IVideoService
{
    void Play();
}

public class VideoService : IVideoService
{
    private readonly string _fileName;
    public VideoService(string fileName)
    {
        this._fileName = fileName;
        Load();
    }

    public void Play()
    {
        Console.WriteLine($"Playing video {_fileName}");
    }

    private void Load()
    {
        Console.WriteLine($"Loading video {_fileName} (This is slow)");
    }
}

public class VideoProxy : IVideoService
{
    private VideoService _realVideo;
    private readonly string _fileName;

    public VideoProxy(string fileName)
    {
        this._fileName = fileName;
    }

    public void Play()
    {
        if (_realVideo == null)
        {
            _realVideo = new VideoService(_fileName);
        }

        _realVideo.Play();
    }
}

class Program
{
    static void Main()
    {
        IVideoService video = new VideoProxy("movie.mp4");

        Console.WriteLine("First Play : ...");
        video.Play();

        Console.WriteLine("Second Play : ...");
        video.Play();
        Console.ReadKey();
    }
}