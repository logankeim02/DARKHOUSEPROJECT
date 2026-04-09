using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// SQLite data layer using direct P/Invoke into sqlite3.dll.
/// Does NOT use Mono.Data.Sqlite — avoids Unity 6 IL compatibility issues.
/// Requires only sqlite3.dll in Assets/Plugins/.
/// </summary>
public class SaveDatabase
{
    // ── Native SQLite P/Invoke ───────────────────────────────────────────────

    private static class Native
    {
        private const string Lib = "sqlite3";

        public const int SQLITE_OK   = 0;
        public const int SQLITE_ROW  = 100;
        public const int SQLITE_DONE = 101;

        // Tells SQLite to copy strings before returning — safe for managed strings
        public static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_open(string filename, out IntPtr db);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_close(IntPtr db);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr data, out IntPtr errmsg);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_prepare_v2(IntPtr db, string sql, int nBytes, out IntPtr stmt, IntPtr pzTail);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_step(IntPtr stmt);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_text(IntPtr stmt, int index, string text, int nBytes, IntPtr destructor);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_column_text(IntPtr stmt, int col);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_int(IntPtr stmt, int col);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_errmsg(IntPtr db);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sqlite3_free(IntPtr ptr);
    }

    // ── Fields ───────────────────────────────────────────────────────────────

    private readonly string dbPath;

    public SaveDatabase(string dbPath)
    {
        this.dbPath = dbPath;
        InitializeTables();
    }

    // ── Connection helpers ───────────────────────────────────────────────────

    private IntPtr OpenDb()
    {
        int rc = Native.sqlite3_open(dbPath, out IntPtr db);
        if (rc != Native.SQLITE_OK)
            throw new Exception($"sqlite3_open failed (code {rc})");
        return db;
    }

    private static void CloseDb(IntPtr db)
    {
        if (db != IntPtr.Zero)
            Native.sqlite3_close(db);
    }

    private static void Exec(IntPtr db, string sql)
    {
        int rc = Native.sqlite3_exec(db, sql, IntPtr.Zero, IntPtr.Zero, out IntPtr errmsg);
        if (rc != Native.SQLITE_OK)
        {
            string msg = errmsg != IntPtr.Zero ? Marshal.PtrToStringAnsi(errmsg) : "unknown error";
            Native.sqlite3_free(errmsg);
            throw new Exception($"sqlite3_exec failed: {msg}");
        }
    }

    private static IntPtr Prepare(IntPtr db, string sql)
    {
        int rc = Native.sqlite3_prepare_v2(db, sql, -1, out IntPtr stmt, IntPtr.Zero);
        if (rc != Native.SQLITE_OK)
        {
            string msg = Marshal.PtrToStringAnsi(Native.sqlite3_errmsg(db));
            throw new Exception($"sqlite3_prepare_v2 failed: {msg}");
        }
        return stmt;
    }

    private static void BindText(IntPtr stmt, int index, string value)
        => Native.sqlite3_bind_text(stmt, index, value, -1, Native.SQLITE_TRANSIENT);

    private static void BindInt(IntPtr stmt, int index, int value)
        => Native.sqlite3_bind_int(stmt, index, value);

    private static string ColText(IntPtr stmt, int col)
    {
        IntPtr ptr = Native.sqlite3_column_text(stmt, col);
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
    }

    // ── Schema ───────────────────────────────────────────────────────────────

    private void InitializeTables()
    {
        IntPtr db = OpenDb();
        try
        {
            Exec(db, @"
                CREATE TABLE IF NOT EXISTS save_slots (
                    slot_id       INTEGER PRIMARY KEY,
                    save_name     TEXT    NOT NULL,
                    current_scene TEXT    NOT NULL,
                    saved_at      TEXT    NOT NULL
                );
                CREATE TABLE IF NOT EXISTS inventory_items (
                    slot_id  INTEGER NOT NULL,
                    item_id  TEXT    NOT NULL
                );
                CREATE TABLE IF NOT EXISTS collected_pickups (
                    slot_id   INTEGER NOT NULL,
                    pickup_id TEXT    NOT NULL
                );
                CREATE TABLE IF NOT EXISTS game_flags (
                    slot_id    INTEGER NOT NULL,
                    flag_key   TEXT    NOT NULL,
                    flag_value INTEGER NOT NULL DEFAULT 0,
                    PRIMARY KEY (slot_id, flag_key)
                );
            ");
        }
        finally { CloseDb(db); }
    }

    // ── Existence ────────────────────────────────────────────────────────────

    public bool SlotExists(int slotId)
    {
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT COUNT(*) FROM save_slots WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            bool exists = false;
            if (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                exists = Native.sqlite3_column_int(stmt, 0) > 0;
            Native.sqlite3_finalize(stmt);
            return exists;
        }
        finally { CloseDb(db); }
    }

    // ── Save (Write) ─────────────────────────────────────────────────────────

    public void UpsertSlot(int slotId, string saveName, string currentScene)
    {
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db,
                "INSERT OR REPLACE INTO save_slots (slot_id, save_name, current_scene, saved_at) VALUES (?, ?, ?, ?)");
            BindInt(stmt,  1, slotId);
            BindText(stmt, 2, saveName);
            BindText(stmt, 3, currentScene);
            BindText(stmt, 4, DateTime.UtcNow.ToString("o"));
            Native.sqlite3_step(stmt);
            Native.sqlite3_finalize(stmt);
        }
        finally { CloseDb(db); }
    }

    public void ReplaceInventoryItems(int slotId, IEnumerable<string> itemIds)
    {
        IntPtr db = OpenDb();
        try
        {
            Exec(db, "BEGIN");
            IntPtr del = Prepare(db, "DELETE FROM inventory_items WHERE slot_id = ?");
            BindInt(del, 1, slotId);
            Native.sqlite3_step(del);
            Native.sqlite3_finalize(del);

            foreach (string id in itemIds)
            {
                IntPtr ins = Prepare(db, "INSERT INTO inventory_items (slot_id, item_id) VALUES (?, ?)");
                BindInt(ins,  1, slotId);
                BindText(ins, 2, id);
                Native.sqlite3_step(ins);
                Native.sqlite3_finalize(ins);
            }
            Exec(db, "COMMIT");
        }
        catch (Exception e)
        {
            try { Exec(db, "ROLLBACK"); } catch { }
            Debug.LogError($"[SaveDatabase] ReplaceInventoryItems: {e.Message}");
        }
        finally { CloseDb(db); }
    }

    public void ReplaceCollectedPickups(int slotId, IEnumerable<string> pickupIds)
    {
        IntPtr db = OpenDb();
        try
        {
            Exec(db, "BEGIN");
            IntPtr del = Prepare(db, "DELETE FROM collected_pickups WHERE slot_id = ?");
            BindInt(del, 1, slotId);
            Native.sqlite3_step(del);
            Native.sqlite3_finalize(del);

            foreach (string id in pickupIds)
            {
                IntPtr ins = Prepare(db, "INSERT INTO collected_pickups (slot_id, pickup_id) VALUES (?, ?)");
                BindInt(ins,  1, slotId);
                BindText(ins, 2, id);
                Native.sqlite3_step(ins);
                Native.sqlite3_finalize(ins);
            }
            Exec(db, "COMMIT");
        }
        catch (Exception e)
        {
            try { Exec(db, "ROLLBACK"); } catch { }
            Debug.LogError($"[SaveDatabase] ReplaceCollectedPickups: {e.Message}");
        }
        finally { CloseDb(db); }
    }

    public void SetFlag(int slotId, string key, int value)
    {
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db,
                "INSERT OR REPLACE INTO game_flags (slot_id, flag_key, flag_value) VALUES (?, ?, ?)");
            BindInt(stmt,  1, slotId);
            BindText(stmt, 2, key);
            BindInt(stmt,  3, value);
            Native.sqlite3_step(stmt);
            Native.sqlite3_finalize(stmt);
        }
        finally { CloseDb(db); }
    }

    // ── Load (Read) ──────────────────────────────────────────────────────────

    public string GetCurrentScene(int slotId)
    {
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT current_scene FROM save_slots WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            string result = null;
            if (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                result = ColText(stmt, 0);
            Native.sqlite3_finalize(stmt);
            return result;
        }
        finally { CloseDb(db); }
    }

    public string GetSaveName(int slotId)
    {
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT save_name FROM save_slots WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            string result = null;
            if (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                result = ColText(stmt, 0);
            Native.sqlite3_finalize(stmt);
            return result;
        }
        finally { CloseDb(db); }
    }

    public List<string> GetInventoryItems(int slotId)
    {
        var result = new List<string>();
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT item_id FROM inventory_items WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            while (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                result.Add(ColText(stmt, 0));
            Native.sqlite3_finalize(stmt);
        }
        finally { CloseDb(db); }
        return result;
    }

    public HashSet<string> GetCollectedPickups(int slotId)
    {
        var result = new HashSet<string>();
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT pickup_id FROM collected_pickups WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            while (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                result.Add(ColText(stmt, 0));
            Native.sqlite3_finalize(stmt);
        }
        finally { CloseDb(db); }
        return result;
    }

    public Dictionary<string, int> GetAllFlags(int slotId)
    {
        var result = new Dictionary<string, int>();
        IntPtr db = OpenDb();
        try
        {
            IntPtr stmt = Prepare(db, "SELECT flag_key, flag_value FROM game_flags WHERE slot_id = ?");
            BindInt(stmt, 1, slotId);
            while (Native.sqlite3_step(stmt) == Native.SQLITE_ROW)
                result[ColText(stmt, 0)] = Native.sqlite3_column_int(stmt, 1);
            Native.sqlite3_finalize(stmt);
        }
        finally { CloseDb(db); }
        return result;
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    public void DeleteSlot(int slotId)
    {
        IntPtr db = OpenDb();
        try
        {
            Exec(db, "BEGIN");
            string[] tables = { "save_slots", "inventory_items", "collected_pickups", "game_flags" };
            foreach (string table in tables)
            {
                IntPtr stmt = Prepare(db, $"DELETE FROM {table} WHERE slot_id = ?");
                BindInt(stmt, 1, slotId);
                Native.sqlite3_step(stmt);
                Native.sqlite3_finalize(stmt);
            }
            Exec(db, "COMMIT");
        }
        catch (Exception e)
        {
            try { Exec(db, "ROLLBACK"); } catch { }
            Debug.LogError($"[SaveDatabase] DeleteSlot: {e.Message}");
        }
        finally { CloseDb(db); }
    }
}
