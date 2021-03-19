# feature-usage-service
A feature usage service that saves a description of feature usage per-user and also metadata about those features.

## Bulk Write Notes
Bulk Write Async seems to start off a task without waiting for it to finish (even though you are awaiting that task).  
When an app is closing and your initiating a bulk write, the app will not wait for the bulk write to finish and will abruptly close your mongo connection.
There must be a way to stop this from happening and wait for the writes to finish, I haven't had time yet to look.