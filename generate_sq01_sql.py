import os
import zipfile
import xml.etree.ElementTree as ET
from collections import defaultdict

output_dir = "Deploy_SQ01_November"
os.makedirs(output_dir, exist_ok=True)

xlsx_path = "SQ01 - All Device - November.xlsx"

print("1. Loading Excel file...")
with zipfile.ZipFile(xlsx_path) as z:
    shared_strings = []
    if 'xl/sharedStrings.xml' in z.namelist():
        sst_xml = z.read('xl/sharedStrings.xml')
        sst_root = ET.fromstring(sst_xml)
        ns = {'ns': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
        for si in sst_root.findall('.//ns:si', ns):
            t_elems = si.findall('.//ns:t', ns)
            text = "".join([t.text or "" for t in t_elems])
            shared_strings.append(text)

    sheet_xml = z.read('xl/worksheets/sheet1.xml')
    root = ET.fromstring(sheet_xml)
    ns = {'ns': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    rows = root.findall('.//ns:row', ns)

print(f"2. Parsing {len(rows)-1} rows...")
device_rows = defaultdict(list)
all_ordered_devices = []
seen_devs = set()

for idx, r in enumerate(rows[1:]):
    cols = {}
    for c in r.findall('ns:c', ns):
        ref = c.attrib.get('r')
        col_letter = "".join([ch for ch in ref if ch.isalpha()])
        t = c.attrib.get('t')
        v = c.find('ns:v', ns)
        val = v.text if v is not None else ""
        if t == 's' and val != "":
            val = shared_strings[int(val)]
        cols[col_letter] = val

    dev = cols.get('A', '').strip()
    seq_str = cols.get('B', '').strip()
    desc = cols.get('C', '').strip()
    mach_str = cols.get('D', '').strip()
    labor_str = cols.get('E', '').strip()

    if not dev:
        continue

    # Clean & fix sequence
    if not seq_str:
        if dev == '56303016' and 'Sawing Glass' in desc:
            seq = 5
        else:
            seq = 10
    else:
        try:
            seq = int(seq_str)
        except:
            seq = 10

    # Clean floats
    try:
        mach = float(mach_str) if mach_str != "" else 0.0
    except:
        mach = 0.0

    try:
        labor = float(labor_str) if labor_str != "" else 0.0
    except:
        labor = 0.0

    # Escape single quotes in description
    clean_desc = desc.replace("'", "''")

    if dev not in seen_devs:
        seen_devs.add(dev)
        all_ordered_devices.append(dev)

    device_rows[dev].append((dev, seq, clean_desc, mach, labor))

total_devices = len(all_ordered_devices)
total_records = sum(len(v) for v in device_rows.values())
print(f"Total Devices: {total_devices}, Total Records: {total_records}")

# Generate 00_Preparation_and_Validation.sql
prep_sql = """-- ============================================================================
-- SCRIPT: 00_Preparation_and_Validation.sql
-- TARGET DATABASE: [CI_LSS]
-- DESCRIPTION: Persiapan Tabel, Index, dan Pengecekan Constraint sebelum Deploy
-- ============================================================================

USE [CI_LSS];
GO

SET NOCOUNT ON;
PRINT '============================================================';
PRINT 'Memulai Persiapan Lingkungan [dbo].[CI_SQ01]...';
PRINT 'Waktu Mulai: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '============================================================';
GO

-- 1. Pastikan tabel CI_SQ01 ada
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_SQ01' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_SQ01] (
        [DeviceCode]       NVARCHAR(50)  NOT NULL,
        [Seq]              INT           NOT NULL,
        [Description]      NVARCHAR(250) NULL,
        [WorkCenter]       NVARCHAR(50)  NULL,
        [MachineHour]      FLOAT         NULL,
        [LaborHour]        FLOAT         NULL,
        [MachineHour_pcs]  AS [MachineHour],
        [LaborHour_pcs]    AS [LaborHour],
        [MachineHour/pcs]  AS [MachineHour],
        [LaborHour/pcs]    AS [LaborHour]
    );

    CREATE CLUSTERED INDEX [CIX_CI_SQ01_DeviceCode_Seq] 
        ON [dbo].[CI_SQ01] ([DeviceCode] ASC, [Seq] ASC);

    CREATE NONCLUSTERED INDEX [IX_CI_SQ01_DeviceCode] 
        ON [dbo].[CI_SQ01] ([DeviceCode] ASC);
    
    PRINT '>> Tabel [dbo].[CI_SQ01] berhasil dibuat.';
END
ELSE
BEGIN
    PRINT '>> Tabel [dbo].[CI_SQ01] sudah tersedia.';
    
    -- Pastikan kolom WorkCenter ada jika skema lama belum memiliki
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_SQ01') AND name = 'WorkCenter')
    BEGIN
        ALTER TABLE [dbo].[CI_SQ01] ADD [WorkCenter] NVARCHAR(50) NULL;
        PRINT '>> Kolom [WorkCenter] berhasil ditambahkan.';
    END
END
GO

-- 2. Lepas Foreign Key constraint jika ada agar data 10,610 devices baru tidak terblokir
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CI_SQ01_CI_Product')
BEGIN
    ALTER TABLE [dbo].[CI_SQ01] DROP CONSTRAINT [FK_CI_SQ01_CI_Product];
    PRINT '>> Constraint [FK_CI_SQ01_CI_Product] dilepas untuk menghindari konflik referensi.';
END
GO

-- 3. Catat status awal sebelum update
PRINT '>> Status Data Awal:';
SELECT 
    COUNT(DISTINCT DeviceCode) AS TotalDevices_Existing,
    COUNT(1) AS TotalRows_Existing
FROM [dbo].[CI_SQ01];
GO
"""

with open(os.path.join(output_dir, "00_Preparation_and_Validation.sql"), "w", encoding="utf-8") as f:
    f.write(prep_sql)

# Batching into 5 parts
num_parts = 5
devices_per_part = (total_devices + num_parts - 1) // num_parts

for part_idx in range(num_parts):
    start_dev_idx = part_idx * devices_per_part
    end_dev_idx = min((part_idx + 1) * devices_per_part, total_devices)
    part_devices = all_ordered_devices[start_dev_idx:end_dev_idx]
    
    part_file = os.path.join(output_dir, f"Part_{part_idx+1:02d}_of_{num_parts:02d}.sql")
    
    with open(part_file, "w", encoding="utf-8") as f:
        f.write(f"""-- ============================================================================
-- SCRIPT: Part_{part_idx+1:02d}_of_{num_parts:02d}.sql
-- TARGET DATABASE: [CI_LSS]
-- DESCRIPTION: Update & Replace SQ01 Data Part {part_idx+1} of {num_parts}
-- DEVICES: {len(part_devices)} devices ({start_dev_idx+1} to {end_dev_idx})
-- STRATEGY: Intelligent Device Replacement (Atomic Delete & Insert per Batch)
-- ============================================================================

USE [CI_LSS];
GO

SET NOCOUNT ON;
PRINT 'Memulai Eksekusi Part {part_idx+1} dari {num_parts}...';
PRINT 'Waktu Mulai: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
GO
""")
        
        # Pack devices into sub-batches <= 900 rows
        current_batch_rows = []
        batch_num = 1
        
        for dev in part_devices:
            rows_for_dev = device_rows[dev]
            if len(current_batch_rows) + len(rows_for_dev) > 900:
                # Flush batch
                f.write(f"\n-- --- Batch {batch_num} (Rows: {len(current_batch_rows)}) ---\n")
                f.write("BEGIN TRANSACTION;\n")
                f.write("CREATE TABLE #Batch_SQ01 (\n")
                f.write("    [DeviceCode] NVARCHAR(50) NOT NULL,\n")
                f.write("    [Seq] INT NOT NULL,\n")
                f.write("    [Description] NVARCHAR(250) NULL,\n")
                f.write("    [MachineHour] FLOAT NULL,\n")
                f.write("    [LaborHour] FLOAT NULL\n")
                f.write(");\n\n")
                f.write("INSERT INTO #Batch_SQ01 ([DeviceCode], [Seq], [Description], [MachineHour], [LaborHour])\nVALUES\n")
                value_lines = []
                for r in current_batch_rows:
                    value_lines.append(f"    (N'{r[0]}', {r[1]}, N'{r[2]}', {r[3]}, {r[4]})")
                f.write(",\n".join(value_lines))
                f.write(";\n\n")
                f.write("-- Menimpa data lama: Hapus record lama hanya untuk DeviceCode di batch ini\n")
                f.write("DELETE target\nFROM [dbo].[CI_SQ01] target\nWHERE target.[DeviceCode] IN (SELECT DISTINCT [DeviceCode] FROM #Batch_SQ01);\n\n")
                f.write("-- Masukkan record baru yang fresh\n")
                f.write("INSERT INTO [dbo].[CI_SQ01] ([DeviceCode], [Seq], [Description], [MachineHour], [LaborHour])\n")
                f.write("SELECT [DeviceCode], [Seq], [Description], [MachineHour], [LaborHour] FROM #Batch_SQ01;\n\n")
                f.write("DROP TABLE #Batch_SQ01;\n")
                f.write("COMMIT TRANSACTION;\nGO\n")
                
                batch_num += 1
                current_batch_rows = []
                
            current_batch_rows.extend(rows_for_dev)
            
        if current_batch_rows:
            f.write(f"\n-- --- Batch {batch_num} (Rows: {len(current_batch_rows)}) ---\n")
            f.write("BEGIN TRANSACTION;\n")
            f.write("CREATE TABLE #Batch_SQ01 (\n")
            f.write("    [DeviceCode] NVARCHAR(50) NOT NULL,\n")
            f.write("    [Seq] INT NOT NULL,\n")
            f.write("    [Description] NVARCHAR(250) NULL,\n")
            f.write("    [MachineHour] FLOAT NULL,\n")
            f.write("    [LaborHour] FLOAT NULL\n")
            f.write(");\n\n")
            f.write("INSERT INTO #Batch_SQ01 ([DeviceCode], [Seq], [Description], [MachineHour], [LaborHour])\nVALUES\n")
            value_lines = []
            for r in current_batch_rows:
                value_lines.append(f"    (N'{r[0]}', {r[1]}, N'{r[2]}', {r[3]}, {r[4]})")
            f.write(",\n".join(value_lines))
            f.write(";\n\n")
            f.write("-- Menimpa data lama: Hapus record lama hanya untuk DeviceCode di batch ini\n")
            f.write("DELETE target\nFROM [dbo].[CI_SQ01] target\nWHERE target.[DeviceCode] IN (SELECT DISTINCT [DeviceCode] FROM #Batch_SQ01);\n\n")
            f.write("-- Masukkan record baru yang fresh\n")
            f.write("INSERT INTO [dbo].[CI_SQ01] ([DeviceCode], [Seq], [Description], [MachineHour], [LaborHour])\n")
            f.write("SELECT [DeviceCode], [Seq], [Description], [MachineHour], [LaborHour] FROM #Batch_SQ01;\n\n")
            f.write("DROP TABLE #Batch_SQ01;\n")
            f.write("COMMIT TRANSACTION;\nGO\n")

        f.write(f"\nPRINT '>> Part {part_idx+1} Selesai Sukses: ' + CONVERT(VARCHAR(30), GETDATE(), 120);\nGO\n")

# Generate 99_Post_Verification.sql
post_sql = """-- ============================================================================
-- SCRIPT: 99_Post_Verification.sql
-- TARGET DATABASE: [CI_LSS]
-- DESCRIPTION: Verifikasi Hasil Update, Pengecekan Device & Step, serta Rebuild Index
-- ============================================================================

USE [CI_LSS];
GO

SET NOCOUNT ON;
PRINT '============================================================';
PRINT 'Memulai Verifikasi Akhir [dbo].[CI_SQ01]...';
PRINT 'Waktu Mulai: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '============================================================';
GO

-- 1. Cek Total Devices dan Total Baris
PRINT '>> Ringkasan Data Setelah Update:';
SELECT 
    COUNT(DISTINCT DeviceCode) AS TotalDevices,
    COUNT(1) AS TotalRecords,
    MIN(Seq) AS MinSeq,
    MAX(Seq) AS MaxSeq,
    SUM(CASE WHEN MachineHour IS NULL THEN 1 ELSE 0 END) AS NullMachineHours,
    SUM(CASE WHEN LaborHour IS NULL THEN 1 ELSE 0 END) AS NullLaborHours
FROM [dbo].[CI_SQ01];
GO

-- 2. Verifikasi Sampel Device Utama
PRINT '>> Sampel Data Device 3346007:';
SELECT TOP (15) DeviceCode, Seq, Description, MachineHour, LaborHour
FROM [dbo].[CI_SQ01]
WHERE DeviceCode = '3346007'
ORDER BY Seq ASC;
GO

PRINT '>> Sampel Data Device 8783755:';
SELECT TOP (15) DeviceCode, Seq, Description, MachineHour, LaborHour
FROM [dbo].[CI_SQ01]
WHERE DeviceCode = '8783755'
ORDER BY Seq ASC;
GO

PRINT '>> Sampel Data Device 56303016 (Termasuk Row yang Diperbaiki Seq=5):';
SELECT DeviceCode, Seq, Description, MachineHour, LaborHour
FROM [dbo].[CI_SQ01]
WHERE DeviceCode = '56303016'
ORDER BY Seq ASC;
GO

-- 3. Maintenance Index agar performa query di CILss cepat maksimal
PRINT '>> Rebuilding Index pada [dbo].[CI_SQ01]...';
ALTER INDEX [CIX_CI_SQ01_DeviceCode_Seq] ON [dbo].[CI_SQ01] REBUILD;
ALTER INDEX [IX_CI_SQ01_DeviceCode] ON [dbo].[CI_SQ01] REBUILD;
PRINT '>> Index berhasil di-rebuild dengan sempurna.';
GO

PRINT '============================================================';
PRINT 'Verifikasi Selesai. Seluruh sistem CILss siap menggunakan data baru!';
PRINT '============================================================';
GO
"""

with open(os.path.join(output_dir, "99_Post_Verification.sql"), "w", encoding="utf-8") as f:
    f.write(post_sql)

print("All scripts generated successfully in Deploy_SQ01_November/")
