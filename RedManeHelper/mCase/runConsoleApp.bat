echo Begining Class Sync
::rem [0] path to exe file
::rem [1] connection string (Verify that connection string allows access to trusted certificate: TrustServerCertificate=true)
::rem [2] sql query
::rem [3] system login for "ADMIN" account (mcase requires admin role for exporting data)
::rem [4 mcase url
::rem [5] output directory
::rem [6] exception directory exist
::rem [7] namespace

::rem (5 & 6) will be created if none exist

::Direct executable filepath "connection string" "sql query" "credentials" "target url" "output directory" "output exception directory" "namespace"

rem [0] path to .exe file
rem [1] csv data
rem [2] system login for "ADMIN" account (mcase requires admin role for exporting data)
rem [3] mcase url
rem [4] output directory
rem [5] exception directory
rem [6] namespace

rem (4 & 5) will be created if none exist

Direct executable filepath "csv data" "credentials" "target url" "output directory" "output exception directory" "namespace"

pause