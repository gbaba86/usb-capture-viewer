# Public release review / การตรวจชุดเผยแพร่

Review date: **2026-11-22**. Scope: **v1.0.0-beta.2**, not a professional penetration test or guarantee of safety. This hotfix changes executable branding and download guidance; capture/audio logic and dependency versions are unchanged. The renamed app passed all 19 synthetic integration checks.

## Build and dependencies

- Public build targets **.NET 10.0.15 LTS** using SDK **10.0.404**, obtained from Microsoft's release metadata and verified against its SHA-512. The earlier private .NET 8 runtime is not shipped in this public build because it is out of support.
- Self-contained runtime configuration confirms Microsoft.NETCore.App and Microsoft.WindowsDesktop.App 10.0.15. Debug symbols are disabled; source paths are mapped to a neutral prefix.
- Release publish completed without compiler warnings/errors. The existing synthetic integration self-test passed **19 checks**, covering startup, settings, menu/fullscreen, video/audio decoding, mute/volume and stop/restart. It did not open a capture device.
- NuGet.org's advisory audit reported no known vulnerabilities for the four resolved managed packages at review time. This is not an exhaustive audit of every native library or future advisory.
- Native VLC is the pinned official **3.0.24** Windows x64 build, with SHA-256 verification and matching-source/notice materials. Check upstream security releases before future redistribution; a self-contained app does not receive runtime updates through a global .NET installation.

## Behavior reviewed

- No application code for telemetry/upload, HTTP/TCP listening, remote updates, spawning external processes, admin elevation, registry autorun, services or scheduled-task persistence was found.
- The executable uses an **asInvoker** manifest; admin rights are not requested by the app.
- Normal preview stays local. Build scripts download pinned public dependencies; the app itself does not automatically download updates.
- First-use auto-connect requires the known Live Gamer MINI USB vendor/product identity as well as its name. Otherwise the person selects capture inputs. Saved selections are local; there is no automatic fallback to an unrelated webcam/microphone.
- Logs/settings may contain sensitive local device identifiers. They are not automatically uploaded, and must be redacted before reporting an issue.
- Broad media/network capabilities exist inside upstream LibVLC, but this app opens its capture input and does not enable a remote-control interface or listener. This review does not prove the absence of all dependency vulnerabilities.

## Scan and integrity

- A local Microsoft Defender custom scan of the clean public binary folder completed with exit code 0 and reported no threats. Final release ZIP scanning and the privacy/package checks are recorded in the accompanying public review report.
- The release binary is **not Authenticode signed**. SHA-256 checksums provide download integrity, not a publisher signature or a guarantee of safety. Do not disable antivirus/SmartScreen protections to run it.
- Source and release packaging exclude tester captures, local settings/logs, hardware instance IDs, desktop shortcuts, credentials and PDB files. Upstream license attributions and publicly supplied upstream source fixtures are retained; those are not the tester's private data.

## Maintenance and reporting

NuGet dependencies are pinned with a lockfile and monitored by Dependabot. Review native VLC, .NET servicing and dependency advisories before each release; update and rerun synthetic/hardware checks as appropriate. There is no automatic updater in the app. No guaranteed security-response SLA is offered. See [SECURITY.md](../SECURITY.md) for safe reporting.

## ภาษาไทย

ชุด public ใช้ .NET 10.0.15 LTS จาก SDK 10.0.404 ที่ตรวจ SHA-512 แล้ว ไม่แจก .NET 8 เดิมที่หมด support การ build ผ่านและ self-test สังเคราะห์ 19 ข้อผ่านโดยไม่เปิดการ์ด ใช้ source path กลางและไม่รวม PDB

NuGet.org ไม่รายงานช่องโหว่ที่รู้จักสำหรับ managed packages สี่ตัว ณ วันที่ตรวจ ไม่ใช่การตรวจ native dependency ทุกตัวอย่างสมบูรณ์ ตรวจโค้ดไม่พบ telemetry/upload, listener, auto-update, เปิด process ภายนอก, ขอแอดมิน, autorun/service/scheduled task ตัวแอปใช้ manifest asInvoker และไม่สลับไปเว็บแคม/ไมโครโฟนส่วนตัวโดยเดาเอง

Defender สแกนโฟลเดอร์ไบนารีสะอาดแล้ว exit 0 และไม่พบภัยคุกคาม ผลสแกน ZIP และการตรวจข้อมูลส่วนตัวอยู่ในรายงานที่แนบ Release ไฟล์ไม่ได้ลงนามดิจิทัล SHA-256 ไม่ใช่ใบรับรองความปลอดภัย และไม่ควรปิด antivirus/SmartScreen เพื่อเปิดแอป

ไม่เผยค่าของผู้ทดสอบ log ตัวระบุอุปกรณ์เฉพาะเครื่อง ภาพ/เสียงส่วนตัว shortcut, credential หรือ PDB คง notices และ source fixtures สาธารณะของ upstream ไว้ตามสิทธิ์ ไม่ได้หมายถึงข้อมูลส่วนตัวของผู้ทดสอบ ยังไม่รับประกันว่าปลอดภัยทุกกรณี และต้องติดตามอัปเดต .NET/VLC/แพ็กเกจก่อนออกรุ่นต่อไป
