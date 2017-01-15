using Defib.Entity;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Defib
{
    public static class Database
    {
        public static SQLiteConnection Connection;

        public static void Initialize()
        {
            bool buildTables = false;

            // Do we have a database yet? If not create one and indicate that we need to build tables
            if (!File.Exists("storage.sqlite"))
            {
                SQLiteConnection.CreateFile("storage.sqlite");

                buildTables = true;
            }

            // Open database connection
            Connection = new SQLiteConnection("Data Source=storage.sqlite;Version=3;");
            Connection.Open();

            // Build database tables
            if (buildTables)
            {
                string buildHeartbeatSql =
                    "CREATE TABLE heartbeats (id INT, name VARCHAR(255), key VARCHAR(255), interval INT, next_beat INT, last_beat INT, script VARCHAR(255))";
                string buildUserSql =
                    "CREATE TABLE users (id INT, username VARCHAR(255), password VARCHAR(255), salt VARCHAR(255), admin INT)";

                SQLiteCommand buildHeartbeatCommand = new SQLiteCommand(buildHeartbeatSql, Connection);
                buildHeartbeatCommand.ExecuteNonQuery();

                SQLiteCommand buildUserCommand = new SQLiteCommand(buildUserSql, Connection);
                buildUserCommand.ExecuteNonQuery();
            }
        }

        // Used for generating ID's and statistics
        public static int GetCount(string table)
        {
            string entityExistsQuery =
                   "SELECT COUNT(id) FROM {0}";

            SQLiteCommand entityExistsCommand = new SQLiteCommand(String.Format(entityExistsQuery, table), Connection);
            int entityCount = Int32.Parse(entityExistsCommand.ExecuteScalar().ToString());

            return entityCount;
        }

        #region HEARTBEATS

        public static void LoadHeartbeats()
        {
            string entityListQuery =
                "SELECT id FROM heartbeats";

            SQLiteCommand entityListCommand = new SQLiteCommand(entityListQuery, Connection);
            SQLiteDataReader entityListReader = entityListCommand.ExecuteReader();

            List<int> entityList = new List<int>();

            while (entityListReader.Read())
            {
                entityList.Add(Int32.Parse(entityListReader[1].ToString()));
            }

            foreach (int entity in entityList)
            {
                Heartbeat tempHeartbeat = new Heartbeat();
                tempHeartbeat.Id = entity;
                tempHeartbeat = LoadHeartbeat(tempHeartbeat);

                Context.Heartbeats.Add(tempHeartbeat.Id, tempHeartbeat);
            }
        }

        public static Heartbeat LoadHeartbeat(Heartbeat heartbeat)
        {
            string fetchEntityQuery =
                "SELECT * FROM heartbeats WHERE id = @Id";

            SQLiteCommand fetchEntityCommand = new SQLiteCommand(fetchEntityQuery, Connection);
            fetchEntityCommand.Parameters.Add(new SQLiteParameter("@Id", heartbeat.Id));
            SQLiteDataReader fetchEntityReader = fetchEntityCommand.ExecuteReader();

            while (fetchEntityReader.Read())
            {
                heartbeat.Name = fetchEntityReader[1].ToString();
                heartbeat.Key = fetchEntityReader[2].ToString();
                heartbeat.Interval = Int32.Parse(fetchEntityReader[3].ToString());
                heartbeat.NextBeat = Int32.Parse(fetchEntityReader[4].ToString());
                heartbeat.LastBeat = Int32.Parse(fetchEntityReader[5].ToString());
                heartbeat.Script = fetchEntityReader[6].ToString();
            }

            return heartbeat;
        }

        public static void SaveHeartbeat(Heartbeat heartbeat)
        {
            // Check if heartbeat is already known in the database
            string entityExistsQuery =
                "SELECT COUNT(id) FROM heartbeats WHERE id = @Id";

            SQLiteCommand entityExistsCommand = new SQLiteCommand(entityExistsQuery, Connection);
            entityExistsCommand.Parameters.Add(new SQLiteParameter("@Id", heartbeat.Id));
            int entityCount = Int32.Parse(entityExistsCommand.ExecuteScalar().ToString());

            if (entityCount > 0)
            {
                // Update
                string createEntityQuery =
                    "UPDATE users SET name = @Name, key = @Key, interval = @Interval, next_beat = @NextBeat, last_beat = @LastBeat, script = @Script WHERE id = @Id";
                SQLiteCommand createEntityCommand = new SQLiteCommand(createEntityQuery, Connection);
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Id", heartbeat.Id));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Name", heartbeat.Name));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Key", heartbeat.Key));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Interval", heartbeat.Interval));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@NextBeat", heartbeat.NextBeat));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@LastBeat", heartbeat.LastBeat));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Script", heartbeat.Script));
                createEntityCommand.ExecuteNonQuery();
            }
            else
            {
                // Create
                string createEntityQuery =
                    "INSERT INTO heartbeats (id, name, key, interval, next_beat, last_beat, script) VALUES (@Id, @Name, @Key, @Interval, @NextBeat, @LastBeat, @Script)";
                SQLiteCommand createEntityCommand = new SQLiteCommand(createEntityQuery, Connection);
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Id", heartbeat.Id));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Name", heartbeat.Name));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Key", heartbeat.Key));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Interval", heartbeat.Interval));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@NextBeat", heartbeat.NextBeat));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@LastBeat", heartbeat.LastBeat));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Script", heartbeat.Script));
                createEntityCommand.ExecuteNonQuery();
            }
        }

        #endregion

        #region USERS

        public static void LoadUsers()
        {
            string entityListQuery =
                "SELECT id FROM users";

            SQLiteCommand entityListCommand = new SQLiteCommand(entityListQuery, Connection);
            SQLiteDataReader entityListReader = entityListCommand.ExecuteReader();

            List<int> entityList = new List<int>();

            while (entityListReader.Read())
            {
                entityList.Add(Int32.Parse(entityListReader[0].ToString()));
            }

            foreach (int entity in entityList)
            {
                User tempUser = new User();
                tempUser.Id = entity;
                tempUser = LoadUser(tempUser);

                Context.Users.Add(tempUser.Id, tempUser);
            }
        }

        public static int GetUserId(string username)
        {
            string entityFetchQuery =
                   "SELECT id FROM users WHERE username = @Username";
            SQLiteCommand entityFetchCommand = new SQLiteCommand(entityFetchQuery, Connection);
            entityFetchCommand.Parameters.Add(new SQLiteParameter("@Username", username));
            int entityId = Int32.Parse(entityFetchCommand.ExecuteScalar().ToString());

            return entityId;
        }

        public static User ValidateUser(string username, string password)
        {
            User tempUser = new Entity.User();
            tempUser.Id = GetUserId(username);
            tempUser = LoadUser(tempUser);

            if (Utils.HashPassword(password, tempUser.Password, username) == tempUser.Password)
            {
                // TODO: Create token
            }
            else
            {
                tempUser = new Entity.User();
                tempUser.Id = -1;
            }

            return tempUser;
        }

        public static User LoadUser(User user)
        {
            string fetchUserQuery =
                "SELECT * FROM users WHERE id = @Id";

            SQLiteCommand fetchUserCommand = new SQLiteCommand(fetchUserQuery, Connection);
            fetchUserCommand.Parameters.Add(new SQLiteParameter("@Id", user.Id));
            SQLiteDataReader fetchUserReader = fetchUserCommand.ExecuteReader();

            while (fetchUserReader.Read())
            {
                user.Username = fetchUserReader[1].ToString();
                user.Password = fetchUserReader[2].ToString();
                user.Salt = fetchUserReader[3].ToString();
                user.Administrator = Int32.Parse(fetchUserReader[4].ToString()) == 1 ? true : false;
            }

            return user;
        }

        public static void SaveUser(User user)
        {
            // Check if user is already known in the database
            string entityExistsQuery =
                "SELECT COUNT(id) FROM users WHERE id = @Id";

            SQLiteCommand entityExistsCommand = new SQLiteCommand(entityExistsQuery, Connection);
            entityExistsCommand.Parameters.Add(new SQLiteParameter("@Id", user.Id));
            int entityCount = Int32.Parse(entityExistsCommand.ExecuteScalar().ToString());

            if (entityCount > 0)
            {
                // Update
                string createEntityQuery =
                    "UPDATE users SET username = @Username, password = @Password, salt = @Salt, admin = @Admin WHERE id = @Id";
                SQLiteCommand createEntityCommand = new SQLiteCommand(createEntityQuery, Connection);
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Id", user.Id));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Username", user.Username));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Password", user.Password)); // Password should already be hashed here
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Salt", user.Salt));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Admin", user.Administrator ? 1 : 0));
                createEntityCommand.ExecuteNonQuery();
            }
            else
            {
                // Create
                string createEntityQuery =
                    "INSERT INTO users (id, username, password, salt, admin) VALUES (@Id, @Username, @Password, @Salt, @Admin)";
                SQLiteCommand createEntityCommand = new SQLiteCommand(createEntityQuery, Connection);
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Id", user.Id));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Username", user.Username));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Password", user.Password));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Salt", user.Salt));
                createEntityCommand.Parameters.Add(new SQLiteParameter("@Admin", user.Administrator ? 1 : 0));
                createEntityCommand.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
