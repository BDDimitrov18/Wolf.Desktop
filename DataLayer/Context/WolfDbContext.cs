using System;
using System.Collections.Generic;
using DataLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using File = DataLayer.Entities.File;
using Task = DataLayer.Entities.Task;

namespace DataLayer.Context;

public partial class WolfDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public WolfDbContext()
    {
    }

    public WolfDbContext(DbContextOptions<WolfDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityPlotrelashionship> ActivityPlotrelashionships { get; set; }

    public virtual DbSet<Activitytype> Activitytypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientRequestrelashionship> ClientRequestrelashionships { get; set; }

    public virtual DbSet<DocumentofownershipOwnerrelashionship> DocumentofownershipOwnerrelashionships { get; set; }

    public virtual DbSet<DocumentplotDocumentowenerrelashionship> DocumentplotDocumentowenerrelashionships { get; set; }

    public virtual DbSet<Documentsofownership> Documentsofownerships { get; set; }

    public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Plot> Plots { get; set; }

    public virtual DbSet<PlotDocumentofownership> PlotDocumentofownerships { get; set; }

    public virtual DbSet<Powerofattorneydocument> Powerofattorneydocuments { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<StarrequestEmployeerelashionship> StarrequestEmployeerelashionships { get; set; }

    public virtual DbSet<Sysdiagram> Sysdiagrams { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Tasktype> Tasktypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Wolf.Data;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Map Identity tables to existing lowercase PostgreSQL schema
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("aspnetusers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserName).HasColumnName("username");
            entity.Property(e => e.NormalizedUserName).HasColumnName("normalizedusername");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.NormalizedEmail).HasColumnName("normalizedemail");
            entity.Property(e => e.EmailConfirmed).HasColumnName("emailconfirmed");
            entity.Property(e => e.PasswordHash).HasColumnName("passwordhash");
            entity.Property(e => e.SecurityStamp).HasColumnName("securitystamp");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("concurrencystamp");
            entity.Property(e => e.PhoneNumber).HasColumnName("phonenumber");
            entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("phonenumberconfirmed");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("twofactorenabled");
            entity.Property(e => e.LockoutEnd).HasPrecision(6).HasColumnName("lockoutend");
            entity.Property(e => e.LockoutEnabled).HasColumnName("lockoutenabled");
            entity.Property(e => e.AccessFailedCount).HasColumnName("accessfailedcount");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");

            entity.HasIndex(e => e.NormalizedEmail, "emailindex");
            entity.HasIndex(e => e.Employeeid, "ix_aspnetusers_employeeid").IsUnique();
            entity.HasIndex(e => e.NormalizedUserName, "usernameindex").IsUnique();

            entity.HasOne(d => d.Employee).WithOne(p => p.Aspnetuser)
                .HasForeignKey<ApplicationUser>(d => d.Employeeid)
                .HasConstraintName("fk_aspnetusers_employees_employeeid");
        });

        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("aspnetroles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NormalizedName).HasColumnName("normalizedname");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("concurrencystamp");

            entity.HasIndex(e => e.NormalizedName, "rolenameindex").IsUnique();
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("aspnetuserroles");

            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.RoleId).HasColumnName("roleid");

            entity.HasIndex(e => e.RoleId).HasDatabaseName("ix_aspnetuserroles_roleid");
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("aspnetuserclaims");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.ClaimType).HasColumnName("claimtype");
            entity.Property(e => e.ClaimValue).HasColumnName("claimvalue");

            entity.HasIndex(e => e.UserId, "ix_aspnetuserclaims_userid");
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("aspnetroleclaims");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(e => e.RoleId).HasColumnName("roleid");
            entity.Property(e => e.ClaimType).HasColumnName("claimtype");
            entity.Property(e => e.ClaimValue).HasColumnName("claimvalue");

            entity.HasIndex(e => e.RoleId, "ix_aspnetroleclaims_roleid");
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("aspnetuserlogins");

            entity.Property(e => e.LoginProvider).HasColumnName("loginprovider");
            entity.Property(e => e.ProviderKey).HasColumnName("providerkey");
            entity.Property(e => e.ProviderDisplayName).HasColumnName("providerdisplayname");
            entity.Property(e => e.UserId).HasColumnName("userid");

            entity.HasIndex(e => e.UserId, "ix_aspnetuserlogins_userid");
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("aspnetusertokens");

            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.LoginProvider).HasColumnName("loginprovider");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        // Domain entities
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Activityid).HasName("pk_activities");

            entity.ToTable("activities");

            entity.HasIndex(e => e.Activitytypeid, "ix_activities_activitytypeid");

            entity.HasIndex(e => e.Executantid, "ix_activities_executantid");

            entity.HasIndex(e => e.Parentactivityid, "ix_activities_parentactivityid");

            entity.HasIndex(e => e.Requestid, "ix_activities_requestid");

            entity.Property(e => e.Activityid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(8285L, null, null, null, null, null)
                .HasColumnName("activityid");
            entity.Property(e => e.Activitytypeid).HasColumnName("activitytypeid");
            entity.Property(e => e.Employeepayment).HasColumnName("employeepayment");
            entity.Property(e => e.Executantid)
                .HasDefaultValue(0)
                .HasColumnName("executantid");
            entity.Property(e => e.Expectedduration)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("expectedduration");
            entity.Property(e => e.Parentactivityid).HasColumnName("parentactivityid");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Startdate)
                .HasDefaultValueSql("'0001-01-01 00:00:00'::timestamp without time zone")
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("startdate");

            entity.HasOne(d => d.Activitytype).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Activitytypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activities_activitytypes_activitytypeid");

            entity.HasOne(d => d.Executant).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Executantid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activities_employees_executantid");

            entity.HasOne(d => d.Parentactivity).WithMany(p => p.InverseParentactivity)
                .HasForeignKey(d => d.Parentactivityid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_activities_activities_parentactivityid");

            entity.HasOne(d => d.Request).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Requestid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activities_requests_requestid");
        });

        modelBuilder.Entity<ActivityPlotrelashionship>(entity =>
        {
            entity.HasKey(e => new { e.Activityid, e.Plotid }).HasName("pk_activity_plotrelashionships");

            entity.ToTable("activity_plotrelashionships");

            entity.HasIndex(e => e.Plotid, "ix_activity_plotrelashionships_plotid");

            entity.Property(e => e.Activityid).HasColumnName("activityid");
            entity.Property(e => e.Plotid).HasColumnName("plotid");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityPlotrelashionships)
                .HasForeignKey(d => d.Activityid)
                .HasConstraintName("fk_activity_plotrelashionships_activities_activityid");

            entity.HasOne(d => d.Plot).WithMany(p => p.ActivityPlotrelashionships)
                .HasForeignKey(d => d.Plotid)
                .HasConstraintName("fk_activity_plotrelashionships_plots_plotid");
        });

        modelBuilder.Entity<Activitytype>(entity =>
        {
            entity.HasKey(e => e.Activitytypeid).HasName("pk_activitytypes");

            entity.ToTable("activitytypes");

            entity.Property(e => e.Activitytypeid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(3059L, null, null, null, null, null)
                .HasColumnName("activitytypeid");
            entity.Property(e => e.Activitytypename).HasColumnName("activitytypename");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Clientid).HasName("pk_clients");

            entity.ToTable("clients");

            entity.Property(e => e.Clientid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(7377L, null, null, null, null, null)
                .HasColumnName("clientid");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Clientlegaltype).HasColumnName("clientlegaltype");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Firstname).HasColumnName("firstname");
            entity.Property(e => e.Lastname).HasColumnName("lastname");
            entity.Property(e => e.Middlename).HasColumnName("middlename");
            entity.Property(e => e.Phone).HasColumnName("phone");
        });

        modelBuilder.Entity<ClientRequestrelashionship>(entity =>
        {
            entity.HasKey(e => new { e.Requestid, e.Clientid }).HasName("pk_client_requestrelashionships");

            entity.ToTable("client_requestrelashionships");

            entity.HasIndex(e => e.Clientid, "ix_client_requestrelashionships_clientid");

            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Ownershiptype).HasColumnName("ownershiptype");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientRequestrelashionships)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("fk_client_requestrelashionships_clients_clientid");

            entity.HasOne(d => d.Request).WithMany(p => p.ClientRequestrelashionships)
                .HasForeignKey(d => d.Requestid)
                .HasConstraintName("fk_client_requestrelashionships_requests_requestid");
        });

        modelBuilder.Entity<DocumentofownershipOwnerrelashionship>(entity =>
        {
            entity.HasKey(e => e.Documentownerid).HasName("pk_documentofownership_ownerrelashionships");

            entity.ToTable("documentofownership_ownerrelashionships");

            entity.HasIndex(e => e.Documentid, "ix_documentofownership_ownerrelashionships_documentid");

            entity.HasIndex(e => e.Ownerid, "ix_documentofownership_ownerrelashionships_ownerid");

            entity.Property(e => e.Documentownerid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1023L, null, null, null, null, null)
                .HasColumnName("documentownerid");
            entity.Property(e => e.Documentid).HasColumnName("documentid");
            entity.Property(e => e.Ownerid).HasColumnName("ownerid");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentofownershipOwnerrelashionships)
                .HasForeignKey(d => d.Documentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documentofownership_ownerrelashionships_documentsofownership");

            entity.HasOne(d => d.Owner).WithMany(p => p.DocumentofownershipOwnerrelashionships)
                .HasForeignKey(d => d.Ownerid)
                .HasConstraintName("fk_documentofownership_ownerrelashionships_owners_ownerid");
        });

        modelBuilder.Entity<DocumentplotDocumentowenerrelashionship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_documentplot_documentowenerrelashionships");

            entity.ToTable("documentplot_documentowenerrelashionships");

            entity.HasIndex(e => e.Documentownerid, "ix_documentplot_documentowenerrelashionships_documentownerid");

            entity.HasIndex(e => e.Documentplotid, "ix_documentplot_documentowenerrelashionships_documentplotid");

            entity.HasIndex(e => e.Powerofattorneyid, "ix_documentplot_documentowenerrelashionships_powerofattorneyid");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1026L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.Documentownerid).HasColumnName("documentownerid");
            entity.Property(e => e.Documentplotid).HasColumnName("documentplotid");
            entity.Property(e => e.Idealparts).HasColumnName("idealparts");
            entity.Property(e => e.Isdrob)
                .IsRequired()
                .HasDefaultValueSql("(0)::boolean")
                .HasColumnName("isdrob");
            entity.Property(e => e.Powerofattorneyid)
                .HasDefaultValue(0)
                .HasColumnName("powerofattorneyid");
            entity.Property(e => e.Wayofacquiring).HasColumnName("wayofacquiring");

            entity.HasOne(d => d.Documentowner).WithMany(p => p.DocumentplotDocumentowenerrelashionships)
                .HasForeignKey(d => d.Documentownerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documentplot_documentowenerrelashionships_documentofownershi");

            entity.HasOne(d => d.Documentplot).WithMany(p => p.DocumentplotDocumentowenerrelashionships)
                .HasForeignKey(d => d.Documentplotid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documentplot_documentowenerrelashionships_plot_documentofown");

            entity.HasOne(d => d.Powerofattorney).WithMany(p => p.DocumentplotDocumentowenerrelashionships)
                .HasForeignKey(d => d.Powerofattorneyid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documentplot_documentowenerrelashionships_powerofattorneydoc");
        });

        modelBuilder.Entity<Documentsofownership>(entity =>
        {
            entity.HasKey(e => e.Documentid).HasName("pk_documentsofownership");

            entity.ToTable("documentsofownership");

            entity.Property(e => e.Documentid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1022L, null, null, null, null, null)
                .HasColumnName("documentid");
            entity.Property(e => e.Dateofissuing)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("dateofissuing");
            entity.Property(e => e.Dateofregistering)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("dateofregistering");
            entity.Property(e => e.Doccase).HasColumnName("doccase");
            entity.Property(e => e.Issuer).HasColumnName("issuer");
            entity.Property(e => e.Numberofdocument).HasColumnName("numberofdocument");
            entity.Property(e => e.Register).HasColumnName("register");
            entity.Property(e => e.Tom).HasColumnName("tom");
            entity.Property(e => e.Typeofdocument).HasColumnName("typeofdocument");
            entity.Property(e => e.Typeofownership)
                .HasDefaultValueSql("''::text")
                .HasColumnName("typeofownership");
        });

        modelBuilder.Entity<Efmigrationshistory>(entity =>
        {
            entity.HasKey(e => e.Migrationid).HasName("pk___efmigrationshistory");

            entity.ToTable("__efmigrationshistory");

            entity.Property(e => e.Migrationid).HasColumnName("migrationid");
            entity.Property(e => e.Productversion).HasColumnName("productversion");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Employeeid).HasName("pk_employees");

            entity.ToTable("employees");

            entity.Property(e => e.Employeeid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1012L, null, null, null, null, null)
                .HasColumnName("employeeid");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Firstname).HasColumnName("firstname");
            entity.Property(e => e.Lastname).HasColumnName("lastname");
            entity.Property(e => e.Outsider)
                .IsRequired()
                .HasDefaultValueSql("(0)::boolean")
                .HasColumnName("outsider");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Secondname).HasColumnName("secondname");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.Fileid).HasName("pk_files");

            entity.ToTable("files");

            entity.Property(e => e.Fileid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(3L, null, null, null, null, null)
                .HasColumnName("fileid");
            entity.Property(e => e.Filename).HasColumnName("filename");
            entity.Property(e => e.Filepath).HasColumnName("filepath");
            entity.Property(e => e.Uploadedat)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("uploadedat");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Invoiceid).HasName("pk_invoices");

            entity.ToTable("invoices");

            entity.HasIndex(e => e.Requestid, "ix_invoices_requestid");

            entity.Property(e => e.Invoiceid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(5088L, null, null, null, null, null)
                .HasColumnName("invoiceid");
            entity.Property(e => e.Number)
                .HasDefaultValueSql("''::text")
                .HasColumnName("number");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Sum).HasColumnName("sum");

            entity.HasOne(d => d.Request).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.Requestid)
                .HasConstraintName("fk_invoices_requests_requestid");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Ownerid).HasName("pk_owners");

            entity.ToTable("owners");

            entity.Property(e => e.Ownerid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1030L, null, null, null, null, null)
                .HasColumnName("ownerid");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Egn).HasColumnName("egn");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
        });

        modelBuilder.Entity<Plot>(entity =>
        {
            entity.HasKey(e => e.Plotid).HasName("pk_plots");

            entity.ToTable("plots");

            entity.Property(e => e.Plotid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(7712L, null, null, null, null, null)
                .HasColumnName("plotid");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Designation).HasColumnName("designation");
            entity.Property(e => e.Locality).HasColumnName("locality");
            entity.Property(e => e.Municipality).HasColumnName("municipality");
            entity.Property(e => e.Neighborhood).HasColumnName("neighborhood");
            entity.Property(e => e.Plotnumber).HasColumnName("plotnumber");
            entity.Property(e => e.Regulatedplotnumber).HasColumnName("regulatedplotnumber");
            entity.Property(e => e.Street).HasColumnName("street");
            entity.Property(e => e.Streetnumber).HasColumnName("streetnumber");
        });

        modelBuilder.Entity<PlotDocumentofownership>(entity =>
        {
            entity.HasKey(e => e.Documentplotid).HasName("pk_plot_documentofownerships");

            entity.ToTable("plot_documentofownerships");

            entity.HasIndex(e => e.Documentofownershipid, "ix_plot_documentofownerships_documentofownershipid");

            entity.HasIndex(e => e.Plotid, "ix_plot_documentofownerships_plotid");

            entity.Property(e => e.Documentplotid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1022L, null, null, null, null, null)
                .HasColumnName("documentplotid");
            entity.Property(e => e.Documentofownershipid).HasColumnName("documentofownershipid");
            entity.Property(e => e.Plotid).HasColumnName("plotid");

            entity.HasOne(d => d.Documentofownership).WithMany(p => p.PlotDocumentofownerships)
                .HasForeignKey(d => d.Documentofownershipid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_plot_documentofownerships_documentsofownership_documentofown");

            entity.HasOne(d => d.Plot).WithMany(p => p.PlotDocumentofownerships)
                .HasForeignKey(d => d.Plotid)
                .HasConstraintName("fk_plot_documentofownerships_plots_plotid");
        });

        modelBuilder.Entity<Powerofattorneydocument>(entity =>
        {
            entity.HasKey(e => e.Powerofattorneyid).HasName("pk_powerofattorneydocuments");

            entity.ToTable("powerofattorneydocuments");

            entity.Property(e => e.Powerofattorneyid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1022L, null, null, null, null, null)
                .HasColumnName("powerofattorneyid");
            entity.Property(e => e.Dateofissuing)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("dateofissuing");
            entity.Property(e => e.Issuer).HasColumnName("issuer");
            entity.Property(e => e.Number).HasColumnName("number");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Requestid).HasName("pk_requests");

            entity.ToTable("requests");

            entity.HasIndex(e => e.Requestcreatorid, "ix_requests_requestcreatorid");

            entity.Property(e => e.Requestid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(8098L, null, null, null, null, null)
                .HasColumnName("requestid");
            entity.Property(e => e.Advance).HasColumnName("advance");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.Paymentstatus).HasColumnName("paymentstatus");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Requestcreatorid).HasColumnName("requestcreatorid");
            entity.Property(e => e.Requestname).HasColumnName("requestname");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Active'::text")
                .HasColumnName("status");

            entity.HasOne(d => d.Requestcreator).WithMany(p => p.Requests)
                .HasForeignKey(d => d.Requestcreatorid)
                .HasConstraintName("fk_requests_employees_requestcreatorid");
        });

        modelBuilder.Entity<StarrequestEmployeerelashionship>(entity =>
        {
            entity.HasKey(e => new { e.Requestid, e.Employeeid }).HasName("pk_starrequest_employeerelashionships");

            entity.ToTable("starrequest_employeerelashionships");

            entity.HasIndex(e => e.Employeeid, "ix_starrequest_employeerelashionships_employeeid");

            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Starcolor).HasColumnName("starcolor");

            entity.HasOne(d => d.Employee).WithMany(p => p.StarrequestEmployeerelashionships)
                .HasForeignKey(d => d.Employeeid)
                .HasConstraintName("fk_starrequest_employeerelashionships_employees_employeeid");

            entity.HasOne(d => d.Request).WithMany(p => p.StarrequestEmployeerelashionships)
                .HasForeignKey(d => d.Requestid)
                .HasConstraintName("fk_starrequest_employeerelashionships_requests_requestid");
        });

        modelBuilder.Entity<Sysdiagram>(entity =>
        {
            entity.HasKey(e => e.DiagramId).HasName("pk__sysdiagr__c2b05b612e0d7d88");

            entity.ToTable("sysdiagrams");

            entity.HasIndex(e => new { e.PrincipalId, e.Name }, "uk_principal_name").IsUnique();

            entity.Property(e => e.DiagramId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("diagram_id");
            entity.Property(e => e.Definition).HasColumnName("definition");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PrincipalId).HasColumnName("principal_id");
            entity.Property(e => e.Version).HasColumnName("version");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Taskid).HasName("pk_tasks");

            entity.ToTable("tasks");

            entity.HasIndex(e => e.Activityid, "ix_tasks_activityid");

            entity.HasIndex(e => e.Controlid, "ix_tasks_controlid");

            entity.HasIndex(e => e.Executantid, "ix_tasks_executantid");

            entity.HasIndex(e => e.Tasktypeid, "ix_tasks_tasktypeid");

            entity.Property(e => e.Taskid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(8842L, null, null, null, null, null)
                .HasColumnName("taskid");
            entity.Property(e => e.Activityid).HasColumnName("activityid");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Commenttax)
                .HasDefaultValueSql("''::text")
                .HasColumnName("commenttax");
            entity.Property(e => e.Controlid).HasColumnName("controlid");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Executantid).HasColumnName("executantid");
            entity.Property(e => e.Executantpayment).HasColumnName("executantpayment");
            entity.Property(e => e.Finishdate)
                .HasDefaultValueSql("'0001-01-01 00:00:00'::timestamp without time zone")
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("finishdate");
            entity.Property(e => e.Startdate)
                .HasColumnType("timestamp(6) without time zone")
                .HasColumnName("startdate");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("''::text")
                .HasColumnName("status");
            entity.Property(e => e.Tasktypeid).HasColumnName("tasktypeid");
            entity.Property(e => e.Tax).HasColumnName("tax");

            entity.HasOne(d => d.Activity).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.Activityid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tasks_activities_activityid");

            entity.HasOne(d => d.Control).WithMany(p => p.TaskControls)
                .HasForeignKey(d => d.Controlid)
                .HasConstraintName("fk_tasks_employees_controlid");

            entity.HasOne(d => d.Executant).WithMany(p => p.TaskExecutants)
                .HasForeignKey(d => d.Executantid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tasks_employees_executantid");

            entity.HasOne(d => d.Tasktype).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.Tasktypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tasks_tasktypes_tasktypeid");
        });

        modelBuilder.Entity<Tasktype>(entity =>
        {
            entity.HasKey(e => e.Tasktypeid).HasName("pk_tasktypes");

            entity.ToTable("tasktypes");

            entity.HasIndex(e => e.Activitytypeid, "ix_tasktypes_activitytypeid");

            entity.Property(e => e.Tasktypeid)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(4220L, null, null, null, null, null)
                .HasColumnName("tasktypeid");
            entity.Property(e => e.Activitytypeid).HasColumnName("activitytypeid");
            entity.Property(e => e.Tasktypename).HasColumnName("tasktypename");

            entity.HasOne(d => d.Activitytype).WithMany(p => p.Tasktypes)
                .HasForeignKey(d => d.Activitytypeid)
                .HasConstraintName("fk_tasktypes_activitytypes_activitytypeid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
