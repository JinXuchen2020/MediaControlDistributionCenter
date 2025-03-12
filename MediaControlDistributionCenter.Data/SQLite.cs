using MediaControlDistributionCenter.Data.Entity;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediaControlDistributionCenter.Data
{
    public static class SQLite
    {
        private static readonly string _connStr = "Data Source=DebugData.db;";
        private static SqlSugarClient _db;

        /// <summary>
        /// 初始化启动 数据库服务
        /// </summary>
        public static void InitServer()
        {
            // 初始化SqlSugarClient
            _db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = _connStr,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute // 使用属性方式
            });
        }

        public static void InitTables()
        {
            var types = typeof(BaseModel).Assembly.DefinedTypes.Where(c => c.BaseType == typeof(BaseModel));

            foreach (var type in types)
            {
                var createTable = typeof(SQLite).GetMethod("CreateTable")!;
                createTable = createTable.MakeGenericMethod(type);
                createTable.Invoke(null, null);
            }

            InserTable(new User()
            {
                Account = "admin",
                Password = "123456",
                Company = "山木时代",
                Contact = "12345678907",
                Email = "1214@164.com",
                Role = "admin",
                Status = 1
            });
        }

        /// <summary>
        /// 检查数据库是否存在
        /// </summary>
        /// <returns></returns>
        public static bool CheckDatabaseExists()
        {
            try
            {
                _db.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool CheckTableExists(string tableName)
        {
            return _db.DbMaintenance.IsAnyTable(tableName, false);
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CreateTable<T>() where T : class, new()
        {
            if (!CheckTableExists(typeof(T).Name + "s"))
            {
                _db.CodeFirst.InitTables<T>();
            }
            else
            {
                _db.DbMaintenance.DropTable(typeof(T).Name + "s");
                _db.CodeFirst.InitTables<T>();
            }
        }
        /// <summary>
        /// 查询表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISugarQueryable<T> QueryTable<T>() where T : class, new()
        {
            return _db.Queryable<T>();
        }

        /// <summary>
        /// 插入表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int InserTable<T>(T data) where T : class, new()
        {
            return _db.Insertable<T>(data).ExecuteReturnIdentity();
        }


        /// <summary>
        /// 批量插入表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">要插入的数据列表</param>
        /// <returns>插入成功的行数</returns>
        public static int InsertTableBatch<T>(List<T> list) where T : class, new()
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("数据列表不能为空");
            }
            return _db.Insertable(list).ExecuteCommand();
        }

         
        /// <summary>
        /// 更新表中的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateTable<T>(T data) where T : class, new()
        {
            return _db.Updateable(data).ExecuteCommand() > 0;
        }


        /// <summary>
        /// 删除表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool DeleTeable<T>(T data) where T : class, new()
        {
            return _db.Deleteable<T>(data).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 根据id删除表中的记录
        /// </summary>
        /// <typeparam name="T">表映射的实体类</typeparam>
        /// <param name="id">要删除记录的id</param>
        /// <returns>删除操作是否成功</returns>
        public static bool DeleteById<T>(object id) where T : class, new()
        {
            // 这里假设T类型有一个属性Id，它对应数据库表的主键
            return _db.Deleteable<T>(id).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 根据ID列表批量删除表中的记录
        /// </summary>
        /// <typeparam name="T">表映射的实体类</typeparam>
        /// <param name="ids">要删除记录的ID列表</param>
        /// <returns>删除操作影响的行数</returns>
        public static int DeleteByIds<T>(List<object> ids) where T : class, new()
        {
            if (ids == null || ids.Count == 0)
            {
                throw new ArgumentException("ID列表不能为空");
            }
            return _db.Deleteable<T>(ids).ExecuteCommand();
        }
    }
}
