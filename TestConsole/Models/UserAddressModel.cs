using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.SqlWriter;

namespace TestDatabaseLibrary
{
    public class User_WithAddresses : UserModel
    {
        public List<AddressModel> Addresses { get; set; } = new List<AddressModel>();

        public static async Task<IEnumerable<User_WithAddresses>> AddAddressesToList(string sql, string splitOn, DynamicParameters p, IDbConnection connection, int? connectionTimeOut)
        {
            using var _ = connection;

            var userDictionary = new Dictionary<ulong, User_WithAddresses>();

            var list = await connection.QueryAsync<User_WithAddresses, AddressModel, User_WithAddresses>(
                map: (user, address) =>
                {
                    if (userDictionary.TryGetValue(user.UserID, out User_WithAddresses? userEntry) == false)
                    {
                        userEntry = user;

                        userDictionary.Add(userEntry.UserID, userEntry);
                    }

                    if (address != null)
                        userEntry.Addresses.Add(address);

                    return userEntry;
                },
                sql: sql, splitOn: splitOn, param: p, commandTimeout: connectionTimeOut);

            return userDictionary.Values;
        }
    }
}
