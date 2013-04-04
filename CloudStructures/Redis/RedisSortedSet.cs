﻿using BookSleeve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStructures.Redis
{
    public class RedisSortedSet<T>
    {
        public string Key { get; private set; }
        readonly RedisSettings settings;
        readonly RedisTransaction transaction;
        readonly IRedisValueConverter valueConverter;
        readonly int db;

        public RedisSortedSet(RedisSettings settings, string stringKey)
        {
            this.settings = settings;
            this.db = settings.Db;
            this.valueConverter = settings.ValueConverter;
            this.Key = stringKey;
        }

        public RedisSortedSet(RedisGroup connectionGroup, string stringKey)
            : this(connectionGroup.GetSettings(stringKey), stringKey)
        {
        }

        public RedisSortedSet(RedisTransaction transaction, int db, IRedisValueConverter valueConverter, string stringKey)
        {
            this.transaction = transaction;
            this.db = db;
            this.valueConverter = valueConverter;
            this.Key = stringKey;
        }

        protected RedisConnection Connection
        {
            get
            {
                return (transaction == null) ? settings.GetConnection() : transaction;
            }
        }

        protected ISortedSetCommands Command
        {
            get
            {
                return Connection.SortedSets;
            }
        }

        /// <summary>
        /// ZADD http://redis.io/commands/zadd
        /// </summary>
        Task<bool> Add(T value, double score, bool queueJump = false)
        {
            return Command.Add(db, Key, valueConverter.Serialize(value), score, queueJump);
        }

        /// <summary>
        /// ZCARD http://redis.io/commands/zcard
        /// </summary>
        Task<long> GetLength(bool queueJump = false)
        {
            return Command.GetLength(db, Key, queueJump);
        }

        /// <summary>
        /// ZCOUNT http://redis.io/commands/zcount
        /// </summary>
        Task<long> GetLength(double min, double max, bool queueJump = false)
        {
            return Command.GetLength(db, Key, min, max, queueJump);
        }

        /// <summary>
        /// ZINCRBY http://redis.io/commands/zincrby
        /// </summary>
        Task<double> Increment(T member, double delta, bool queueJump = false)
        {
            return Command.Increment(db, Key, valueConverter.Serialize(member), delta, queueJump);
        }

        /// <summary>
        /// ZINCRBY http://redis.io/commands/zincrby
        /// </summary>
        Task<double>[] Increment(T[] members, double delta, bool queueJump = false)
        {
            var v = members.Select(x => valueConverter.Serialize(x)).ToArray();
            return Command.Increment(db, Key, v, delta, queueJump);
        }

        /// <summary>
        /// ZRANGE http://redis.io/commands/zrange
        /// </summary>
        public async Task<KeyValuePair<T, double>[]> Range(long start, long stop, bool ascending = true, bool queueJump = false)
        {
            var v = await Command.Range(db, Key, start, stop, ascending, queueJump).ConfigureAwait(false);
            return v.Select(x => new KeyValuePair<T, double>(valueConverter.Deserialize<T>(x.Key), x.Value)).ToArray();
        }

        /// <summary>
        /// ZRANGEBYSCORE http://redis.io/commands/zrangebyscore
        /// </summary>
        async Task<KeyValuePair<T, double>[]> Range(double min = -1.0 / 0.0, double max = 1.0 / 0.0, bool ascending = true, bool minInclusive = true, bool maxInclusive = true, long offset = 0, long count = 9223372036854775807, bool queueJump = false)
        {
            var v = await Command.Range(db, Key, min, max, ascending, minInclusive, maxInclusive, offset, count, queueJump).ConfigureAwait(false);
            return v.Select(x => new KeyValuePair<T, double>(valueConverter.Deserialize<T>(x.Key), x.Value)).ToArray();
        }

        /// <summary>
        /// ZRANK http://redis.io/commands/zrank
        /// </summary>
        Task<long?> Rank(T member, bool ascending = true, bool queueJump = false)
        {
            return Command.Rank(db, Key, valueConverter.Serialize(member), ascending, queueJump);
        }

        /// <summary>
        /// ZREM http://redis.io/commands/zrem
        /// </summary>
        Task<bool> Remove(T member, bool queueJump = false)
        {
            return Command.Remove(db, Key, valueConverter.Serialize(member), queueJump);
        }


    }
}
