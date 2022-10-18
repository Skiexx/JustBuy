#!/bin/bash

set -e

until dotnet ef database update; do
>&2 echo "MySQL init process done. Ready for start up."
sleep 1
done

>&2 echo "SQL Server is up - executing command"
exec $run_cmd