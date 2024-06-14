echo Begining Class Sync

cd C:\Users\jreiner\source\repos\RedManeHelper\RedManeHelper

dotnet run program.cs "data source=localhost;initial catalog=mCASE_ADMIN;integrated security=True" "SELECT [DataListID] FROM [mCASE_ADMIN].[dbo].[DataList]" "admin:Password123!" "http://localhost:64762/Resource/Export/DataList/Configuration/" "C:\Users\jreiner\Desktop\FactoryEntities" "C:\Users\jreiner\Desktop\Exceptions"

echo process completed
pause