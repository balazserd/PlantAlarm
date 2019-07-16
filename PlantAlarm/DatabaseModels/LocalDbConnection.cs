using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;

namespace PlantAlarm.DatabaseModels
{
    public class LocalDbConnection
    { 
        private SQLiteAsyncConnection db;
        public SQLiteAsyncConnection Db
        {
            get => db;
            private set => db = value;
        }

        public LocalDbConnection(string dbPath)
        {
            //if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantAlarmSQLite.db3")))
            if (false)
            {
                db = new SQLiteAsyncConnection(dbPath);
            }
            else
            {
                db = new SQLiteAsyncConnection(dbPath);

                db.CreateTableAsync<Plant>().Wait();
                db.CreateTableAsync<PlantPhoto>().Wait();
                db.CreateTableAsync<PlantCategory>().Wait();
                db.CreateTableAsync<Accessory>().Wait();
                db.CreateTableAsync<AccessoryPhoto>().Wait();
                db.CreateTableAsync<AccessoryCategory>().Wait();
                db.CreateTableAsync<PlantTask>().Wait();
                db.CreateTableAsync<PlantActivityItem>().Wait();
            }
        }
    }
}
