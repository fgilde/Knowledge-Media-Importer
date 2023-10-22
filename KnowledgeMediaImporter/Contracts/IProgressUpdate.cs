namespace KnowledgeMediaImporter.Contracts;

public interface IProgressUpdate
{
    public IProgressUpdate Update(string text, int value = 0);
    public IProgressUpdate WriteLog(string text);
    public IProgressUpdate Failed(string text);
    public IProgressUpdate Done(string text);
    public IProgressUpdate Start();
}