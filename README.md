# CILeanSixSigma

Aplikasi web internal untuk mencatat dan memantau training Lean Six Sigma: White Belt, Yellow Belt, Green Belt + TOC, dan Black Belt.

## Prasyarat

- .NET SDK 9.0 atau lebih baru
- SQL Server Express (tested on SQL Server 2022 Express)
- Instance SQL Server: `DESKTOP-ABQLQF1\SQLEXPRESS`

## Menyiapkan Database

Jalankan dari folder repository menggunakan Windows Authentication:

```bash
sqlcmd -S "DESKTOP-ABQLQF1\SQLEXPRESS" -E -i database/schema.sql
```

Script idempotent membuat database `LssTraining`, seluruh tabel, program, dan modul awal. Connection string dapat diubah melalui `LssTraining.Web/appsettings.json` atau environment variable/user secrets.

## Menjalankan Aplikasi

```bash
dotnet run --project LssTraining.Web
```

Buka URL yang dicetak oleh aplikasi (default HTTP `http://localhost:5277`).

## Fitur

- Dashboard KPI, empat training pathway, grafik completion, dan aktivitas terbaru
- Autentikasi cookie berbasis username/password dengan role Admin/Trainer
- CRUD peserta dengan dropdown Status Phase DMAIC (Define, Measure, Analyze, Improve, Control)
- Visual roadmap interaktif dan matriks status review R0–R5
- Import CSV massal peserta
- Master program dan kurikulum modul dengan target durasi
- Progress enrollment dan tracking jam aktual vs target
- Flag TOC pada enrollment Green Belt
