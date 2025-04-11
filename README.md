# Installation
>#1 Clone the repository:https://github.com/KTZN/ECommerce.git
>#2 Configure the database connection in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShoppingCartDb;Trusted_Connection=True;TrustServerCertificate=true;"
}
>#3 Install required packages:
```powershell
dotnet restore
```

# Running the Application
```powershell
dotnet run
```
# Unit Tests
```powershell
cd ECommerce.Tests
dotnet test
```