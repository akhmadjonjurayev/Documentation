public class Memento
{
    public string Context { get; }

    public Memento(string content)
    {
        this.Context = content;
    }
}

public class TextEditor
{
    private string _content = "";

    public void Write(string text)
    {
        _content += text;
    }

    public string GetContent => _content;

    public Memento Save => new Memento(_content);

    public void Restore(Memento memento)
    {
        _content = memento.Context;
    }
}

public class History
{
    private Stack<Memento> _histories = new();

    public void Save(Memento memento)
    {
        _histories.Push(memento);
    }

    public Memento? Undo()
    {
        return _histories.Count > 0 ? _histories.Pop() : null;
    }
}

class Program
{
    static void Main()
    {
        var editor = new TextEditor();
        var history = new History();

        editor.Write("Salom");
        history.Save(editor.Save);

        editor.Write(" Dunyo!");
        history.Save(editor.Save);

        editor.Write(" Qalaysiz?");
        history.Save(editor.Save);

        Console.WriteLine("Hozirgi matn: {0}", editor.GetContent);

        editor.Restore(history.Undo());
        Console.WriteLine("Undo1 : {0}", editor.GetContent);

        editor.Restore(history.Undo());
        Console.WriteLine("Undo2 : {0}", editor.GetContent);

        Console.ReadKey();
    }
}