# Building and packaging / การ build และจัดชุด

## English

Use Windows x64 with .NET SDK **10.0.404** (or a compatible 10.0 patch accepted by `global.json`). The project pins .NET runtime **10.0.15**, LibVLCSharp **3.10.2**, DirectShowLib.Standard **2.1.0**, and NAudio.Wasapi/Core **2.2.1**. NuGet dependencies are locked in `src/packages.lock.json`.

Run `./build.ps1` from PowerShell. Its first run downloads the official VLC **3.0.24** Windows x64 ZIP, checks SHA-256, and copies its native libraries, plugins and notices into `native/`. Then it publishes a self-contained Windows x64 application to `publish/` and copies the public guides/licenses. No system installation is performed.

The VLC ZIP SHA-256 is:

```text
fcf30850371ad10c9373cc4f0f4501e7dee49e3e9ae9f20c72fb2661a1ca6323
```

Do not substitute the LGPL-only `VideoLAN.LibVLC.Windows` NuGet build: the previously tested package lacked the DirectShow access plugin needed here. The bundled official native build includes GPL plugins, and the application is distributed as GPL-2.0-or-later with corresponding sources.

Public publishing sets deterministic compilation, maps source paths to `/_/src`, and disables debug symbols. Never add a local `publish/`, `bin/`, `obj/`, `native/`, settings, logs, test captures, shortcut or pre-existing private output archive to Git. Build artifacts belong in Releases, not source history.

To reproduce the public package, build this tagged source, include `LICENSE`, `THIRD_PARTY_NOTICES.md`, `licenses/` and both guides, then archive the **whole** `publish/` folder. Include SHA-256 checksums using release filenames only. Supply the accompanying corresponding-source archives when redistributing GPL/LGPL components. The app's dynamic libraries remain replaceable; no signing/DRM mechanism prevents compatible replacements.

Hardware playback is not part of the build script. Optional developer diagnostics in `Program.cs` accept a synthetic local clip or device inspection; their output is private local data, not material to publish. The icon-only and public-packaging updates were checked without reopening capture devices.

## ภาษาไทย

ใช้ Windows x64 และ .NET SDK **10.0.404** (หรือ patch 10.0 ที่ `global.json` ยอมรับ) โปรเจกต์ตรึง .NET runtime **10.0.15**, LibVLCSharp **3.10.2**, DirectShowLib.Standard **2.1.0** และ NAudio.Wasapi/Core **2.2.1** มี lock file ที่ `src/packages.lock.json`

รัน `./build.ps1` ใน PowerShell ครั้งแรกสคริปต์ดาวน์โหลด ZIP Windows x64 ของ VLC **3.0.24** ทางการ ตรวจ SHA-256 ด้านบน แล้ววางไลบรารี native, plugins และ notices ใน `native/` จากนั้น publish แอป Windows x64 แบบ self-contained ไป `publish/` พร้อมคู่มือ/สิทธิ์ใช้งานสาธารณะ ไม่ติดตั้งลงระบบ

อย่าสลับเป็นแพ็กเกจ NuGet `VideoLAN.LibVLC.Windows` แบบ LGPL-only โดยไม่ตรวจ เพราะแพ็กเกจที่เคยทดลองไม่มี DirectShow access plugin ที่ต้องใช้ ชุด native ทางการที่รวมมามี GPL plugins จึงแจกแอปแบบ GPL-2.0-or-later พร้อม corresponding sources

ชุด public ใช้ deterministic compilation, source path กลาง `/_/src` และไม่สร้าง debug symbols ห้ามนำ `publish/`, `bin/`, `obj/`, `native/`, settings, log, ไฟล์แคปเจอร์, shortcut หรือ archive ส่วนตัวเดิมเข้า Git ไบนารีให้อัปโหลดใน Releases

การทำชุด public ซ้ำให้ build จาก tag นี้ รวม `LICENSE`, `THIRD_PARTY_NOTICES.md`, `licenses/` และคู่มือทั้งสองภาษา แล้ว ZIP **ทั้ง** โฟลเดอร์ `publish/` ทำ SHA-256 โดยใช้ชื่อไฟล์ Release เท่านั้น และแจก corresponding-source archives คู่กับส่วน GPL/LGPL สามารถเปลี่ยนไลบรารี dynamic เป็นรุ่นที่เข้ากันได้ ไม่มี DRM หรือการบังคับลงนามขวางไว้

สคริปต์ build ไม่เปิดอุปกรณ์แคปเจอร์ โค้ดมี diagnostic สำหรับ synthetic clip/inspect อุปกรณ์ แต่ข้อมูลที่ออกมาเป็นข้อมูลในเครื่อง ไม่ควรเผยแพร่ การแก้เฉพาะไอคอนและแพ็กเกจ public ตรวจโดยไม่เปิด capture ซ้ำ
