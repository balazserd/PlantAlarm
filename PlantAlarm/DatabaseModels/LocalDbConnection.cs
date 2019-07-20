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

                db.DropTableAsync<Plant>().Wait();
                db.DropTableAsync<PlantPhoto>().Wait();
                db.DropTableAsync<PlantCategory>().Wait();
                db.DropTableAsync<PlantCategorization>().Wait();

                db.DropTableAsync<Accessory>().Wait();
                db.DropTableAsync<AccessoryPhoto>().Wait();
                db.DropTableAsync<AccessoryCategory>().Wait();

                db.DropTableAsync<PlantTask>().Wait();
                db.DropTableAsync<PlantActivityItem>().Wait();

                db.CreateTableAsync<Plant>().Wait();
                db.CreateTableAsync<PlantPhoto>().Wait();
                db.CreateTableAsync<PlantCategory>().Wait();
                db.CreateTableAsync<PlantCategorization>().Wait();

                db.CreateTableAsync<Accessory>().Wait();
                db.CreateTableAsync<AccessoryPhoto>().Wait();
                db.CreateTableAsync<AccessoryCategory>().Wait();

                db.CreateTableAsync<PlantTask>().Wait();
                db.CreateTableAsync<PlantActivityItem>().Wait();
            }
        }
    }
}
