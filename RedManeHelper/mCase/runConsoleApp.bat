echo Begining Class Sync
::rem [1] path to exe file
::rem [2] connection string (Verify that connection string allows access to trusted certificate: TrustServerCertificate=true)
::rem [3] sql query
::rem [4] system login for "ADMIN" account (mcase requires admin role for exporting data)
::rem [5] mcase url
::rem [6] output directory
::rem [7] exception directory exist
::rem [8] namespace

::rem (6 & 7) will be created if none exist

::Direct executable filepath "connection string" "sql query" "credentials" "target url" "output directory" "output exception directory" "namespace"


rem [1] csv data
rem [2] sql query
rem [3] system login for "ADMIN" account (mcase requires admin role for exporting data)
rem [4] mcase url
rem [5] output directory
rem [6] exception directory
rem [7] namespace

rem (6 & 7) will be created if none exist

Direct executable filepath "csv data" "credentials" "target url" "output directory" "output exception directory" "namespace"

pause