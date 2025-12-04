/*using TodoApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// הוספת DbContext
*//*builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));*//*


// קודם ננסה לקרוא ממשתנה סביבה, אם אין - מ-appsettings
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ToDoDB")
                       ?? builder.Configuration.GetConnectionString("ToDoDB");


// 👇 הוסיפי את זה כדי לראות את ה-Connection String בלוגים
Console.WriteLine("=== CONNECTION STRING DEBUG ===");
Console.WriteLine($"Connection String: {connectionString}");
Console.WriteLine("=== END DEBUG ===");


builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));



// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// בניית האפליקציה
var app = builder.Build();

// CORS לפני הכל!
app.UseCors();

// הפעלת Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 👇 הסרנו את app.UseHttpsRedirection() - זה גורם לבעיות ב-Render!

// Routes
app.MapGet("/items", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});

app.MapGet("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/items", async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (int id, Item updatedItem, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();*/



using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

// *******************************************************************
// אנא ודא/י שה-namespace מתאים לשם הפרויקט שלך (לדוגמה: TodoApi)
// קבצים אלו נוצרו ע"י פקודת ה-scaffold:
// *******************************************************************
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// הגדרת CORS - הרשאת גישה כללית
var MyAllowAllOrigins = "_myAllowAllOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowAllOrigins,
        builder =>
        {
            // מאפשר גישה מכל דומיין, עם כל כותרת ובכל HTTP Method
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// הגדרת Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הגדרת Entity Framework Core (EF Core) עם MySql
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

var app = builder.Build();

// שימוש ב-Swagger רק בסביבת פיתוח (אופציונלי, אבל מומלץ)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// שימוש ב-CORS
app.UseCors(MyAllowAllOrigins);

// *******************************************************************
// === התיקון עבור Render: הקשבה לפורט 8080 או הפורט ממשתנה הסביבה PORT ===
// *******************************************************************
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");
// *******************************************************************
// ===================================================================

// ===================================================================
// הגדרת ה-Routes (ניתובים)
// ===================================================================

// 1. שליפת כל המשימות (GET)
app.MapGet("/items", async ([FromServices] ToDoDbContext context) =>
{
    var items = await context.Items.ToListAsync();
    return Results.Ok(items);
})
.WithName("GetAllItems")
.Produces<List<Item>>(StatusCodes.Status200OK);

// 2. הוספת משימה חדשה (POST)
app.MapPost("/items", async ([FromBody] Item item, [FromServices] ToDoDbContext context) =>
{
    // ודא ש-Id הוא 0 על מנת שה-DB יבצע AUTO_INCREMENT
    item.Id = 0;
    context.Items.Add(item);
    await context.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
})
.WithName("CreateItem")
.Produces<Item>(StatusCodes.Status201Created);

// 3. עדכון משימה (PUT)
app.MapPut("/items/{id}", async (int id, [FromBody] Item inputItem, [FromServices] ToDoDbContext context) =>
{
    var item = await context.Items.FindAsync(id);

    if (item == null)
    {
        return Results.NotFound();
    }

    // מעדכן את השדות הנדרשים
    item.Name = inputItem.Name;
    item.IsComplete = inputItem.IsComplete;

    await context.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateItem")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// 4. מחיקת משימה (DELETE)
app.MapDelete("/items/{id}", async (int id, [FromServices] ToDoDbContext context) =>
{
    var item = await context.Items.FindAsync(id);

    if (item == null)
    {
        return Results.NotFound();
    }

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteItem")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// ===================================================================

app.Run();