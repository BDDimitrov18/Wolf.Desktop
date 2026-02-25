using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Wolf.Desktop.Services;
using Wolf.Desktop.Views;
using Wolf.Dtos;

namespace Wolf.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly WolfDockFactory _factory = new();
    private readonly OrderFiltersViewModel _orderFilters = new();
    [ObservableProperty] private IRootDock? _layout;
    [ObservableProperty] private ObservableCollection<NavItemViewModel> _navItems = [];

    // Sidebar footer — bound to logged-in user
    public string CurrentUserDisplayName => ServiceLocator.Auth.CurrentDisplayName;
    public string CurrentUserInitials => ServiceLocator.Auth.CurrentInitials;
    public string CurrentUserRole => ServiceLocator.Auth.CurrentRole;

    public MainWindowViewModel()
    {
        InitNavItems();

        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);

        OpenOrCreateTab("orders");
    }

    [RelayCommand]
    private void OpenProfile()
    {
        var key = "user-profile";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var doc = new WolfDocument
        {
            Key = key,
            Id = key,
            Title = "Моят профил",
            Icon = "👤",
            CanClose = true,
            Content = new UserProfileViewModel()
        };
        _factory.AddDocument(doc);

        foreach (var nav in NavItems)
            nav.IsActive = false;
    }

    private void InitNavItems()
    {
        var isAdmin = string.Equals(ServiceLocator.Auth.CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase);

        var items = new List<NavItemViewModel>();

        if (isAdmin)
            items.Add(new NavItemViewModel { Label = "Табло", Icon = "📊", TabKey = "dashboard" });

        items.Add(new NavItemViewModel { Label = "Поръчки",   Icon = "📋", TabKey = "orders",    Badge = "24" });
        items.Add(new NavItemViewModel { Label = "Парцели",   Icon = "🗺", TabKey = "plots",     Badge = "156" });
        items.Add(new NavItemViewModel { Label = "Документи", Icon = "📄", TabKey = "documents" });
        items.Add(new NavItemViewModel { Label = "Клиенти",   Icon = "👥", TabKey = "clients",   Badge = "89" });
        items.Add(new NavItemViewModel { Label = "Служители", Icon = "👷", TabKey = "employees" });
        items.Add(new NavItemViewModel { Label = "Фактури",   Icon = "💰", TabKey = "invoices" });

        foreach (var item in items)
            item.NavigateRequested += OnNavItemNavigate;

        NavItems = new ObservableCollection<NavItemViewModel>(items);
    }

    private void OnNavItemNavigate(NavItemViewModel item)
    {
        OpenOrCreateTab(item.TabKey);
        foreach (var nav in NavItems)
            nav.IsActive = nav.TabKey == item.TabKey;
    }

    private void OpenOrCreateTab(string key)
    {
        var existing = _factory.FindByKey(key);
        if (existing is not null)
        {
            _factory.ActivateDocument(existing);
            return;
        }

        var doc = CreateDocument(key);
        if (doc is null) return;
        _factory.AddDocument(doc);
    }

    private WolfDocument? CreateDocument(string key)
    {
        return key switch
        {
            "orders" => new WolfDocument
            {
                Key = "orders", Id = "orders",
                Title = "Поръчки", Icon = "📋",
                CanClose = true,
                Content = CreateOrdersViewModel()
            },
            "dashboard" => new WolfDocument
            {
                Key = "dashboard", Id = "dashboard",
                Title = "Табло", Icon = "📊",
                CanClose = true,
                Content = new DashboardViewModel()
            },
            "plots" => new WolfDocument
            {
                Key = "plots", Id = "plots",
                Title = "Парцели", Icon = "🗺",
                CanClose = true,
                Content = CreatePlotsViewModel()
            },
            "documents" => new WolfDocument
            {
                Key = "documents", Id = "documents",
                Title = "Документи", Icon = "📄",
                CanClose = true,
                Content = CreateDocumentsViewModel()
            },
            "clients" => new WolfDocument
            {
                Key = "clients", Id = "clients",
                Title = "Клиенти", Icon = "👥",
                CanClose = true,
                Content = CreateClientsViewModel()
            },
            "employees" => new WolfDocument
            {
                Key = "employees", Id = "employees",
                Title = "Служители", Icon = "👷",
                CanClose = true,
                Content = CreateEmployeesViewModel()
            },
            "invoices" => new WolfDocument
            {
                Key = "invoices", Id = "invoices",
                Title = "Фактури", Icon = "💰",
                CanClose = true,
                Content = CreateInvoicesViewModel()
            },
            _ => null
        };
    }

    private OrdersViewModel CreateOrdersViewModel()
    {
        var vm = new OrdersViewModel();
        vm.Filters = _orderFilters;
        vm.OpenEditOrderRequested += OpenEditOrder;
        vm.OpenFiltersRequested += OpenOrderFilters;
        vm.ActivitiesDetail.OpenEditActivityRequested += OpenEditActivity;
        vm.ActivitiesDetail.OpenEditTaskRequested += OpenEditTask;
        vm.PlotsDetail.OpenEditPlotRequested += OpenEditPlot;
        vm.PlotsDetail.OpenEditDocumentRequested += OpenEditDocument;
        vm.ClientsDetail.OpenEditClientRequested += OpenEditClient;
        vm.ClientsDetail.OpenLinkClientRequested += OpenLinkClient;
        vm.InvoicesDetail.OpenEditInvoiceRequested += OpenEditInvoiceForOrder;
        vm.LoadAsync();
        return vm;
    }

    private void OpenOrderFilters()
    {
        var key = "order-filters";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = "Филтри", Icon = "🔍",
            CanClose = true,
            Content = _orderFilters
        });
    }

    private ClientsViewModel CreateClientsViewModel()
    {
        var vm = new ClientsViewModel();
        vm.OpenEditClientRequested += OpenEditClient;
        return vm;
    }

    private AllPlotsViewModel CreatePlotsViewModel()
    {
        var vm = new AllPlotsViewModel();
        vm.OpenEditPlotRequested += OpenEditPlotStandalone;
        return vm;
    }

    private AllDocumentsViewModel CreateDocumentsViewModel()
    {
        var vm = new AllDocumentsViewModel();
        vm.OpenEditDocumentRequested += OpenEditDocumentStandalone;
        return vm;
    }

    private EmployeesViewModel CreateEmployeesViewModel()
    {
        var vm = new EmployeesViewModel();
        vm.OpenEmployeeStatsRequested += OpenEmployeeStats;
        return vm;
    }

    private InvoicesViewModel CreateInvoicesViewModel()
    {
        var vm = new InvoicesViewModel();
        vm.OpenEditInvoiceRequested += OpenEditInvoice;
        return vm;
    }

    [RelayCommand]
    private void Logout()
    {
        _ = ServiceLocator.Realtime.StopAsync();
        ServiceLocator.Auth.Logout();
        ServiceLocator.Cache.Clear();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginVm = new LoginViewModel();
            var loginWindow = new LoginWindow { DataContext = loginVm };

            loginVm.LoginSucceeded += () =>
            {
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                loginWindow.Close();
            };

            var currentWindow = desktop.MainWindow;
            desktop.MainWindow = loginWindow;
            loginWindow.Show();
            currentWindow?.Close();
        }
    }

    private void CloseByKey(string key)
    {
        var doc = _factory.FindByKey(key);
        if (doc is not null) _factory.CloseDocument(doc);
    }

    // ── Edit Order ─────────────────────────────────────────────────

    private void OpenEditOrder(OrderRowViewModel? row)
    {
        var key = row is null ? "new-order" : $"edit-order-{row.Requestid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditOrderViewModel();
        if (row is not null)
        {
            vm.RequestId = row.Requestid;
            vm.Requestname = row.Requestname;
            vm.Price = row.Price;
            vm.Advance = row.Advance;
            vm.Status = row.Status;
            vm.Comments = row.Comments;
        }

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = row is null ? "Нова поръчка" : $"Поръчка #{row.Requestid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Activity ──────────────────────────────────────────────

    private void OpenEditActivity(ActivityDto? dto, int requestId)
    {
        var key = dto is null ? $"new-activity-{requestId}" : $"edit-activity-{dto.Activityid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditActivityViewModel();
        vm.RequestId = requestId;
        if (dto is not null)
            vm.LoadFromDto(dto);
        vm.LoadPickerData();
        vm.ApplyDeferredSelections();

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нова дейност" : $"Дейност #{dto.Activityid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Task ──────────────────────────────────────────────────

    private void OpenEditTask(TaskDto? dto, int activityId)
    {
        var key = dto is null ? $"new-task-{activityId}" : $"edit-task-{dto.Taskid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditTaskViewModel();
        vm.ActivityId = activityId;
        if (dto is not null)
            vm.LoadFromDto(dto);
        vm.LoadPickerData();
        vm.ApplyDeferredSelections();

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нова задача" : $"Задача #{dto.Taskid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Plot ─────────────────────────────────────────────────

    private void OpenEditPlot(PlotDto? dto, int requestId)
    {
        var key = dto is null ? $"new-plot-{requestId}" : $"edit-plot-{dto.Plotid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditPlotViewModel();
        vm.RequestId = requestId;
        if (dto is not null)
            vm.LoadFromDto(dto);
        vm.LoadPickerData();

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нов парцел" : $"Парцел #{dto.Plotid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Document ─────────────────────────────────────────────

    private void OpenEditDocument(DocumentsofownershipDto? dto, int plotId)
    {
        var key = dto is null ? $"new-doc-{plotId}" : $"edit-doc-{dto.Documentid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditDocumentViewModel();
        vm.PlotId = plotId;
        if (dto is not null)
            vm.LoadFromDto(dto);
        vm.LoadPickerData();
        if (dto is not null)
            vm.LoadExistingOwners();

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нов документ" : $"Документ #{dto.Documentid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Plot (standalone) ────────────────────────────────────

    private void OpenEditPlotStandalone(PlotDto? dto)
    {
        var key = dto is null ? "new-plot-standalone" : $"edit-plot-{dto.Plotid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditPlotViewModel();
        if (dto is not null)
            vm.LoadFromDto(dto);

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нов парцел" : $"Парцел #{dto.Plotid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Document (standalone) ─────────────────────────────────

    private void OpenEditDocumentStandalone(DocumentsofownershipDto? dto)
    {
        var key = dto is null ? "new-doc-standalone" : $"edit-doc-{dto.Documentid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditDocumentViewModel();
        if (dto is not null)
            vm.LoadFromDto(dto);
        vm.LoadPickerData();
        if (dto is not null)
            vm.LoadExistingOwners();

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нов документ" : $"Документ #{dto.Documentid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Client ───────────────────────────────────────────────

    private void OpenEditClient(ClientDto? dto)
    {
        var key = dto is null ? "new-client" : $"edit-client-{dto.Clientid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditClientViewModel();
        if (dto is not null)
            vm.LoadFromDto(dto);

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нов клиент" : $"Клиент #{dto.Clientid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }

    // ── Link Client to Order ──────────────────────────────────────

    private void OpenLinkClient(int requestId)
    {
        var key = $"link-client-{requestId}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new OrderClientLinkViewModel { RequestId = requestId };
        vm.Load();
        vm.LinkCompleted += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = $"Свързване клиент - Поръчка #{requestId}",
            Icon = "🔗", CanClose = true,
            Content = vm
        });
    }

    // ── Employee Statistics ──────────────────────────────────────

    private void OpenEmployeeStats(EmployeeDto dto)
    {
        var key = $"employee-stats-{dto.Employeeid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EmployeeStatsViewModel();
        vm.Load(dto);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = $"Статистика: {dto.Firstname} {dto.Lastname}",
            Icon = "📊", CanClose = true,
            Content = vm
        });
    }

    // ── Edit Invoice ─────────────────────────────────────────────

    private void OpenEditInvoice(InvoiceDto? dto)
    {
        OpenEditInvoiceForOrder(dto, 0);
    }

    private void OpenEditInvoiceForOrder(InvoiceDto? dto, int requestId)
    {
        var key = dto is null
            ? (requestId > 0 ? $"new-invoice-{requestId}" : "new-invoice")
            : $"edit-invoice-{dto.Invoiceid}";
        var existing = _factory.FindByKey(key);
        if (existing is not null) { _factory.ActivateDocument(existing); return; }

        var vm = new EditInvoiceViewModel();
        if (dto is not null)
            vm.LoadFromDto(dto);
        else if (requestId > 0)
            vm.RequestId = requestId;

        vm.SaveCompleted += () => CloseByKey(key);
        vm.CancelRequested += () => CloseByKey(key);

        _factory.AddDocument(new WolfDocument
        {
            Key = key, Id = key,
            Title = dto is null ? "Нова фактура" : $"Фактура #{dto.Invoiceid}",
            Icon = "✏️", CanClose = true,
            Content = vm
        });
    }
}
