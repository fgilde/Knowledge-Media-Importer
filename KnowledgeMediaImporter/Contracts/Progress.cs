using System;
using System.Text;
using MudBlazor;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Contracts;

// Define the progress structure.
public class Progress : IProgressUpdate
{
    public CancellationTokenSource? Cancellation { get; set; }
    private StringBuilder _log = new();
    private string _text;
    private int _value;
    private ProgressStatus _status;
    private int _minValue = 0;  // minimum progress value
    private int _maxValue = 100;  // maximum progress value
    public event EventHandler<Progress> Changed;
    public Progress(IUploadableFile file, CancellationTokenSource cancellation)
    {
        Status = ProgressStatus.Queued;
        File = file;
        Cancellation = cancellation;
    }
    public IUploadableFile File { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            WriteLog(value);
            Changed?.Invoke(this, this);
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            int adjustedValue = _minValue + (int)((_maxValue - _minValue) * (value / 100.0));
            _value = Math.Min(Math.Max(adjustedValue, _minValue), _maxValue);
            Changed?.Invoke(this, this);
        }
    }

    public ProgressStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Changed?.Invoke(this, this);
        }
    }

    public string Log => _log.ToString();
    
    public bool IsCompleted => Status is ProgressStatus.Failed or ProgressStatus.Successful || Cancellation?.IsCancellationRequested == true;

    IProgressUpdate IProgressUpdate.WriteLog(string text) => WriteLog(text);

    IProgressUpdate IProgressUpdate.Failed(string text) => Failed(text);

    IProgressUpdate IProgressUpdate.Done(string text) => Done(text);
    IProgressUpdate IProgressUpdate.Start() => Start();

    public Progress Failed(string s)
    {
        Status = ProgressStatus.Failed;
        Text = s;
        Value = 100;
        return WriteLog(s);
    }

    IProgressUpdate IProgressUpdate.Update(string text, int value)
    {
        Text = text;
        Value = Math.Min(value, 100);
        return this;
    }

    public Progress WriteLog(string s)
    {
        _log.AppendLine(s);
        return this;
    }

    public Progress Start()
    {
        Status = ProgressStatus.Running;
        Value = _minValue;
        return this;
    }

    public Progress Done(string t = "Successfully finished")
    {
        Status = ProgressStatus.Successful;
        Text = t;
        Value = _maxValue;
        return this;
    }

    public Color Color =>
        Status switch
        {
            ProgressStatus.Running => Color.Info,
            ProgressStatus.Failed => Color.Error,
            ProgressStatus.Successful => Color.Success,
            _ => Color.Default
        };

    public Severity Severity =>
        Status switch
        {
            ProgressStatus.Running => Severity.Info,
            ProgressStatus.Failed => Severity.Error,
            ProgressStatus.Successful => Severity.Success,
            _ => Severity.Normal
        };

    internal Progress WithoutRange()
    {
        _minValue = 0;
        _maxValue = 100;
        return this;
    }
    internal Progress WithRange(int minValue, int maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        return this;
    }
}

public enum ProgressStatus
{
    Queued,
    Running,
    Failed,
    Successful,
}