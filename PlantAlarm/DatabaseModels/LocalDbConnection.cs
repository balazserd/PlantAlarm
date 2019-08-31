using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;

namespace PlantAlarm.DatabaseModels
{
    public class LocalDbConnection
    { 
        private SQLiteAsyncConnection asyncDb;
        public SQLiteAsyncConnection AsyncDb
        {
            get => asyncDb;
            private set => asyncDb = value;
        }

        private SQLiteConnection db;
        public SQLiteConnection Db
        {
            get => db;
            private set => db = value;
        }

        public LocalDbConnection(string dbPath)
        {
            //if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantAlarmSQLite.db3")))
            if (false)
            {
                AsyncDb = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                Db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            }
            else
            {
                AsyncDb = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                Db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

                //AsyncDb.DropTableAsync<Plant>().Wait();
                //AsyncDb.DropTableAsync<PlantPhoto>().Wait();
                //AsyncDb.DropTableAsync<PlantCategory>().Wait();
                //AsyncDb.DropTableAsync<PlantCategorization>().Wait();

                //AsyncDb.DropTableAsync<Accessory>().Wait();
                //AsyncDb.DropTableAsync<AccessoryPhoto>().Wait();
                //AsyncDb.DropTableAsync<AccessoryCategory>().Wait();

                //AsyncDb.DropTableAsync<PlantTask>().Wait();
                //AsyncDb.DropTableAsync<PlantActivityItem>().Wait();

                AsyncDb.CreateTableAsync<Plant>().Wait();
                AsyncDb.CreateTableAsync<PlantPhoto>().Wait();
                AsyncDb.CreateTableAsync<PlantCategory>().Wait();
                AsyncDb.CreateTableAsync<PlantCategorization>().Wait();

                AsyncDb.CreateTableAsync<Accessory>().Wait();
                AsyncDb.CreateTableAsync<AccessoryPhoto>().Wait();
                AsyncDb.CreateTableAsync<AccessoryCategory>().Wait();

                AsyncDb.CreateTableAsync<PlantTask>().Wait();
                AsyncDb.CreateTableAsync<PlantTaskPlantConnection>().Wait();
                AsyncDb.CreateTableAsync<PlantActivityItem>().Wait();
            }
        }
    }
}
