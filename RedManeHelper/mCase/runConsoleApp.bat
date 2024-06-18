echo Begining Class Sync
rem [1] path to exe file
rem [2] connection string (Verify that connection string allows access to trusted certificate: TrustServerCertificate=true)
rem [3] sql query
rem [4] system login for "ADMIN" account (mcase requires admin role for exporting data)
rem [5] mcase url
rem [6] output directory
rem [7] exception directory [6 & 7] will be created if none exist
rem (6 & 7) will be created if none exist
rem [8] namespace
Direct executable filepath "connection string" "sql query" "credentials" "target url" "output directory" "output exception directory" "namespace"
