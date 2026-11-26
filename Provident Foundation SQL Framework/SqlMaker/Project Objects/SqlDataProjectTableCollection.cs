using Provident.Foundations.Collections;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Contains a list of <see cref="SqlDataProjectTable"/> instances indexed by schema and name.
    /// </summary>
    /// <seealso cref="NamedIndexCollection{T}" />
    public sealed class SqlDataProjectTableCollection : NamedIndexCollection<SqlDataProjectTable>
    {
        private readonly Dictionary<string, SqlDataProjectTable> _tablesByIdentity = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets a table by its unqualified name. Ambiguous names throw an exception.
        /// </summary>
        /// <param name="name">The unqualified table name.</param>
        /// <returns>The matching table, or <see langword="null"/> if not found.</returns>
        public override SqlDataProjectTable? this[string name]
        {
            get
            {
                ValidateName(name);
                EnsureIndex();
                return FindByName(name, out _);
            }
            set
            {
                ValidateName(name);
                EnsureIndex();
                SqlDataProjectTable? current = FindByName(name, out int index);
                if (value == null || current == null)
                    return;
                ValidateTable(value);
                EnsureSameIdentity(current, value);
                this[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets a table by its schema qualifier and name.
        /// </summary>
        /// <param name="qualifier">The schema qualifier, or <see langword="null"/> for an unqualified table.</param>
        /// <param name="name">The table name.</param>
        /// <returns>The matching table, or <see langword="null"/> if not found.</returns>
        public SqlDataProjectTable? this[string? qualifier, string name]
        {
            get
            {
                ValidateName(name);
                EnsureIndex();
                return _tablesByIdentity.GetValueOrDefault(GetKey(qualifier, name));
            }
            set
            {
                ValidateName(name);
                EnsureIndex();
                SqlDataProjectTable? current = _tablesByIdentity.GetValueOrDefault(GetKey(qualifier, name));
                if (value == null || current == null)
                    return;
                ValidateTable(value);
                EnsureSameIdentity(current, value);
                this[IndexOf(current)] = value;
            }
        }

        /// <summary>
        /// Gets or sets a table by its zero-based position.
        /// </summary>
        /// <param name="index">The position of the table.</param>
        public new SqlDataProjectTable this[int index]
        {
            get => base[index];
            set
            {
                EnsureIndex();
                ValidateTable(value);
                _ = base[index];
                string newKey = GetName(value);
                for (int i = 0; i < Count; i++)
                {
                    if (i != index && string.Equals(GetName(base[i]), newKey, StringComparison.Ordinal))
                        throw new ArgumentException("A table with the same schema-qualified name already exists.", nameof(value));
                }

                base[index] = value;
                EnsureIndex();
            }
        }

        /// <summary>
        /// Determines whether any table has the specified unqualified name.
        /// </summary>
        /// <param name="name">The unqualified table name.</param>
        /// <returns><see langword="true"/> if at least one match exists.</returns>
        public override bool Contains(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            EnsureIndex();
            return this.Any(table => string.Equals(table.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Determines whether a table with the specified schema-qualified name exists.
        /// </summary>
        /// <param name="qualifier">The schema qualifier, or <see langword="null"/> for an unqualified table.</param>
        /// <param name="name">The table name.</param>
        /// <returns><see langword="true"/> if the table exists.</returns>
        public bool Contains(string? qualifier, string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            EnsureIndex();
            return _tablesByIdentity.ContainsKey(GetKey(qualifier, name));
        }

        /// <summary>
        /// Adds a table, rejecting invalid names and duplicate schema-qualified identities.
        /// </summary>
        /// <param name="item">The table to add.</param>
        public override void Add(SqlDataProjectTable item)
        {
            EnsureIndex();
            ValidateTable(item);
            string key = GetName(item);
            ThrowIfDuplicate(key);
            _tablesByIdentity.Add(key, item);
            base.Add(item);
        }

        /// <summary>
        /// Adds a range atomically, rejecting invalid names and duplicate identities.
        /// </summary>
        /// <param name="collection">The tables to add.</param>
        public override void AddRange(IEnumerable<SqlDataProjectTable> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);
            EnsureIndex();
            List<SqlDataProjectTable> additions = collection.ToList();
            HashSet<string> keys = new(StringComparer.Ordinal);
            foreach (SqlDataProjectTable item in additions)
            {
                ValidateTable(item);
                string key = GetName(item);
                if (_tablesByIdentity.ContainsKey(key) || !keys.Add(key))
                    throw new ArgumentException("The collection contains a duplicate schema-qualified table name.", nameof(collection));
            }
            foreach (SqlDataProjectTable item in additions)
                Add(item);
        }

        /// <summary>
        /// Inserts a table, rejecting invalid names and duplicate schema-qualified identities.
        /// </summary>
        /// <param name="index">The zero-based insertion index.</param>
        /// <param name="item">The table to insert.</param>
        public override void Insert(int index, SqlDataProjectTable item)
        {
            EnsureIndex();
            ValidateTable(item);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            string key = GetName(item);
            ThrowIfDuplicate(key);
            _tablesByIdentity.Add(key, item);
            base.Insert(index, item);
        }

        /// <summary>
        /// Inserts a range atomically, rejecting invalid names and duplicate identities.
        /// </summary>
        /// <param name="index">The zero-based insertion index.</param>
        /// <param name="collection">The tables to insert.</param>
        public new void InsertRange(int index, IEnumerable<SqlDataProjectTable> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);
            EnsureIndex();
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            List<SqlDataProjectTable> additions = collection.ToList();
            HashSet<string> keys = new(StringComparer.Ordinal);
            foreach (SqlDataProjectTable item in additions)
            {
                ValidateTable(item);
                string key = GetName(item);
                if (_tablesByIdentity.ContainsKey(key) || !keys.Add(key))
                    throw new ArgumentException("The collection contains a duplicate schema-qualified table name.", nameof(collection));
            }
            for (int i = 0; i < additions.Count; i++)
                Insert(index + i, additions[i]);
        }

        /// <summary>
        /// Removes a table by unqualified name. Ambiguous names throw an exception.
        /// </summary>
        /// <param name="name">The unqualified table name.</param>
        public override void Remove(string name)
        {
            ValidateName(name);
            EnsureIndex();
            _ = FindByName(name, out int index);
            if (index >= 0)
                RemoveAt(index);
        }

        /// <summary>
        /// Removes a table by schema-qualified name.
        /// </summary>
        /// <param name="qualifier">The schema qualifier, or <see langword="null"/> for an unqualified table.</param>
        /// <param name="name">The table name.</param>
        public void Remove(string? qualifier, string name)
        {
            ValidateName(name);
            EnsureIndex();
            if (_tablesByIdentity.TryGetValue(GetKey(qualifier, name), out SqlDataProjectTable? item))
                RemoveAt(IndexOf(item));
        }

        /// <summary>
        /// Removes the specified table instance if it is in the collection.
        /// </summary>
        /// <param name="item">The table to remove.</param>
        public override void Remove(SqlDataProjectTable? item)
        {
            if (item == null)
                return;
            EnsureIndex();
            int index = IndexOf(item);
            if (index >= 0)
                RemoveAt(index);
        }

        /// <summary>
        /// Removes a table at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        public override void RemoveAt(int index)
        {
            EnsureIndex();
            if (index < 0 || index >= Count)
                return;
            RemoveAtFromList(index);
            EnsureIndex();
        }

        /// <summary>
        /// Removes all tables matching a predicate.
        /// </summary>
        /// <param name="match">The predicate to match.</param>
        public override void RemoveAll(Predicate<SqlDataProjectTable> match)
        {
            ArgumentNullException.ThrowIfNull(match);
            EnsureIndex();
            base.RemoveAll(match);
            EnsureIndex();
        }

        /// <summary>
        /// Removes a range of tables.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of tables to remove.</param>
        public override void RemoveRange(int index, int count)
        {
            EnsureIndex();
            base.RemoveRange(index, count);
            EnsureIndex();
        }

        /// <summary>
        /// Removes all tables.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _tablesByIdentity.Clear();
        }

        /// <summary>
        /// Gets the internal key for the specified table.
        /// </summary>
        /// <param name="item">The table to identify.</param>
        /// <returns>A collision-safe key for its qualifier and name.</returns>
        protected override string GetName(SqlDataProjectTable item)
        {
            return GetKey(item.Qualifier, item.Name);
        }

        private void EnsureIndex()
        {
            Dictionary<string, SqlDataProjectTable> rebuilt = new(StringComparer.Ordinal);
            foreach (SqlDataProjectTable table in this)
            {
                ValidateTable(table);
                string key = GetName(table);
                if (!rebuilt.TryAdd(key, table))
                    throw new InvalidOperationException("The collection contains duplicate schema-qualified table names.");
            }

            RebuildNameIndex();
            _tablesByIdentity.Clear();
            foreach (KeyValuePair<string, SqlDataProjectTable> pair in rebuilt)
                _tablesByIdentity.Add(pair.Key, pair.Value);
        }

        private void ThrowIfDuplicate(string key)
        {
            if (_tablesByIdentity.ContainsKey(key))
                throw new ArgumentException("A table with the same schema-qualified name already exists.", "item");
        }

        private SqlDataProjectTable? FindByName(string name, out int index)
        {
            SqlDataProjectTable? match = null;
            index = -1;
            for (int i = 0; i < Count; i++)
            {
                if (!string.Equals(base[i].Name, name, StringComparison.Ordinal))
                    continue;
                if (match != null)
                    throw new InvalidOperationException($"Table name '{name}' is ambiguous; use a schema-qualified lookup.");
                match = base[i];
                index = i;
            }
            return match;
        }

        private static void ValidateTable(SqlDataProjectTable? item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateName(item.Name);
        }

        private static void ValidateName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("A table name is required.", nameof(name));
        }

        private static void EnsureSameIdentity(SqlDataProjectTable current, SqlDataProjectTable replacement)
        {
            if (!string.Equals(GetKey(current.Qualifier, current.Name), GetKey(replacement.Qualifier, replacement.Name), StringComparison.Ordinal))
                throw new ArgumentException("A replacement table must have the same schema-qualified identity.", nameof(replacement));
        }

        private static string GetKey(string? qualifier, string? name)
        {
            ValidateName(name);
            qualifier ??= string.Empty;
            return $"{qualifier.Length}:{qualifier}{name}";
        }
    }
}
