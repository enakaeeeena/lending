using lending_skills_backend.DataAccess;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using lending_skills_backend.Validators;
using lending_skills_backend.Repositories;
var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5218")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Set port
builder.WebHost.UseUrls("http://localhost:5218");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Lending Skills API",
        Version = "v1",
        Description = "API для платформы Lending Skills"
    });
});

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<GetPageRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddBlockToPageRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<ChangeBlockPositionRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<EditBlockRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddProgramRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<EditProgramRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<FormRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<GetFormsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<HideFormsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<ShowFormsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveFormsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<GetUsersRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddProgramAdminRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveProgramAdminRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddProfessorRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateProfessorRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddProfessorToProgramRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveProfessorFromProgramRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<ChangeProfessorProgramPositionRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddTagRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateTagRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveTagRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddTagToWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveTagFromWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddSkillRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateSkillRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveSkillRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddSkillToWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveSkillFromWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddSkillToUserRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<RemoveSkillFromUserRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<GetWorksRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<HideWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<ShowWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<LikeWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UnlikeWorkRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<GetProfilesRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<CreateStudentProfileRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateProfileRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<HideProfileRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<ShowProfileRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<GetReviewsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<UpdateStudentReviewsRequestValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<AddReviewRequestValidator>();
        });
   
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<ReviewsRepository>();
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<ProgramsRepository>();
builder.Services.AddScoped<FormsRepository>();
builder.Services.AddScoped<ProfessorsRepository>();
builder.Services.AddScoped<BlocksRepository>();
builder.Services.AddScoped<SkillsRepository>();
builder.Services.AddScoped<TagsRepository>();
builder.Services.AddScoped<WorksRepository>();
builder.Services.AddScoped<TokensRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
using var scope = app.Services.CreateScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await dbContext.Database.EnsureCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lending Skills API V1");
        c.RoutePrefix = "swagger";
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });
}

// Enable CORS before other middleware
app.UseCors();

app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/", () => "Backend server is running!");

app.Run();