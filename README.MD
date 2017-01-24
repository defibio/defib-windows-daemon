### Defib Windows Daemon

## Please note that application is still under development, and that you may encounter bugs

In order to start the deamon, please build the solution in `Debug` mode, with `Any CPU`.  
  
In order to access the web administration panel, please create a User programmatically or by editing the SQLite database  
  
Passwords are hashed with SHA256 with the following patern: `CONCAT(password, hash, username)`