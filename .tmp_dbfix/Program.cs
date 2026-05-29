using MySqlConnector;

static async Task<int> CountAsync(MySqlConnection connection, string sql)
{
    await using var command = new MySqlCommand(sql, connection);
    return Convert.ToInt32(await command.ExecuteScalarAsync());
}

static async Task ExecAsync(MySqlConnection connection, string sql)
{
    await using var command = new MySqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();
}

var connectionString = "Server=localhost;Port=3306;Database=khct;User=root;Password=rootpass;SslMode=None;";
await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();

if (await CountAsync(connection, "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='khct' AND TABLE_NAME='task' AND COLUMN_NAME='approval_status'") == 0)
{
    await ExecAsync(connection, "ALTER TABLE task ADD COLUMN approval_status varchar(32) NOT NULL DEFAULT 'Draft'");
}

if (await CountAsync(connection, "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='khct' AND TABLE_NAME='task' AND COLUMN_NAME='submitted_at'") == 0)
{
    await ExecAsync(connection, "ALTER TABLE task ADD COLUMN submitted_at datetime(6) NULL");
}

if (await CountAsync(connection, "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA='khct' AND TABLE_NAME='task' AND COLUMN_NAME='approved_at'") == 0)
{
    await ExecAsync(connection, "ALTER TABLE task ADD COLUMN approved_at datetime(6) NULL");
}

if (await CountAsync(connection, "SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA='khct' AND TABLE_NAME='task' AND INDEX_NAME='ix_task_approval_status'") == 0)
{
    await ExecAsync(connection, "CREATE INDEX ix_task_approval_status ON task(approval_status)");
}

if (await CountAsync(connection, "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='khct' AND TABLE_NAME='task_approval_history'") == 0)
{
    await ExecAsync(connection, """
        CREATE TABLE task_approval_history (
          id char(36) COLLATE ascii_general_ci NOT NULL,
          task_id char(36) COLLATE ascii_general_ci NOT NULL,
          department_id char(36) COLLATE ascii_general_ci NULL,
          action varchar(32) NOT NULL,
          from_status varchar(32) NOT NULL,
          to_status varchar(32) NOT NULL,
          actor_user_id char(36) COLLATE ascii_general_ci NOT NULL,
          comment varchar(2000) NULL,
          created_at datetime(6) NOT NULL,
          updated_at datetime(6) NOT NULL,
          PRIMARY KEY (id),
          KEY ix_task_approval_history_task_id (task_id),
          KEY ix_task_approval_history_department_id (department_id),
          KEY ix_task_approval_history_actor_user_id (actor_user_id),
          CONSTRAINT fk_task_approval_history_task_task_id FOREIGN KEY (task_id) REFERENCES task(id) ON DELETE CASCADE,
          CONSTRAINT fk_task_approval_history_department_department_id FOREIGN KEY (department_id) REFERENCES department(id) ON DELETE RESTRICT,
          CONSTRAINT fk_task_approval_history_user_actor_user_id FOREIGN KEY (actor_user_id) REFERENCES app_user(id) ON DELETE RESTRICT
        ) CHARACTER SET utf8mb4
        """);
}

await ExecAsync(connection, """
    UPDATE task t
    INNER JOIN plan p ON p.id = t.plan_id
    INNER JOIN department d ON d.id = t.owner_department_id
    SET t.approval_status = 'Draft',
        t.submitted_at = NULL,
        t.approved_at = NULL
    WHERE p.scope = 'Main'
      AND p.year = 2026
      AND p.month = 5
      AND d.code = 'KTNB1'
    """);

await ExecAsync(connection, """
    DELETE tah
    FROM task_approval_history tah
    INNER JOIN task t ON t.id = tah.task_id
    INNER JOIN plan p ON p.id = t.plan_id
    INNER JOIN department d ON d.id = t.owner_department_id
    WHERE p.scope = 'Main'
      AND p.year = 2026
      AND p.month = 5
      AND d.code = 'KTNB1'
    """);

Console.WriteLine("schema_ok");
