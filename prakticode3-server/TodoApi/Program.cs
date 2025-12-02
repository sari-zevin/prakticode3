/*using TodoApi;
using Microsoft.EntityFrameworkCore;

// יצירת ה-builder - זה האובייקט שבונה את כל האפליקציה
var builder = WebApplication.CreateBuilder(args);

// הוספת DbContext ל-Services - זה מאפשר לנו להשתמש במסד הנתונים
// GetConnectionString מושך את המחרוזת התחברות מ-appsettings.json
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));

// הוספת CORS - מאפשר לאפליקציית React לדבר עם ה-API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // מאפשר מכל כתובת
              .AllowAnyMethod()    // מאפשר כל סוג בקשה (GET, POST, וכו')
              .AllowAnyHeader();   // מאפשר כל header
    });
});

// הוספת Swagger - זה הממשק היפה שבו אפשר לבדוק את ה-API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();






// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});






// בניית האפליקציה
var app = builder.Build();

// הפעלת Swagger רק בזמן פיתוח
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// הפעלת ה-CORS שהגדרנו למעלה
app.UseCors("AllowAll");

// שימוש ב-CORS
app.UseCors("AllowAll");


// ========== ROUTES - הניתובים של ה-API ==========

// 1. שליפת כל המשימות
// GET: /items
// פשוט מחזיר את כל הרשומות מהטבלה
app.MapGet("/items", async (ToDoDbContext db) =>
{
    // ToList מחזיר רשימה של כל ה-Items
    return await db.Items.ToListAsync();
});

// 2. שליפת משימה בודדת לפי ID
// GET: /items/5
// {id} זה פרמטר שמקבלים מה-URL
app.MapGet("/items/{id}", async (int id, ToDoDbContext db) =>
{
    // FindAsync מחפש לפי המפתח הראשי (Id)
    var item = await db.Items.FindAsync(id);
    
    // אם לא מצאנו - מחזירים 404 (Not Found)
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

// 3. הוספת משימה חדשה
// POST: /items
// מקבל אובייקט Item בגוף הבקשה (body)
app.MapPost("/items", async (Item item, ToDoDbContext db) =>
{
    // Add מוסיף את הרשומה לזיכרון
    db.Items.Add(item);
    
    // SaveChanges שומר בפועל במסד הנתונים
    await db.SaveChangesAsync();
    
    // מחזיר 201 Created עם המשימה החדשה
    return Results.Created($"/items/{item.Id}", item);
});

// 4. עדכון משימה קיימת
// PUT: /items/5
// מקבל ID ב-URL ואובייקט Item מעודכן בגוף הבקשה
app.MapPut("/items/{id}", async (int id, Item updatedItem, ToDoDbContext db) =>
{
    // מחפשים את המשימה הקיימת
    var item = await db.Items.FindAsync(id);
    
    if (item is null) 
        return Results.NotFound();
    
    // מעדכנים את השדות
    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    
    // שומרים את השינויים
    await db.SaveChangesAsync();
    
    // מחזיר 204 No Content (עדכון הצליח)
    return Results.NoContent();
});

// 5. מחיקת משימה
// DELETE: /items/5
// מקבל רק ID למחיקה
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    // מחפשים את המשימה
    var item = await db.Items.FindAsync(id);
    
    if (item is null) 
        return Results.NotFound();
    
    // Remove מוחק מהזיכרון
    db.Items.Remove(item);
    
    // SaveChanges מוחק בפועל מהמסד נתונים
    await db.SaveChangesAsync();
    
    // מחזיר 204 No Content (מחיקה הצליחה)
    return Results.NoContent();
});

// הרצת האפליקציה
app.Run();*/







using TodoApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// הוספת DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
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

app.Run();