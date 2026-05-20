"""Normalize all GUID columns to uppercase so EF Core's case-sensitive
TEXT comparisons match the rows we created via Python scripts.

EF Core for SQLite stores Guids as uppercase TEXT, but uuid.uuid4() in
Python produces lowercase strings. That mismatch caused HTTP 404 on
the customer detail endpoint (and would silently break any other
EF-side WHERE Id == @guid query).

Run once after any Python seeding.
"""
import sqlite3
import shutil
import os
import sys
from datetime import datetime

DB = r'C:\Users\danga\Downloads\applicationdevelopment\backend\src\VehiclePartsSystem.API\vehicle_parts.db'
if not os.path.exists(DB):
    sys.exit(f'DB not found at {DB}')

# Back up first
backup = f'{DB}.bak-{datetime.now().strftime("%Y%m%d-%H%M%S")}'
shutil.copy2(DB, backup)
print(f'Backup saved to: {backup}')

# Tables with their GUID columns. Excludes __EFMigrationsHistory (not a GUID).
GUID_COLUMNS = {
    'Appointments':         ['Id', 'CustomerId', 'VehicleId'],
    'Notifications':        ['Id', 'UserId', 'RelatedEntityId'],
    'PartRequests':         ['Id', 'CustomerId'],
    'Parts':                ['Id', 'VendorId'],
    'PurchaseInvoiceItems': ['Id', 'PurchaseInvoiceId', 'PartId'],
    'PurchaseInvoices':     ['Id', 'VendorId', 'CreatedByAdminId'],
    'Reviews':              ['Id', 'CustomerId'],
    'SalesInvoiceItems':    ['Id', 'SalesInvoiceId', 'PartId'],
    'SalesInvoices':        ['Id', 'CustomerId', 'StaffId', 'VehicleId'],
    'Users':                ['Id'],
    'Vehicles':             ['Id', 'CustomerId'],
    'Vendors':              ['Id'],
}

con = sqlite3.connect(DB, timeout=10)
cur = con.cursor()

# Disable FK checks during the rewrite so we can update parents + children
cur.execute('PRAGMA foreign_keys = OFF')

total_updated = 0
for table, cols in GUID_COLUMNS.items():
    for col in cols:
        # Skip if column doesn't actually exist (safety)
        col_check = cur.execute(f'PRAGMA table_info({table})').fetchall()
        col_names = [c[1] for c in col_check]
        if col not in col_names:
            continue

        # Count rows that need fixing
        n = cur.execute(
            f'SELECT COUNT(*) FROM {table} WHERE {col} IS NOT NULL AND {col} != upper({col})'
        ).fetchone()[0]
        if n > 0:
            cur.execute(
                f'UPDATE {table} SET {col} = upper({col}) WHERE {col} IS NOT NULL AND {col} != upper({col})'
            )
            print(f'  {table}.{col}: updated {n} rows')
            total_updated += n

con.commit()
cur.execute('PRAGMA foreign_keys = ON')

# Verify FK integrity afterwards
violations = cur.execute('PRAGMA foreign_key_check').fetchall()
if violations:
    print(f'\nWARNING: {len(violations)} FK violations after fix:')
    for v in violations[:10]:
        print(f'  {v}')
else:
    print(f'\nDone. {total_updated} cells updated. FK integrity OK.')

con.close()
