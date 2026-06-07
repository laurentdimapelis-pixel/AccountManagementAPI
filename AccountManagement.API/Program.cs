using AccountManagementAppService;
using AccountManagementModels;

var builder = WebApplication.CreateBuilder(args);

// Core services for routing only
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// === INSTANTIATE SYSTEM MODULE LOGIC ===
var accountService = new AccountAppService();

// 1. GET ALL ACCOUNTS
app.MapGet("/api/accounts", () => {
    return Results.Ok(accountService.GetAccounts());
});

// 2. GET ACCOUNT BY ID
app.MapGet("/api/accounts/{id}", (Guid id) => {
    var account = accountService.GetAccount(id);
    return account is not null 
        ? Results.Ok(account) 
        : Results.NotFound(new { message = $"Account with ID {id} not found." });
});

// 3. POST (REGISTER / CREATE ACCOUNT)
app.MapPost("/api/accounts", (Account account) => {
    if (account == null) return Results.BadRequest();
    var success = accountService.Register(account);
    return success 
        ? Results.Created($"/api/accounts/{account.AccountId}", account) 
        : Results.BadRequest(new { message = "Failed to register account." });
});

// 4. PUT (UPDATE ACCOUNT USER)
app.MapPut("/api/accounts/{id}", (Guid id, Account account) => {
    var existingAccount = accountService.GetAccount(id);
    if (existingAccount == null) return Results.NotFound();

    account.AccountId = id; 
    accountService.UpdateUser(account);
    return Results.NoContent();
});

// 5. DELETE (REMOVE USER)
app.MapDelete("/api/accounts/{id}", (Guid id) => {
    var existingAccount = accountService.GetAccount(id);
    if (existingAccount == null) return Results.NotFound();

    accountService.RemoveUser(id);
    return Results.NoContent();
});

app.Run();