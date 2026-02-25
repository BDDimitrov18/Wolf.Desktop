using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Wolf.Desktop.ViewModels;

namespace Wolf.Desktop.Services;

public class WolfDockFactory : Factory
{
    private DocumentDock? _documentDock;
    private IRootDock? _rootDock;

    public DocumentDock DocumentDock => _documentDock
        ?? throw new InvalidOperationException("Layout not initialized");

    public override IRootDock CreateLayout()
    {
        _documentDock = new DocumentDock
        {
            Id = "MainDocumentDock",
            Title = "Documents",
            CanCreateDocument = false,
            IsCollapsable = false,
        };

        var rootDock = CreateRootDock();
        rootDock.Id = "RootDock";
        rootDock.Title = "Root";
        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = _documentDock;
        rootDock.DefaultDockable = _documentDock;
        rootDock.VisibleDockables = CreateList<IDockable>(_documentDock);

        _rootDock = rootDock;
        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            ["RootDock"] = () => layout,
            ["MainDocumentDock"] = () => layout,
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["Documents"] = () => _documentDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    public void AddDocument(WolfDocument doc)
    {
        AddDockable(DocumentDock, doc);
        SetActiveDockable(doc);
        SetFocusedDockable(DocumentDock, doc);
    }

    public WolfDocument? FindByKey(string key)
    {
        // Search main DocumentDock
        if (_documentDock?.VisibleDockables is not null)
        {
            foreach (var d in _documentDock.VisibleDockables)
            {
                if (d is WolfDocument wd && wd.Key == key)
                    return wd;
            }
        }

        // Search floating windows
        if (_rootDock?.Windows is not null)
        {
            foreach (var window in _rootDock.Windows)
            {
                var found = SearchDockable(window.Layout, key);
                if (found is not null)
                    return found;
            }
        }

        return null;
    }

    private static WolfDocument? SearchDockable(IDockable? dockable, string key)
    {
        if (dockable is WolfDocument wd && wd.Key == key)
            return wd;

        if (dockable is IDock dock && dock.VisibleDockables is not null)
        {
            foreach (var child in dock.VisibleDockables)
            {
                var found = SearchDockable(child, key);
                if (found is not null)
                    return found;
            }
        }

        return null;
    }

    public void CloseDocument(WolfDocument doc)
    {
        CloseDockable(doc);
    }

    public void ActivateDocument(WolfDocument doc)
    {
        SetActiveDockable(doc);
        SetFocusedDockable(DocumentDock, doc);
    }
}
